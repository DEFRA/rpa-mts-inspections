using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RPA.MTSInspections.Services;

/// <summary>
/// Readiness probe: a bounded SELECT 1 against the application database. It does not check tables, grants or Entra.
/// </summary>
internal sealed class SqlReadinessHealthCheck(IConfiguration configuration, ILogger<SqlReadinessHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new SqlConnectionStringBuilder(configuration.GetConnectionString("MTSInspectionsContext"))
            {
                ConnectTimeout = 3
            };

            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 3;

            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1
                ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException or OperationCanceledException or ArgumentException)
        {
            // SQL exceptions can contain server, database and identity information.
            logger.LogWarning(new EventId(1002, "SqlReadinessFailed"), "SQL readiness check failed.");

            return HealthCheckResult.Unhealthy();
        }
    }
}
