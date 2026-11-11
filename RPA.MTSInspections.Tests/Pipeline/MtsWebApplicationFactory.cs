using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using X.PagedList;

namespace RPA.MTSInspections.Tests.Pipeline
{
    /// <summary>
    /// Signs a request in from headers instead of contacting Entra: X-Test-User marks it authenticated and
    /// X-Test-Roles carries a comma-separated list of app-role values. No header means anonymous.
    /// </summary>
    internal sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public new const string Scheme = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Test-User"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim> { new("preferred_username", "Test.User@example.invalid") };

            foreach (var role in Request.Headers["X-Test-Roles"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim("roles", role.Trim()));
            }

            var identity = new ClaimsIdentity(claims, Scheme, "preferred_username", "roles");

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme)));
        }
    }

    internal sealed class StubPlantService : IPlantService
    {
        public List<Plant> GetPlant() => [];

        public PagedList<Plant> GetPlant(string searchString, int page, int pageSize, bool riskNeeded) => new([], 1, 15);

        public Plant GetPlantById(Guid plantId) => null;
    }

    /// <summary>
    /// Hosts the real application pipeline (routing, authorization, antiforgery, forwarding, probes) in
    /// Production mode, swapping only Entra sign-in for header-based test sign-in and the plant service for a stub.
    /// </summary>
    internal sealed class MtsWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");

            foreach (var (key, value) in Hosting.ApplicationServicesTests.ValidSettings())
            {
                builder.UseSetting(key, value);
            }

            builder.UseSetting("AllowedHosts", "localhost");

            // Nothing listens on port 1, so readiness fails immediately instead of waiting on a network timeout.
            builder.UseSetting("ConnectionStrings:MTSInspectionsContext",
                "Server=tcp:127.0.0.1,1;Database=D;User Id=u;Password=p;Encrypt=True;TrustServerCertificate=False");

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = TestAuthHandler.Scheme;
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                    options.DefaultForbidScheme = TestAuthHandler.Scheme;
                    options.DefaultSignInScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

                services.RemoveAll<IPlantService>();
                services.AddScoped<IPlantService, StubPlantService>();
            });
        }

        public HttpClient Client(string baseAddress = "https://localhost") =>
            CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri(baseAddress) });
    }
}
