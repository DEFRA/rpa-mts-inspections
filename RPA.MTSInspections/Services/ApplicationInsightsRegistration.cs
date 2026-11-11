using Azure.Identity;

namespace RPA.MTSInspections.Services;

public static class ApplicationInsightsRegistration
{
    /// <summary>
    /// Registers Application Insights when a connection string is configured, optionally authenticating ingestion
    /// with a user-assigned managed identity. Set OTEL_SERVICE_NAME to give the service a stable cloud role name.
    /// </summary>
    public static IServiceCollection AddMtsApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        // Prefer the Azure-standard environment variable; fall back only when it is absent.
        var connection = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] ??
                         configuration["ApplicationInsights:ConnectionString"];
        var identity = configuration["Telemetry:ManagedIdentityClientId"];

        // An explicitly empty identity setting is a configuration error, not an opt-out.
        var hasIdentity = identity != null || configuration.GetSection("Telemetry").GetChildren()
            .Any(child => child.Key.Equals("ManagedIdentityClientId", StringComparison.OrdinalIgnoreCase));

        var clientId = Guid.Empty;

        if (hasIdentity)
        {
            if (!Guid.TryParse(identity, out clientId) || clientId == Guid.Empty)
                throw new InvalidOperationException("Telemetry:ManagedIdentityClientId must identify the user-assigned managed identity.");

            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("Telemetry:ManagedIdentityClientId requires an Application Insights connection string.");
        }

        // No connection string means no SDK registration, so telemetry stays off.
        if (string.IsNullOrWhiteSpace(connection)) return services;

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = connection;

            // Without an identity, ingestion uses connection-string authentication (local auth must be enabled).
            if (hasIdentity)
                options.Credential = new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId.ToString()));
        });

        return services;
    }
}
