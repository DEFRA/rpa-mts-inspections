using System.Net;
using NUnit.Framework;

namespace RPA.MTSInspections.Tests.Pipeline
{
    /// <summary>
    /// End-to-end checks of the HTTP pipeline: who may reach what, antiforgery, probes, HTTPS and host filtering.
    /// Sign-in is simulated, so these do not prove a live Entra exchange.
    /// </summary>
    [TestFixture]
    public class PipelineTests
    {
        private MtsWebApplicationFactory factory;

        [OneTimeSetUp]
        public void CreateHost() => factory = new MtsWebApplicationFactory();

        [OneTimeTearDown]
        public void DisposeHost() => factory.Dispose();

        private static HttpRequestMessage Request(HttpMethod method, string path, string roles = null, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, path) { Content = content };

            if (roles != null)
            {
                request.Headers.Add("X-Test-User", "1");
                request.Headers.Add("X-Test-Roles", roles);
            }

            return request;
        }

        private async Task<HttpResponseMessage> Send(string path, string roles = null, HttpMethod method = null, HttpContent content = null)
        {
            using var client = factory.Client();

            return await client.SendAsync(Request(method ?? HttpMethod.Get, path, roles, content));
        }

        private const string Basic = TestRoles.Basic;
        private const string Support = TestRoles.Support;
        private const string Inspector = TestRoles.Inspector;
        private const string Admin = TestRoles.Admin;

        // Authentication

        [TestCase("/Home")]
        [TestCase("/Plant")]
        [TestCase("/Admin/Index")]
        [TestCase("/Export/Index")]
        public async Task Anonymous_requests_are_challenged(string path)
        {
            var response = await Send(path);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestCase("/Home")]
        [TestCase("/Plant")]
        [TestCase("/Export/Index")]
        public async Task A_signed_in_user_without_an_mts_role_is_forbidden(string path)
        {
            var response = await Send(path, roles: "Some.Other.Role");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task The_former_idt_labels_do_not_grant_access()
        {
            var response = await Send("/Home", roles: "MTS Risk Analysis: Basic Access");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Authorization by capability

        [TestCase("/Home", Basic, HttpStatusCode.OK)]
        [TestCase("/Home", Admin, HttpStatusCode.OK)]
        [TestCase("/Plant", Inspector, HttpStatusCode.OK)]
        [TestCase("/Admin/Index", Basic, HttpStatusCode.Forbidden)]
        [TestCase("/Admin/Index", Inspector, HttpStatusCode.Forbidden)]
        [TestCase("/Admin/Index", Support, HttpStatusCode.OK)]
        [TestCase("/Admin/Index", Admin, HttpStatusCode.OK)]
        [TestCase("/Admin/AdminIndex", Support, HttpStatusCode.Forbidden)]
        [TestCase("/Admin/AdminIndex", Inspector, HttpStatusCode.Forbidden)]
        [TestCase("/Admin/AdminIndex", Admin, HttpStatusCode.OK)]
        [TestCase("/Admin/BlsCreate", Support, HttpStatusCode.Forbidden)]
        [TestCase("/Admin/SccOptions", Support, HttpStatusCode.Forbidden)]
        [TestCase("/Export/Index", Basic, HttpStatusCode.Forbidden)]
        [TestCase("/Export/Index", Inspector, HttpStatusCode.Forbidden)]
        [TestCase("/Export/Index", Support, HttpStatusCode.OK)]
        [TestCase("/Export/Index", Admin, HttpStatusCode.OK)]
        [TestCase("/Export/_ImportPartial", Support, HttpStatusCode.Forbidden)]
        public async Task Each_page_requires_its_role(string path, string role, HttpStatusCode expected)
        {
            var response = await Send(path, roles: role);

            Assert.AreEqual(expected, response.StatusCode);
        }

        [Test]
        public async Task Roles_are_not_cumulative_across_users_but_are_across_assignments()
        {
            var response = await Send("/Admin/AdminIndex", roles: $"{Basic},{Admin}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // Writes: verbs, antiforgery and roles

        [TestCase("/Scheme/CaclulateAllRiskScores")]
        [TestCase("/Scheme/RiskScoreUpdate")]
        public async Task Recalculation_cannot_be_triggered_by_a_get(string path)
        {
            var response = await Send(path, roles: Admin);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.MethodNotAllowed));
        }

        [TestCase("/Scheme/CaclulateAllRiskScores", Admin)]
        [TestCase("/Scheme/BLSSchemePost", Admin)]
        [TestCase("/Scheme/BCCSchemePost", Inspector)]
        [TestCase("/Scheme/PCGSchemePost", Admin)]
        [TestCase("/Scheme/SCCSchemePost", Basic)]
        [TestCase("/Admin/Create", Admin)]
        [TestCase("/Admin/BlsEdit", Admin)]
        [TestCase("/Admin/EditWeightingScore", Admin)]
        [TestCase("/Export/ExportCSV", Support)]
        public async Task A_post_without_an_antiforgery_token_is_rejected(string path, string role)
        {
            var response = await Send(path, roles: role, method: HttpMethod.Post, content: new FormUrlEncodedContent([]));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestCase("/Scheme/BLSSchemePost", Basic)]
        [TestCase("/Scheme/BLSSchemePost", Support)]
        [TestCase("/Scheme/BCCSchemePost", Support)]
        [TestCase("/Scheme/PCGSchemePost", Basic)]
        [TestCase("/Scheme/CaclulateAllRiskScores", Support)]
        [TestCase("/Scheme/CaclulateAllRiskScores", Inspector)]
        [TestCase("/Admin/Create", Inspector)]
        [TestCase("/Export/_ImportPartial", Support)]
        public async Task A_post_without_the_required_role_is_forbidden_before_anything_else(string path, string role)
        {
            var response = await Send(path, roles: role, method: HttpMethod.Post, content: new FormUrlEncodedContent([]));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Export_download_is_post_only()
        {
            var response = await Send("/Export/ExportCSV", roles: Support);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.MethodNotAllowed));
        }

        // Forms still render an antiforgery token, so the global check does not break existing screens

        [TestCase("/Plant", Basic)]
        [TestCase("/Admin/AdminIndex", Admin)]
        [TestCase("/Export/Index", Support)]
        [TestCase("/Account/SignOut", Basic)]
        public async Task Existing_forms_carry_an_antiforgery_token(string path, string role)
        {
            var response = await Send(path, roles: role);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
            StringAssert.Contains("__RequestVerificationToken", body);
        }

        [Test]
        public async Task Sign_out_needs_a_token_and_a_session()
        {
            var anonymous = await Send("/Account/SignOut", method: HttpMethod.Post, content: new FormUrlEncodedContent([]));
            var noToken = await Send("/Account/SignOut", roles: Basic, method: HttpMethod.Post, content: new FormUrlEncodedContent([]));

            Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, noToken.StatusCode);
        }

        [Test]
        public async Task The_layout_offers_sign_out_only_when_signed_in()
        {
            var signedIn = await (await Send("/Home", roles: Basic)).Content.ReadAsStringAsync();
            var signedOut = await (await Send("/Account/SignedOut")).Content.ReadAsStringAsync();

            StringAssert.Contains("/Account/SignOut", signedIn);
            StringAssert.DoesNotContain("/Account/SignOut", signedOut);
        }

        // Status pages and errors

        [Test]
        public async Task Status_pages_work_without_a_session()
        {
            Assert.AreEqual(HttpStatusCode.OK, (await Send("/Account/SignedOut")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await Send("/Account/AccessDenied")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await Send("/Account/AuthenticationFailed")).StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, (await Send("/Error")).StatusCode);
        }

        [Test]
        public async Task A_failed_post_can_still_show_the_error_page()
        {
            var response = await Send("/Error", method: HttpMethod.Post, content: new FormUrlEncodedContent([]));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // Probes, HTTPS and hosts

        [Test]
        public async Task Liveness_is_anonymous_and_never_touches_the_database()
        {
            var response = await Send("/health/live");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Healthy", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task Readiness_is_anonymous_and_reports_an_unreachable_database_as_unhealthy()
        {
            var response = await Send("/health/ready");

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.AreEqual("Unhealthy", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task Probes_are_served_over_http_without_a_redirect()
        {
            using var client = factory.Client("http://localhost");

            var live = await client.GetAsync("/health/live");

            Assert.AreEqual(HttpStatusCode.OK, live.StatusCode);
        }

        [Test]
        public async Task Application_routes_are_redirected_to_https()
        {
            using var client = factory.Client("http://localhost");

            var response = await client.GetAsync("/Home");

            Assert.That((int)response.StatusCode, Is.EqualTo(307).Or.EqualTo(308));
            Assert.AreEqual("https://localhost/Home", response.Headers.Location?.ToString());
        }

        [Test]
        public async Task An_unlisted_host_header_is_rejected()
        {
            using var client = factory.Client();
            var request = Request(HttpMethod.Get, "/health/live");
            request.Headers.Host = "evil.example";

            var response = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Untrusted_forwarded_headers_do_not_change_the_scheme()
        {
            using var client = factory.Client("http://localhost");
            var request = Request(HttpMethod.Get, "/Home", Basic);
            request.Headers.Add("X-Forwarded-Proto", "https");

            var response = await client.SendAsync(request);

            Assert.That((int)response.StatusCode, Is.EqualTo(307).Or.EqualTo(308));
        }
    }
}
