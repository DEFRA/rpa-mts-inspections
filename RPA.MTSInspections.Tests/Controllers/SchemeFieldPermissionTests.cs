using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using RPA.MTSInspections.Controllers;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.ViewModels;
using System.Data.Entity;

namespace RPA.MTSInspections.Tests.Controllers
{
    /// <summary>
    /// The scheme screens disable fields by role, but a browser can post anything. These tests post forged values
    /// straight to the actions and check the server, not the screen, decides what changes.
    /// </summary>
    [TestFixture]
    public class SchemeFieldPermissionTests
    {
        private static readonly Guid PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79");
        private static readonly Guid Existing = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Forged = Guid.Parse("99999999-9999-9999-9999-999999999999");
        private static readonly DateTime StoredInspection = new(2020, 1, 1);
        private static readonly DateTime ForgedInspection = new(2021, 6, 6);

        private sealed class Rig
        {
            public SchemeController Controller;
            public Mock<DbSet<PlantOption>> PlantOptions;
            public Mock<DbSet<LastInspection>> LastInspections;
            public Mock<IAuditService> Audit;
            public Mock<MTSInspectionsContext> Db;
        }

        private static Rig Build(string schemeName, params string[] roles)
        {
            var scheme = new Scheme { SchemeId = Guid.NewGuid(), Name = schemeName, Active = true };
            var plant = new Plant { PlantId = PlantId, PlantName = "Test plant", LicenceNo = "1" };

            var plants = new Mock<DbSet<Plant>>().SetupData([plant], _ => plant);
            var schemes = new Mock<DbSet<Scheme>>().SetupData([scheme], _ => scheme);
            var plantOptions = new Mock<DbSet<PlantOption>>().SetupData([]);
            var riskScores = new Mock<DbSet<RiskScore>>().SetupData([]);
            var lastInspections = new Mock<DbSet<LastInspection>>().SetupData(
                [new LastInspection { PlantId = PlantId, SchemeId = scheme.SchemeId, Date = StoredInspection }]);

            var db = new Mock<MTSInspectionsContext>();
            db.Setup(x => x.Plant).Returns(plants.Object);
            db.Setup(x => x.Scheme).Returns(schemes.Object);
            db.Setup(x => x.PlantOption).Returns(plantOptions.Object);
            db.Setup(x => x.RiskScore).Returns(riskScores.Object);
            db.Setup(x => x.LastInspection).Returns(lastInspections.Object);

            // Every criterion already has a stored option, so an unchanged post must change nothing.
            var service = new Mock<ISchemeService>();
            service.Setup(x => x.GetCurrentPlantOption(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns<Guid, Guid, string>((p, s, criteria) => Option(p, s, Existing, criteria));
            service.Setup(x => x.CreateNewPlantOption(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns<Guid, Guid, Guid>((p, s, option) => Option(p, s, option, "criteria"));

            var audit = new Mock<IAuditService>();

            var controller = new SchemeController(db.Object, service.Object, audit.Object, new FakeUser(), TestRoles.Access())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = TestRoles.Principal(roles) } }
            };
            controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());

            return new Rig { Controller = controller, PlantOptions = plantOptions, LastInspections = lastInspections, Audit = audit, Db = db };
        }

        private static PlantOption Option(Guid plantId, Guid schemeId, Guid optionId, string criteria) => new()
        {
            PlantId = plantId,
            SchemeId = schemeId,
            OptionId = optionId,
            Option = new Option { OptionId = optionId, Name = "option", Criteria = new Criteria { Name = criteria } }
        };

        private static SelectOption Unselected() => new() { Option = new Option { OptionId = Guid.NewGuid() }, Selected = false };

        private static BLSSchemeView Bls(Guid history, Guid other) => new()
        {
            Throughput = other,
            OperatingHours = Existing,
            PremisesType = Existing,
            TypeOfOperation = Existing,
            OriginofProducts = Existing,
            NumofInspFail = history,
            NumofNonComp = history,
            SevofNonComp = history,
            NumYrsSinceInsp = history,
            SchemeYear = history,
            OptionANoSplit = Unselected(),
            OptionBSplit = Unselected(),
            OptionCCatering = Unselected(),
            OptionDRetail = Unselected(),
            OptionESomeCut = Unselected(),
            OptionFWholesale = Unselected(),
            OptionGCutting = Unselected(),
            OptionHSlaughter = Unselected(),
            LastInspection = new LastInspection { Date = ForgedInspection }
        };

        private static BCCSchemeView Bcc(Guid history, Guid other) => new()
        {
            Throughput = other,
            OperatingHours = Existing,
            SignIn = Existing,
            RecordKeeping = Existing,
            LineClearing = Existing,
            CarcassHang = Existing,
            Trimming = Existing,
            HidePuller = Existing,
            CarcassDress = Existing,
            NumofWeeks = history,
            FailedVisit = history,
            FailedLastThree = history,
            MissLastThree = history,
            SchemeYear = history,
            LastInspection = new LastInspection { Date = ForgedInspection }
        };

        private static PCGSchemeView Pcg(Guid history, Guid other) => new()
        {
            Throughput = other,
            OperatingHours = Existing,
            SignIn = Existing,
            RecordKeeping = Existing,
            LineClearing = Existing,
            Probe = Existing,
            Trimming = Existing,
            CarcassDress = Existing,
            NumofWeeks = history,
            FailedVisit = history,
            FailedLastThree = history,
            MissLastThree = history,
            SchemeYear = history,
            LastInspection = new LastInspection { Date = ForgedInspection }
        };

        private static IActionResult Post(Rig rig, string scheme, Guid history, Guid other) => scheme switch
        {
            "BLS" => rig.Controller.BLSSchemePost(Bls(history, other), PlantId.ToString()),
            "BCC" => rig.Controller.BCCSchemePost(Bcc(history, other), PlantId.ToString()),
            _ => rig.Controller.PCGSchemePost(Pcg(history, other), PlantId.ToString())
        };

        private static void AssertSaved(IActionResult result)
        {
            var redirect = result as RedirectToActionResult;

            Assert.IsNotNull(redirect, "expected a redirect after a successful save");
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("Plant", redirect.ControllerName);
        }

        [TestCase("BLS")]
        [TestCase("BCC")]
        [TestCase("PCG")]
        public void An_inspector_cannot_change_the_inspection_history_fields(string scheme)
        {
            var rig = Build(scheme, TestRoles.Inspector);

            var result = Post(rig, scheme, history: Forged, other: Existing);

            AssertSaved(result);
            rig.PlantOptions.Verify(x => x.Remove(It.IsAny<PlantOption>()), Times.Never);
            rig.PlantOptions.Verify(x => x.Add(It.IsAny<PlantOption>()), Times.Never);
        }

        [TestCase("BLS")]
        [TestCase("BCC")]
        [TestCase("PCG")]
        public void An_admin_can_change_the_inspection_history_fields(string scheme)
        {
            var rig = Build(scheme, TestRoles.Admin);

            var result = Post(rig, scheme, history: Forged, other: Existing);

            AssertSaved(result);
            rig.PlantOptions.Verify(x => x.Remove(It.IsAny<PlantOption>()), Times.Exactly(5));
            rig.PlantOptions.Verify(x => x.Add(It.IsAny<PlantOption>()), Times.Exactly(5));
        }

        [TestCase("BLS")]
        [TestCase("BCC")]
        [TestCase("PCG")]
        public void An_inspector_can_still_change_the_fields_they_own(string scheme)
        {
            var rig = Build(scheme, TestRoles.Inspector);

            var result = Post(rig, scheme, history: Existing, other: Forged);

            AssertSaved(result);
            rig.PlantOptions.Verify(x => x.Remove(It.IsAny<PlantOption>()), Times.Once);
            rig.PlantOptions.Verify(x => x.Add(It.IsAny<PlantOption>()), Times.Once);
        }

        [TestCase("BLS")]
        [TestCase("BCC")]
        [TestCase("PCG")]
        public void Only_an_admin_can_change_the_last_inspection_date(string scheme)
        {
            var inspector = Build(scheme, TestRoles.Inspector);
            var admin = Build(scheme, TestRoles.Admin);

            Post(inspector, scheme, history: Existing, other: Existing);
            Post(admin, scheme, history: Existing, other: Existing);

            inspector.LastInspections.Verify(x => x.Remove(It.IsAny<LastInspection>()), Times.Never);
            inspector.LastInspections.Verify(x => x.Add(It.IsAny<LastInspection>()), Times.Never);
            admin.LastInspections.Verify(x => x.Remove(It.IsAny<LastInspection>()), Times.Once);
            admin.LastInspections.Verify(x => x.Add(It.IsAny<LastInspection>()), Times.Once);
        }

        [Test]
        public void A_missing_last_inspection_in_the_post_is_ignored_for_an_admin_instead_of_failing()
        {
            var rig = Build("BCC", TestRoles.Admin);
            var form = Bcc(Existing, Existing);
            form.LastInspection = null;

            var result = rig.Controller.BCCSchemePost(form, PlantId.ToString());

            AssertSaved(result);
            rig.LastInspections.Verify(x => x.Remove(It.IsAny<LastInspection>()), Times.Never);
        }

        [Test]
        public void Audit_rows_are_written_as_the_signed_in_user()
        {
            var rig = Build("BCC", TestRoles.Admin);

            Post(rig, "BCC", history: Forged, other: Existing);

            rig.Audit.Verify(x => x.Log(PlantId, "test.user", "BCC", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            rig.Audit.Verify(x => x.Log(It.IsAny<Guid>(), It.Is<string>(user => user != "test.user"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Saving_a_scheme_recalculates_its_score_in_the_same_request()
        {
            var rig = Build("BCC", TestRoles.Admin);

            Post(rig, "BCC", history: Existing, other: Existing);

            // One save for the edit, one for the recalculated risk score.
            rig.Db.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }
    }
}
