using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RPA.MTSInspections.DAL;

namespace RPA.MTSInspections.Services;

/// <summary>
/// One-off schema utilities for the DBA or a release pipeline, run instead of the web host:
/// <c>--script &lt;file&gt;</c> writes the idempotent SQL of every migration; <c>--migrate</c> applies them to the configured database.
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

        var options = new DbContextOptionsBuilder<MTSInspectionsContext>().UseSqlServer(connectionString).Options;
        using var context = new MTSInspectionsContext(options);

        if (args[0] == "--script")
        {
            if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                output.WriteLine("Usage: --script <output file>");
                return 2;
            }

            // Idempotent script of the whole history, so the DBA can review it and run it against any database state.
            var sql = context.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
            File.WriteAllText(args[1], sql);
            output.WriteLine($"Wrote {sql.Length} characters of SQL to {args[1]}.");

            return 0;
        }

        if (args.Length != 1)
        {
            output.WriteLine("Usage: --migrate");
            return 2;
        }

        var pending = context.Database.GetPendingMigrations().ToList();
        context.Database.Migrate();
        output.WriteLine($"Applied {pending.Count} migration(s).");

        return 0;
    }
}
