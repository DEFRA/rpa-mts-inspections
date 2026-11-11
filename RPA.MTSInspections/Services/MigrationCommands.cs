using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using RPA.MTSInspections.Migrations;

namespace RPA.MTSInspections.Services;

/// <summary>
/// One-off schema utilities for the DBA or a release pipeline, run instead of the web host:
/// <c>--script &lt;file&gt;</c> writes the SQL of every migration; <c>--migrate</c> applies them to the configured database.
/// The web application itself never creates or changes the schema.
/// </summary>
public static class MigrationCommands
{
    public static bool IsCommand(string[] args) => args.Length > 0 && (args[0] == "--script" || args[0] == "--migrate");

    public static int Run(string[] args, string connectionString, TextWriter output)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            output.WriteLine("ConnectionStrings:MTSInspectionsContext is required.");
            return 2;
        }

        var configuration = new Configuration(connectionString);

        if (args[0] == "--script")
        {
            if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                output.WriteLine("Usage: --script <output file>");
                return 2;
            }

            // Scripts the whole history from an empty database, so the DBA can review and apply it.
            var sql = new MigratorScriptingDecorator(new DbMigrator(configuration)).ScriptUpdate(null, null);
            File.WriteAllText(args[1], sql);
            output.WriteLine($"Wrote {sql.Length} characters of SQL to {args[1]}.");

            return 0;
        }

        if (args.Length != 1)
        {
            output.WriteLine("Usage: --migrate");
            return 2;
        }

        var migrator = new DbMigrator(configuration);
        var pending = migrator.GetPendingMigrations().ToList();
        migrator.Update();
        output.WriteLine($"Applied {pending.Count} migration(s).");

        return 0;
    }
}
