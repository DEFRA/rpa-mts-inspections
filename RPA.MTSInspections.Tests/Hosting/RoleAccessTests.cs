using NUnit.Framework;

namespace RPA.MTSInspections.Tests.Hosting
{
    [TestFixture]
    public class RoleAccessTests
    {
        private readonly RPA.MTSInspections.Services.RoleAccess access = TestRoles.Access();

        [Test]
        public void Each_role_maps_to_its_own_capability()
        {
            var admin = TestRoles.Principal(TestRoles.Admin);
            var support = TestRoles.Principal(TestRoles.Support);
            var inspector = TestRoles.Principal(TestRoles.Inspector);
            var basic = TestRoles.Principal(TestRoles.Basic);
            var none = TestRoles.Principal();

            Assert.IsTrue(access.IsAdmin(admin));
            Assert.IsFalse(access.IsAdmin(support));

            Assert.IsTrue(access.IsSupportOrAdmin(admin));
            Assert.IsTrue(access.IsSupportOrAdmin(support));
            Assert.IsFalse(access.IsSupportOrAdmin(inspector));
            Assert.IsFalse(access.IsSupportOrAdmin(basic));

            Assert.IsTrue(access.CanEditScheme(admin));
            Assert.IsTrue(access.CanEditScheme(inspector));
            Assert.IsFalse(access.CanEditScheme(support));
            Assert.IsFalse(access.CanEditScheme(basic));

            Assert.IsTrue(access.CanEditScc(basic));
            Assert.IsTrue(access.CanEditScc(support));
            Assert.IsTrue(access.CanEditScc(inspector));
            Assert.IsTrue(access.CanEditScc(admin));
            Assert.IsFalse(access.CanEditScc(none));
        }

        [Test]
        public void The_former_idt_role_labels_grant_nothing()
        {
            var legacy = TestRoles.Principal("MTS Risk Analysis : Admin User", "MTS Risk Analysis: Basic Access");

            Assert.IsFalse(access.IsAdmin(legacy));
            Assert.IsFalse(access.CanEditScc(legacy));
        }
    }
}
