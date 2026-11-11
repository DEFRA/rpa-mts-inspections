using NUnit.Framework;
using RPA.MTSInspections.Models;

namespace RPA.MTSInspections.Tests.Model
{
    /// <summary>
    /// Records what the four risk formulas return today, including cases the business may not intend. A failing test
    /// here means a score changed: that needs owner approval, it is not a refactoring side effect.
    /// </summary>
    [TestFixture]
    public class RiskCalculationCharacterizationTests
    {
        // Every criterion gets a distinct weighting (1, 2, 3, ...) and option score 1, so each term is visible in the total.
        private static PlantOption Option(string criteria, int weighting, int score = 1, bool active = true) => new()
        {
            Option = new Option
            {
                Name = criteria + " option",
                Score = score,
                Active = active,
                Criteria = new Criteria { Name = criteria, Weighting = weighting }
            }
        };

        private static List<PlantOption> Weighted(params string[] criteria) =>
            criteria.Select((name, index) => Option(name, index + 1)).ToList();

        private static readonly string[] SccCriteria =
        [
            "Throughput", "Operating hours", "Sign in prior to inspection", "Trimming", "Skin Removal", "Carcass dressing",
            "Head Removal", "Feet Removal", "Failed / Unsatisfactory",
            "How many fails / unsatisfactory in previous 12 Months", "How many near misses in previous 12 Months",
            "Number of Enforcement Notices Issued in Last 12 Months", "Number of Penalty Notices Issued in Last 12 Months"
        ];

        [Test]
        public void Scc_score_sums_score_times_weighting_for_each_criterion()
        {
            // Weightings 1..13 with score 1 would total 91 (1+2+...+13), but the enforcement and penalty terms are multiplied by the
            // weighting of "How many fails / unsatisfactory in previous 12 Months" (10) instead of their own (12, 13):
            // (1+2+...+11 = 66) + 10 + 10 = 86. This records the existing behaviour for the owner to review; it is not endorsed.
            var score = new SccRiskCalculation().Calculation(Weighted(SccCriteria));

            Assert.AreEqual(86, score);
        }

        [Test]
        public void Scc_enforcement_and_penalty_terms_ignore_their_own_weighting()
        {
            var options = Weighted(SccCriteria);
            var baseline = new SccRiskCalculation().Calculation(options);

            options.Single(x => x.Option.Criteria.Name == "Number of Enforcement Notices Issued in Last 12 Months").Option.Criteria.Weighting = 500;
            options.Single(x => x.Option.Criteria.Name == "Number of Penalty Notices Issued in Last 12 Months").Option.Criteria.Weighting = 900;

            Assert.AreEqual(baseline, new SccRiskCalculation().Calculation(options));
        }

        [Test]
        public void Scc_scales_with_the_option_score()
        {
            var options = Weighted(SccCriteria);
            options.Single(x => x.Option.Criteria.Name == "Throughput").Option.Score = 5;

            // Throughput weighting is 1, so the total rises by (5 - 1) * 1.
            Assert.AreEqual(90, new SccRiskCalculation().Calculation(options));
        }

        [TestCase(typeof(BlsRiskCalculation))]
        [TestCase(typeof(BccRiskCalculation))]
        [TestCase(typeof(PcgRiskCalculation))]
        [TestCase(typeof(SccRiskCalculation))]
        public void A_score_is_zero_until_every_required_criterion_has_an_option(Type calculation)
        {
            var calc = (RiskCalculation)Activator.CreateInstance(calculation);

            Assert.AreEqual(0, calc.Calculation([]));
            Assert.AreEqual(0, calc.Calculation(Weighted("Throughput", "Operating Hours")));
        }

        [Test]
        public void Scc_is_zero_when_one_required_criterion_is_missing()
        {
            var options = Weighted(SccCriteria).Where(x => x.Option.Criteria.Name != "Feet Removal").ToList();

            Assert.AreEqual(0, new SccRiskCalculation().Calculation(options));
        }

        [Test]
        public void Scc_criteria_names_are_case_sensitive()
        {
            // "Operating hours" (lower case h) is the SCC spelling; the BLS/BCC/PCG spelling "Operating Hours" does not match.
            var options = Weighted(SccCriteria);
            options.Single(x => x.Option.Criteria.Name == "Operating hours").Option.Criteria.Name = "Operating Hours";

            Assert.AreEqual(0, new SccRiskCalculation().Calculation(options));
        }

        [Test]
        public void Bls_only_counts_active_nature_of_operation_options()
        {
            string[] required =
            [
                "Premises Type", "Operating Hours", "Throughput", "Origin of Products Handled", "Type of Operation",
                "Number of Inspections Resulting in Failure", "Number of Non-Compliance", "Severity of Non-Compliance",
                "Number of Months Since Last Inspection"
            ];

            List<PlantOption> Build(bool inactiveNopCounts)
            {
                var options = required.Select(name => Option(name, 1, 0)).ToList();
                options.Add(Option("Nature of Operation", 10, 3));
                options.Add(Option("Nature of Operation", 10, 7, active: inactiveNopCounts));

                return options;
            }

            var withInactive = new BlsRiskCalculation().Calculation(Build(true));
            var withoutInactive = new BlsRiskCalculation().Calculation(Build(false));

            // Active options add (3 + 7) * 10 = 100; with the second option inactive only 3 * 10 = 30 counts.
            Assert.AreEqual(100, withInactive);
            Assert.AreEqual(30, withoutInactive);
        }
    }
}
