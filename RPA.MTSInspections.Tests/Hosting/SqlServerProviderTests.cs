using Microsoft.Data.SqlClient;
using NUnit.Framework;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Migrations;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using System.Data.Entity.Migrations;

namespace RPA.MTSInspections.Tests.DAL
{
    /// <summary>
    /// Runs the EF6 model, the existing migrations and the Microsoft.Data.SqlClient provider against a real SQL
    /// Server (LocalDB, a scratch database that is dropped afterwards). Skipped when LocalDB is not available.
    /// It proves the provider path works; it cannot prove Azure SQL managed-identity access.
    /// </summary>
    [TestFixture]
    [Category("SqlServer")]
    public class SqlServerProviderTests
    {
        private const string Server = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=15";

        private string database;
        private string connection;

        private static void Run(string connectionString, string sql)
        {
            using var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        [OneTimeSetUp]
        public void CreateScratchDatabase()
        {
            database = "MtsTest_" + Guid.NewGuid().ToString("N");
            connection = $"{Server};Database={database};MultipleActiveResultSets=True";

            try
            {
                Run($"{Server};Database=master", $"CREATE DATABASE [{database}]");
            }
            catch (SqlException exception)
            {
                Assert.Ignore("LocalDB is not available: " + exception.Message);
            }

            new DbMigrator(new Configuration(connection)).Update();
        }

        [OneTimeTearDown]
        public void DropScratchDatabase()
        {
            if (database == null) return;

            SqlConnection.ClearAllPools();

            try
            {
                Run($"{Server};Database=master",
                    $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END");
            }
            catch (SqlException)
            {
                // Best-effort clean-up of a scratch database.
            }
        }

        [Test]
        public void The_existing_migrations_apply_and_leave_nothing_pending()
        {
            Assert.IsEmpty(new DbMigrator(new Configuration(connection)).GetPendingMigrations());
        }

        [Test]
        public void The_orphan_migration_stays_excluded()
        {
            var applied = new DbMigrator(new Configuration(connection)).GetDatabaseMigrations().ToList();

            Assert.IsFalse(applied.Any(name => name.Contains("201803161004159")));
            Assert.IsTrue(applied.Any(name => name.Contains("AddSccScheme")));
        }

        [Test]
        public void Data_round_trips_through_the_provider()
        {
            var plantId = Guid.NewGuid();
            var schemeId = Guid.NewGuid();

            using (var db = new MTSInspectionsContext(connection))
            {
                db.Scheme.Add(new Scheme { SchemeId = schemeId, Name = "TEST", Active = true, Weighting = 1 });
                db.Plant.Add(new Plant { PlantId = plantId, PlantName = "Provider test plant", LicenceNo = "T-1", Active = true, SCC = true });
                db.Audit.Add(new Audit(plantId, "test.user", "TEST", "Field", "old", "new"));
                db.SaveChanges();
            }

            using (var db = new MTSInspectionsContext(connection))
            {
                var plant = db.Plant.Single(x => x.PlantId == plantId);
                var audit = db.Audit.Single(x => x.PlantId == plantId);

                Assert.AreEqual("Provider test plant", plant.PlantName);
                Assert.IsTrue(plant.SCC);
                Assert.AreEqual("test.user", audit.User);
                Assert.AreEqual(DateTimeKind.Unspecified, audit.Date.Kind);
                Assert.IsTrue(db.Scheme.Any(x => x.SchemeId == schemeId));
            }
        }

        [Test]
        public void The_generated_schema_script_builds_the_same_schema_as_the_migrations()
        {
            var scriptDb = "MtsScript_" + Guid.NewGuid().ToString("N");
            var scriptFile = Path.Combine(Path.GetTempPath(), scriptDb + ".sql");
            var scratch = $"{Server};Database={scriptDb};MultipleActiveResultSets=True";

            try
            {
                Run($"{Server};Database=master", $"CREATE DATABASE [{scriptDb}]");
                var output = new StringWriter();

                // The script is produced from an empty database, then applied to another empty one.
                Assert.AreEqual(0, MigrationCommands.Run(["--script", scriptFile], scratch, output), output.ToString());

                var script = File.ReadAllText(scriptFile);
                StringAssert.Contains("CREATE TABLE [dbo].[Plants]", script);
                StringAssert.Contains("[__MigrationHistory]", script);

                foreach (var batch in System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline))
                {
                    if (!string.IsNullOrWhiteSpace(batch)) Run(scratch, batch);
                }

                Assert.AreEqual(Columns(connection), Columns(scratch));
                Assert.IsEmpty(new DbMigrator(new Configuration(scratch)).GetPendingMigrations());
            }
            finally
            {
                SqlConnection.ClearAllPools();
                if (File.Exists(scriptFile)) File.Delete(scriptFile);
                Run($"{Server};Database=master",
                    $"IF DB_ID('{scriptDb}') IS NOT NULL BEGIN ALTER DATABASE [{scriptDb}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{scriptDb}]; END");
            }
        }

        [Test]
        public void The_migrate_command_applies_pending_migrations_and_is_repeatable()
        {
            var migrateDb = "MtsMigrate_" + Guid.NewGuid().ToString("N");
            var target = $"{Server};Database={migrateDb};MultipleActiveResultSets=True";

            try
            {
                Run($"{Server};Database=master", $"CREATE DATABASE [{migrateDb}]");
                var first = new StringWriter();
                var second = new StringWriter();

                Assert.AreEqual(0, MigrationCommands.Run(["--migrate"], target, first));
                Assert.AreEqual(0, MigrationCommands.Run(["--migrate"], target, second));

                StringAssert.DoesNotContain("Applied 0", first.ToString());
                StringAssert.Contains("Applied 0 migration(s)", second.ToString());
                Assert.AreEqual(Columns(connection), Columns(target));
            }
            finally
            {
                SqlConnection.ClearAllPools();
                Run($"{Server};Database=master",
                    $"IF DB_ID('{migrateDb}') IS NOT NULL BEGIN ALTER DATABASE [{migrateDb}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{migrateDb}]; END");
            }
        }

        [TestCase("--script")]
        [TestCase("--script", "")]
        [TestCase("--migrate", "extra")]
        public void Bad_command_arguments_are_rejected_without_touching_a_database(params string[] args)
        {
            var output = new StringWriter();

            Assert.AreEqual(2, MigrationCommands.Run(args, $"{Server};Database=NeverCreated", output));
            StringAssert.Contains("Usage", output.ToString());
        }

        [Test]
        public void A_missing_connection_string_is_rejected()
        {
            Assert.AreEqual(2, MigrationCommands.Run(["--migrate"], null, new StringWriter()));
        }

        [Test]
        public void Only_the_two_utility_commands_are_recognised()
        {
            Assert.IsTrue(MigrationCommands.IsCommand(["--script", "x.sql"]));
            Assert.IsTrue(MigrationCommands.IsCommand(["--migrate"]));
            Assert.IsFalse(MigrationCommands.IsCommand([]));
            Assert.IsFalse(MigrationCommands.IsCommand(["--urls", "https://localhost"]));
        }

        private static string Columns(string connectionString)
        {
            using var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using var command = sqlConnection.CreateCommand();
            command.CommandText =
                "SELECT t.name + '.' + c.name + ':' + ty.name + ':' + CAST(c.max_length AS varchar) + ':' + CAST(c.is_nullable AS varchar) " +
                "FROM sys.tables t JOIN sys.columns c ON c.object_id = t.object_id JOIN sys.types ty ON ty.user_type_id = c.user_type_id " +
                "WHERE t.name <> '__MigrationHistory' ORDER BY t.name, c.name";

            var rows = new List<string>();
            using var reader = command.ExecuteReader();
            while (reader.Read()) rows.Add(reader.GetString(0));

            return string.Join("\n", rows);
        }

        [Test]
        public void A_missing_database_is_never_created_by_the_runtime()
        {
            var missing = "MtsMissing_" + Guid.NewGuid().ToString("N");

            using (var db = new MTSInspectionsContext($"{Server};Database={missing}"))
            {
                Assert.Catch(() => db.Plant.ToList());
            }

            SqlConnection.ClearAllPools();

            using var master = new SqlConnection($"{Server};Database=master");
            master.Open();
            using var command = master.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{missing}'";

            Assert.AreEqual(0, (int)command.ExecuteScalar());
        }
    }
}
