using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Options;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL;
using System.Data.Entity;
using System.Globalization;

namespace RPA.MTSInspections.Tests.SL
{
    /// <summary>
    /// Freezes the exact text of all 16 CSV exports for a fixed synthetic data set (the "golden" files in
    /// Golden/). A difference means a downstream consumer would see different output: review it, and only
    /// regenerate the files deliberately with the explicit Regenerate_golden_files test.
    /// </summary>
    [TestFixture]
    public class ExportCharacterizationTests
    {
        private static readonly Guid Bls = Guid.Parse("00000000-0000-0000-0000-0000000000b1");
        private static readonly Guid Bcc = Guid.Parse("00000000-0000-0000-0000-0000000000b2");
        private static readonly Guid Pcg = Guid.Parse("00000000-0000-0000-0000-0000000000b3");
        private static readonly Guid Scc = Guid.Parse("00000000-0000-0000-0000-0000000000b4");

        private static readonly string[] BlsCriteria =
        [
            "Scheme Year", "Number of Months Since Last Inspection", "Number of Inspections Resulting in Failure",
            "Number of Non-Compliance", "Severity of Non-Compliance", "Throughput", "Operating Hours", "Premises Type",
            "Type of Operation", "Origin of Products Handled", "Nature of Operation"
        ];

        private static readonly string[] BccCriteria =
        [
            "Scheme Year", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit",
            "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput",
            "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Line Clearing", "Record Keeping", "Trimming",
            "Carcass Hanging Method", "Hide Puller"
        ];

        private static readonly string[] PcgCriteria =
        [
            "Scheme Year", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit",
            "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput",
            "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Line Clearing", "Record Keeping", "Trimming", "Probe"
        ];

        private static readonly string[] SccCriteria =
        [
            "Scheme Year", "Throughput", "Operating hours", "Sign in prior to inspection", "Trimming", "Skin Removal",
            "Carcass dressing", "Head Removal", "Feet Removal", "Failed / Unsatisfactory",
            "How many fails / unsatisfactory in previous 12 Months", "How many near misses in previous 12 Months",
            "Number of Enforcement Notices Issued in Last 12 Months", "Number of Penalty Notices Issued in Last 12 Months"
        ];

        private static Plant BuildPlant(int number, Guid scheme, string[] criteria, bool active, string name)
        {
            var plant = new Plant
            {
                PlantId = Guid.Parse($"00000000-0000-0000-0000-{number:D12}"),
                PlantName = name,
                LicenceNo = $"L{number:D3}",
                AddressOne = $"{number} High Street",
                AddressTwo = number % 2 == 0 ? null : "Industrial Estate",
                TownCity = "Testtown",
                County = "Testshire",
                PostCode = "TE5 7AB",
                Active = active,
                BLS = scheme == Bls,
                BCC = scheme == Bcc,
                PCG = scheme == Pcg,
                SCC = scheme == Scc,
                PlantOptions = [],
                RiskScore = [new RiskScore { PlantId = Guid.Empty, SchemeId = scheme, Score = 100 + number, DateCalculated = new DateTime(2020, 1, 1) }],
                LastInspection = [new LastInspection { PlantId = Guid.Empty, SchemeId = scheme, Date = new DateTime(2020, 1, 31) }]
            };

            var schemeEntity = new Scheme { SchemeId = scheme, Name = scheme == Bls ? "BLS" : scheme == Bcc ? "BCC" : scheme == Pcg ? "PCG" : "SCC" };

            foreach (var criterion in criteria)
            {
                var option = new Option
                {
                    OptionId = Guid.NewGuid(),
                    Name = criterion == "Scheme Year" ? "2019" : $"{criterion} option",
                    Active = true,
                    Criteria = new Criteria { Name = criterion, SchemeId = scheme, Scheme = schemeEntity }
                };

                plant.PlantOptions.Add(new PlantOption { PlantId = plant.PlantId, SchemeId = scheme, Scheme = schemeEntity, Option = option, OptionId = option.OptionId });
            }

            return plant;
        }

        private static ExportService BuildService()
        {
            var plants = new List<Plant>
            {
                BuildPlant(1, Bls, BlsCriteria, true, "Alpha, Meats Ltd"),
                BuildPlant(2, Bls, BlsCriteria, false, "Bravo Abattoir"),
                BuildPlant(3, Bcc, BccCriteria, true, "Charlie Cutting"),
                BuildPlant(4, Bcc, BccCriteria, false, "Delta Cutting"),
                BuildPlant(5, Pcg, PcgCriteria, true, "Echo Poultry"),
                BuildPlant(6, Pcg, PcgCriteria, false, "Foxtrot Poultry"),
                BuildPlant(7, Scc, SccCriteria, true, "Golf Slaughter"),
                BuildPlant(8, Scc, SccCriteria, false, "Hotel Slaughter")
            };

            var schemes = new List<Scheme>
            {
                new() { SchemeId = Bls, Name = "BLS", Active = true },
                new() { SchemeId = Bcc, Name = "BCC", Active = true },
                new() { SchemeId = Pcg, Name = "PCG", Active = true },
                new() { SchemeId = Scc, Name = "SCC", Active = true }
            };

            var natureOfOperation = new Criteria { Name = "Nature of Operation", SchemeId = Bls };
            var options = new List<Option>
            {
                new() { OptionId = Guid.NewGuid(), Name = "Catering", Criteria = natureOfOperation },
                new() { OptionId = Guid.NewGuid(), Name = "Wholesale", Criteria = natureOfOperation }
            };

            var db = new Mock<MTSInspectionsContext>();
            db.Setup(x => x.Plant).Returns(new Mock<DbSet<Plant>>().SetupData(plants).Object);
            db.Setup(x => x.Scheme).Returns(new Mock<DbSet<Scheme>>().SetupData(schemes).Object);
            db.Setup(x => x.Option).Returns(new Mock<DbSet<Option>>().SetupData(options).Object);

            return new ExportService(db.Object, Mock.Of<ISchemeService>(), Mock.Of<IAuditService>(), new FakeUser(),
                Options.Create(new UploadOptions()));
        }

        private static string GoldenPath(int option) =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Golden", $"export-{option:D2}.csv");

        private static string Export(int option)
        {
            var previous = CultureInfo.CurrentCulture;

            try
            {
                // The application serves every request in en-GB.
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");

                return BuildService().ConvertListToString(option.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [TestCaseSource(nameof(ExportOptions))]
        public void Export_matches_the_frozen_output(int option)
        {
            Assert.IsTrue(File.Exists(GoldenPath(option)), $"Missing golden file for export {option}.");

            Assert.AreEqual(File.ReadAllText(GoldenPath(option)), Export(option));
        }

        public static IEnumerable<int> ExportOptions() => Enumerable.Range(1, 16);

        [TestCaseSource(nameof(ExportOptions))]
        public void Every_export_uses_windows_line_endings_only(int option)
        {
            var csv = Export(option);

            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(csv, "(?<!\r)\n"), "Found a bare line feed.");
            Assert.IsFalse(csv.Contains('\r') && System.Text.RegularExpressions.Regex.IsMatch(csv, "\r(?!\n)"), "Found a bare carriage return.");
        }

        [TestCase(1, 2)]
        [TestCase(5, 2)]
        [TestCase(9, 2)]
        [TestCase(13, 2)]
        [TestCase(2, 1)]
        [TestCase(6, 1)]
        [TestCase(10, 1)]
        [TestCase(14, 1)]
        public void The_plant_list_has_a_header_and_one_row_per_listed_plant(int option, int listedPlants)
        {
            // Options 1, 5, 9 and 13 list every plant of a scheme; 2, 6, 10 and 14 list its active plants only.
            var lines = Export(option).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(1 + listedPlants, lines.Length, $"export {option}");
        }

        [Test]
        public void Known_issue_pcg_export_labels_11_and_12_are_swapped()
        {
            // Existing behaviour, present before the ACA work: option 11 is labelled "All Plants and Data" but lists active
            // plants only, and option 12 is labelled "Active Plants Only" but lists every plant. Fixing it changes what
            // users download, so it needs owner approval; update the golden files when that is given.
            var allAndData = Export(11).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            var activeAndData = Export(12).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(2, allAndData.Length);
            Assert.AreEqual(3, activeAndData.Length);
        }

        [Test]
        public void Last_inspection_dates_use_the_british_day_month_year_order()
        {
            StringAssert.Contains("\"31/01/2020\"", Export(3));
        }

        [Test]
        public void A_comma_in_a_plant_name_stays_inside_its_quoted_field()
        {
            StringAssert.Contains("\"Alpha, Meats Ltd\"", Export(1));
        }

        /// <summary>
        /// Rewrites the golden files from the current code. Run only when an output change is deliberate and approved:
        /// set MTS_GOLDEN_OUT to the Golden folder of this project and run this test explicitly.
        /// </summary>
        [Test]
        [Explicit("Rewrites the golden files; run only for an approved output change.")]
        public void Regenerate_golden_files()
        {
            var folder = Environment.GetEnvironmentVariable("MTS_GOLDEN_OUT");
            Assert.IsFalse(string.IsNullOrWhiteSpace(folder), "Set MTS_GOLDEN_OUT to the Golden folder.");
            Directory.CreateDirectory(folder);

            foreach (var option in ExportOptions())
            {
                File.WriteAllText(Path.Combine(folder, $"export-{option:D2}.csv"), Export(option), new System.Text.UTF8Encoding(false));
            }
        }
    }
}
