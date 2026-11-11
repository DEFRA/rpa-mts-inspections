using System.Security.Claims;
using NUnit.Framework;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Tests.Hosting
{
    [TestFixture]
    public class AuditIdentityTests
    {
        private static ClaimsPrincipal Principal(bool authenticated, params Claim[] claims) =>
            new(authenticated ? new ClaimsIdentity(claims, "Test") : new ClaimsIdentity(claims));

        [Test]
        public void Returns_the_lower_case_configured_claim()
        {
            var principal = Principal(true, new Claim("preferred_username", "Operator@Example.invalid"));

            Assert.AreEqual("operator@example.invalid", AuditIdentity.GetName(principal, "preferred_username"));
        }

        [Test]
        public void Keeps_the_legacy_account_formatting()
        {
            var principal = Principal(true, new Claim("upn", "EARTH\\M01234"));

            Assert.AreEqual("m1234", AuditIdentity.GetName(principal, "upn"));
        }

        [Test]
        public void Does_not_fall_back_to_identity_name()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "someone")], "Test");

            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(new ClaimsPrincipal(identity), "preferred_username"));
        }

        [Test]
        public void A_duplicated_claim_is_rejected()
        {
            var principal = Principal(true, new Claim("preferred_username", "a@x.invalid"), new Claim("preferred_username", "b@x.invalid"));

            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(principal, "preferred_username"));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("earth\\")]
        public void A_blank_claim_is_rejected(string value)
        {
            var principal = Principal(true, new Claim("preferred_username", value));

            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(principal, "preferred_username"));
        }

        [Test]
        public void An_unauthenticated_principal_is_rejected()
        {
            var principal = Principal(false, new Claim("preferred_username", "a@x.invalid"));

            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(principal, "preferred_username"));
        }

        [Test]
        public void A_missing_principal_or_claim_type_is_rejected()
        {
            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(null, "preferred_username"));
            Assert.Throws<InvalidOperationException>(() => AuditIdentity.GetName(Principal(true), " "));
        }
    }
}
