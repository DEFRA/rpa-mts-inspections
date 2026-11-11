using System.Security.Claims;
using Microsoft.Extensions.Options;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Tests
{
    /// <summary>The audit user for unit tests that run without an HTTP request.</summary>
    internal sealed class FakeUser : IUser
    {
        public string Name => "test.user";
    }

    internal static class TestRoles
    {
        public const string Basic = "MTS.BasicAccess";
        public const string Support = "MTS.SupportTeam";
        public const string Inspector = "MTS.Inspector";
        public const string Admin = "MTS.Admin";

        public static RoleAccess Access() => new(Options.Create(new MtsRoleOptions
        {
            BasicAccess = Basic,
            SupportTeam = Support,
            Inspector = Inspector,
            Admin = Admin
        }));

        /// <summary>A signed-in principal whose role claim type matches the production OIDC configuration.</summary>
        public static ClaimsPrincipal Principal(params string[] roles)
        {
            var claims = new List<Claim> { new("preferred_username", "Test.User@example.invalid") };
            claims.AddRange(roles.Select(role => new Claim("roles", role)));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test", "preferred_username", "roles"));
        }
    }
}
