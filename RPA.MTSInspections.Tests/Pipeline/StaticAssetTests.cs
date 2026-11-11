using System.Net;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace RPA.MTSInspections.Tests.Pipeline
{
    /// <summary>
    /// Every script, stylesheet, font and image the layout needs must be served by the application itself:
    /// a missing or wrongly cased file would break the page on a case-sensitive Linux container, and an external
    /// host would send users' requests to a third party and need outbound access from ACA.
    /// </summary>
    [TestFixture]
    public class StaticAssetTests
    {
        private MtsWebApplicationFactory factory;

        [OneTimeSetUp]
        public void CreateHost() => factory = new MtsWebApplicationFactory();

        [OneTimeTearDown]
        public void DisposeHost() => factory.Dispose();

        private static readonly Regex Reference = new("""(?:src|href)\s*=\s*"([^"]+)"|url\(\s*['"]?([^'")]+)""", RegexOptions.IgnoreCase);

        private static IEnumerable<string> References(string text) =>
            Reference.Matches(text).Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value);

        private async Task<string> Get(string path)
        {
            using var client = factory.Client();
            var response = await client.GetAsync(path);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, path);

            return await response.Content.ReadAsStringAsync();
        }

        private static string Clean(string reference)
        {
            var end = reference.IndexOfAny(['?', '#']);

            return end < 0 ? reference : reference[..end];
        }

        [Test]
        public async Task The_layout_loads_only_local_assets_and_all_of_them_exist()
        {
            var html = await Get("/Account/SignedOut");
            var assets = References(html).Where(r => r.Contains("/Scripts/") || r.Contains("/Content/") || r.Contains("favicon")).Select(Clean).ToList();

            Assert.That(assets, Is.Not.Empty);

            foreach (var reference in References(html).Where(r => !r.StartsWith('#') && !r.StartsWith("javascript:")))
            {
                Assert.IsFalse(Regex.IsMatch(reference, "^(https?:)?//"), $"External reference in the layout: {reference}");
            }

            foreach (var asset in assets.Distinct())
            {
                await Get(asset);
            }
        }

        [TestCase("/Content/Site.css")]
        [TestCase("/Content/bootstrap.css")]
        [TestCase("/Content/bootstrap-datepicker3.css")]
        [TestCase("/Content/PagedList.css")]
        public async Task Every_file_a_stylesheet_points_to_exists_and_is_local(string stylesheet)
        {
            var css = await Get(stylesheet);
            var directory = stylesheet[..(stylesheet.LastIndexOf('/') + 1)];

            foreach (var reference in References(css).Where(r => !r.StartsWith("data:") && !r.StartsWith('#')))
            {
                Assert.IsFalse(Regex.IsMatch(reference, "^(https?:)?//"), $"{stylesheet} points to an external file: {reference}");

                // Resolve relative to the stylesheet exactly as a browser would.
                var resolved = new Uri(new Uri("https://localhost" + directory), Clean(reference)).AbsolutePath;
                await Get(resolved);
            }
        }

        [Test]
        public async Task The_patched_library_versions_are_the_ones_served()
        {
            StringAssert.Contains("jQuery v3.7.1", await Get("/Scripts/jquery-3.7.1.min.js"));
            StringAssert.Contains("Bootstrap v3.4.1", await Get("/Scripts/bootstrap.js"));
            StringAssert.Contains("Bootstrap v3.4.1", await Get("/Content/bootstrap.css"));
            StringAssert.Contains("jQuery Validation Plugin v1.19.5", await Get("/Scripts/jquery.validate.js"));
            StringAssert.Contains("Datepicker for Bootstrap v1.10.0", await Get("/Scripts/bootstrap-datepicker.js"));
        }

        [Test]
        public async Task The_removed_jquery_3_3_1_is_no_longer_served()
        {
            using var client = factory.Client();

            // A missing file falls through to the authenticated fallback policy, so an anonymous request gets 401, not 404.
            Assert.AreNotEqual(HttpStatusCode.OK, (await client.GetAsync("/Scripts/jquery-3.3.1.min.js")).StatusCode);
        }
    }
}
