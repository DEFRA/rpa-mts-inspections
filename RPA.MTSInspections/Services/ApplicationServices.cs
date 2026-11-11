using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Services;

public sealed class UploadOptions
{
    /// <summary>Largest accepted bulk-load workbook, in bytes.</summary>
    public long MaxFileBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Largest number of data rows accepted on any one worksheet.</summary>
    public int MaxRowsPerSheet { get; set; } = 10000;
}

public static class ApplicationServices
{
    private sealed record Settings(Guid TenantId, MtsRoleOptions Roles, string AuditClaim, string ConnectionString, int HttpsPort);

    /// <summary>
    /// Registers authentication, authorization, data access and runtime services. Required external settings are
    /// validated here so a misconfigured deployment fails at start-up instead of running unprotected.
    /// </summary>
    public static IServiceCollection AddMtsApplication(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment = null)
    {
        var settings = ValidateConfiguration(configuration, environment);

        services.Configure<AuditIdentityOptions>(options => options.AuditNameClaimType = settings.AuditClaim);
        services.Configure<MtsRoleOptions>(options =>
        {
            options.BasicAccess = settings.Roles.BasicAccess;
            options.SupportTeam = settings.Roles.SupportTeam;
            options.Inspector = settings.Roles.Inspector;
            options.Admin = settings.Roles.Admin;
        });
        services.Configure<UploadOptions>(configuration.GetSection("Upload"));
        services.AddSingleton<RoleAccess>();
        services.AddMtsDataProtection(configuration);

        AddIdentity(services, configuration, settings);

        services.AddMtsForwarding(configuration);
        services.AddHttpsRedirection(options => options.HttpsPort = settings.HttpsPort);
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();

        AddData(services, settings.ConnectionString);

        // Every unsafe-verb request needs an antiforgery token; forms render one automatically.
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

        services.AddMtsApplicationInsights(configuration);

        return services;
    }

    private static Settings ValidateConfiguration(IConfiguration configuration, IHostEnvironment environment)
    {
        var tenant = Required(configuration, "AzureAd:TenantId");

        if (!Guid.TryParse(tenant, out var tenantId) || tenantId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:TenantId must identify the configured tenant, not a shared or wildcard authority.");

        if (!Guid.TryParse(Required(configuration, "AzureAd:ClientId"), out var clientId) || clientId == Guid.Empty)
            throw new InvalidOperationException("AzureAd:ClientId must be an application registration ID.");

        if (!Uri.TryCreate(Required(configuration, "AzureAd:Instance"), UriKind.Absolute, out var instance) ||
            instance.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(instance.UserInfo) ||
            !string.IsNullOrEmpty(instance.Query) || !string.IsNullOrEmpty(instance.Fragment))
            throw new InvalidOperationException("AzureAd:Instance must be the externally approved HTTPS identity authority.");

        // Supplied by an ACA Key Vault secret reference in production or by user secrets in development.
        Required(configuration, "AzureAd:ClientSecret");

        var roles = new MtsRoleOptions
        {
            BasicAccess = Role(configuration, "BasicAccess"),
            SupportTeam = Role(configuration, "SupportTeam"),
            Inspector = Role(configuration, "Inspector"),
            Admin = Role(configuration, "Admin")
        };

        if (roles.All.Distinct(StringComparer.OrdinalIgnoreCase).Count() != roles.All.Length)
            throw new InvalidOperationException("Authorization:Roles must define four different app-role values.");

        var auditClaim = Required(configuration, "Identity:AuditNameClaimType");

        if (auditClaim.Any(char.IsWhiteSpace))
            throw new InvalidOperationException("Identity:AuditNameClaimType must be an explicit claim type without whitespace.");

        var connectionString = Required(configuration, "ConnectionStrings:MTSInspectionsContext");
        ValidateSqlConnection(connectionString, environment);

        var allowedHosts = Required(configuration, "AllowedHosts").Split(';');
        if (allowedHosts.Any(host => host.Contains('*') || Uri.CheckHostName(host) == UriHostNameType.Unknown))
            throw new InvalidOperationException("AllowedHosts must contain explicit hostnames separated by semicolons, without wildcards, schemes, paths, ports or empty entries.");

        if (!int.TryParse(configuration["HttpsRedirection:HttpsPort"] ?? "443", out var httpsPort) ||
            httpsPort is < 1 or > 65535)
            throw new InvalidOperationException("HttpsRedirection:HttpsPort must be a valid HTTPS port.");

        if (!string.IsNullOrWhiteSpace(configuration["ConnectionStrings:IDTSecurityConnection"]))
            throw new InvalidOperationException("ConnectionStrings:IDTSecurityConnection is obsolete: roles now come from Entra app roles. Remove the setting.");

        var upload = configuration.GetSection("Upload").Get<UploadOptions>() ?? new UploadOptions();
        if (upload.MaxFileBytes < 1 || upload.MaxRowsPerSheet < 1)
            throw new InvalidOperationException("Upload:MaxFileBytes and Upload:MaxRowsPerSheet must be positive.");

        return new Settings(tenantId, roles, auditClaim, connectionString, httpsPort);
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration, Settings settings)
    {
        var tenantId = settings.TenantId;
        var auditClaim = settings.AuditClaim;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            // The sign-in-only registration defaults to id_token; keep the authorization code flow.
            options.ResponseType = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.Code;
            options.MapInboundClaims = false;
            // Microsoft.Identity.Web replaces the handler independently of the OIDC claim-mapping setter.
            if (options.TokenHandler is Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler tokenHandler)
                tokenHandler.MapInboundClaims = false;
            options.TokenValidationParameters.RoleClaimType = "roles";
            options.TokenValidationParameters.NameClaimType = auditClaim;

            var previous = options.Events.OnTokenValidated;
            options.Events.OnTokenValidated = async context =>
            {
                if (previous != null) await previous(context);
                if (context.Result?.Failure != null) return;
                var tenants = context.Principal?.FindAll("tid").ToArray() ?? [];
                if (tenants.Length != 1 || !Guid.TryParse(tenants[0].Value, out var actualTenant) || actualTenant != tenantId)
                {
                    LogAuthenticationFailure(context.HttpContext, "TENANT_CONTRACT_REJECTED");
                    context.Fail("The identity does not belong to the configured tenant.");
                    return;
                }

                try
                {
                    AuditIdentity.GetName(context.Principal, auditClaim);
                }
                catch (InvalidOperationException)
                {
                    LogAuthenticationFailure(context.HttpContext, "AUDIT_CONTRACT_REJECTED");
                    context.Fail("The identity is missing its required audit-name contract.");
                }
            };

            var previousRemoteFailure = options.Events.OnRemoteFailure;
            options.Events.OnRemoteFailure = async context =>
            {
                LogAuthenticationFailure(context.HttpContext, "OIDC_REMOTE_FAILURE");
                if (previousRemoteFailure != null) await previousRemoteFailure(context);
                if (context.Result?.Handled == true || context.Result?.Skipped == true) return;
                context.HandleResponse();
                context.Response.Redirect("/Account/AuthenticationFailed");
            };
        });

        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "__Host-Mts.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        });

        var roles = settings.Roles;

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.AddPolicy(MtsPolicies.ReadAccess, policy => policy.RequireAuthenticatedUser().RequireRole(roles.All));
            options.AddPolicy(MtsPolicies.SupportOrAdmin, policy => policy.RequireAuthenticatedUser().RequireRole(roles.SupportTeam, roles.Admin));
            options.AddPolicy(MtsPolicies.AdminOnly, policy => policy.RequireAuthenticatedUser().RequireRole(roles.Admin));
            options.AddPolicy(MtsPolicies.SchemeEdit, policy => policy.RequireAuthenticatedUser().RequireRole(roles.Inspector, roles.Admin));
            options.AddPolicy(MtsPolicies.SccEdit, policy => policy.RequireAuthenticatedUser().RequireRole(roles.All));
        });
    }

    private static void AddData(IServiceCollection services, string connectionString)
    {
        services.AddScoped(_ => new MTSInspectionsContext(connectionString));
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICriteriaService, CriteriaService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<ISchemeService, SchemeService>();
        services.AddHealthChecks().AddCheck<SqlReadinessHealthCheck>("sql", tags: ["ready"]);
    }

    private static void LogAuthenticationFailure(HttpContext context, string failureCode) =>
        context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RPA.MTSInspections.Authentication")
            .LogWarning(new EventId(1001, "AuthenticationRejected"),
                "Authentication rejected. FailureCode: {FailureCode}", failureCode);

    private static string Required(IConfiguration configuration, string key) =>
        !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key] :
            throw new InvalidOperationException($"Required external configuration '{key}' is missing.");

    private static string Role(IConfiguration configuration, string name)
    {
        var role = Required(configuration, $"Authorization:Roles:{name}");

        if (role.Length > 120 || role.Any(c => !IsAppRoleCharacter(c)) || role[0] == '.')
            throw new InvalidOperationException($"Authorization:Roles:{name} must be an Entra app-role value, not a display name.");

        return role;
    }

    private static bool IsAppRoleCharacter(char c) =>
        char.IsAsciiLetterOrDigit(c) || ".-_!#$%&'()*+,/:;<=>?@[]^`{|}~".Contains(c);

    private static void ValidateSqlConnection(string connectionString, IHostEnvironment environment)
    {
        SqlConnectionStringBuilder sql;

        try
        {
            sql = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("ConnectionStrings:MTSInspectionsContext is not a valid Microsoft.Data.SqlClient connection string.");
        }

        if (environment?.IsDevelopment() == true) return;

        // A Linux container has no Windows identity to delegate, and certificate validation must stay on.
        if (sql.IntegratedSecurity)
            throw new InvalidOperationException("ConnectionStrings:MTSInspectionsContext must not use Integrated Security outside Development; use a managed identity.");

        if (sql.TrustServerCertificate)
            throw new InvalidOperationException("ConnectionStrings:MTSInspectionsContext must not set TrustServerCertificate outside Development.");
    }
}
