using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Tests.Hosting
{
    [TestFixture]
    public class InfrastructureRegistrationTests
    {
        private static IConfiguration Config(Dictionary<string, string> settings) =>
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        private static ForwardedHeadersOptions Forwarding(Dictionary<string, string> settings)
        {
            var services = new ServiceCollection();
            services.AddMtsForwarding(Config(settings));

            return services.BuildServiceProvider().GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        }

        // Forwarded headers

        [Test]
        public void Forwarding_is_off_when_no_proxy_is_configured()
        {
            Assert.AreEqual(ForwardedHeaders.None, Forwarding([]).ForwardedHeaders);
        }

        [Test]
        public void Forwarding_trusts_only_the_configured_proxy_and_network()
        {
            var options = Forwarding(new()
            {
                ["ReverseProxy:KnownProxies:0"] = "10.1.2.3",
                ["ReverseProxy:KnownIPNetworks:0"] = "10.20.0.0/24"
            });

            Assert.AreEqual(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, options.ForwardedHeaders);
            Assert.AreEqual(1, options.ForwardLimit);
            CollectionAssert.Contains(options.KnownProxies, IPAddress.Parse("10.1.2.3"));
            Assert.AreEqual(1, options.KnownIPNetworks.Count);
        }

        [TestCase("ReverseProxy:KnownProxies:0", "0.0.0.0")]
        [TestCase("ReverseProxy:KnownProxies:0", "::")]
        [TestCase("ReverseProxy:KnownProxies:0", "not-an-ip")]
        [TestCase("ReverseProxy:KnownIPNetworks:0", "0.0.0.0/0")]
        [TestCase("ReverseProxy:KnownIPNetworks:0", "10.0.0.0/0")]
        [TestCase("ReverseProxy:KnownIPNetworks:0", "10.0.0.5/8")]
        [TestCase("ReverseProxy:KnownIPNetworks:0", "garbage")]
        public void Wildcard_or_malformed_proxy_trust_is_rejected(string key, string value)
        {
            Assert.Throws<InvalidOperationException>(() => Forwarding(new() { [key] = value }));
        }

        // Shared Data Protection keys

        private static Dictionary<string, string> KeyRing() => new()
        {
            ["DataProtection:Enabled"] = "true",
            ["DataProtection:BlobUri"] = "https://store.blob.core.windows.net/data-protection/keys.xml",
            ["DataProtection:KeyIdentifier"] = "https://vault.vault.azure.net/keys/data-protection",
            ["DataProtection:ApplicationName"] = "mts-prod",
            ["DataProtection:ManagedIdentityClientId"] = "11111111-1111-1111-1111-111111111111"
        };

        private static void AddKeyRing(Dictionary<string, string> settings) =>
            new ServiceCollection().AddMtsDataProtection(Config(settings));

        [Test]
        public void Data_protection_is_optional_and_off_by_default()
        {
            Assert.DoesNotThrow(() => AddKeyRing([]));
        }

        [Test]
        public void A_complete_key_ring_configuration_is_accepted()
        {
            Assert.DoesNotThrow(() => AddKeyRing(KeyRing()));
        }

        [Test]
        public void Disabled_means_no_further_settings_are_required()
        {
            Assert.DoesNotThrow(() => AddKeyRing(new() { ["DataProtection:Enabled"] = "false" }));
        }

        [TestCase("DataProtection:Enabled", "maybe")]
        [TestCase("DataProtection:BlobUri", "http://store.blob.core.windows.net/data-protection/keys.xml")]
        [TestCase("DataProtection:BlobUri", "https://store.blob.core.windows.net/data-protection/keys.xml?sv=secret")]
        [TestCase("DataProtection:BlobUri", "https://store.blob.core.windows.net/data-protection")]
        [TestCase("DataProtection:KeyIdentifier", "https://vault.vault.azure.net/secrets/data-protection")]
        [TestCase("DataProtection:KeyIdentifier", "https://vault.vault.azure.net/keys/data-protection/0123456789abcdef")]
        [TestCase("DataProtection:ApplicationName", " ")]
        [TestCase("DataProtection:ManagedIdentityClientId", "00000000-0000-0000-0000-000000000000")]
        [TestCase("DataProtection:ManagedIdentityClientId", "not-a-guid")]
        public void An_invalid_key_ring_setting_fails_start_up(string key, string value)
        {
            var settings = KeyRing();
            settings[key] = value;

            Assert.Throws<InvalidOperationException>(() => AddKeyRing(settings));
        }

        [Test]
        public void An_unknown_key_ring_setting_fails_start_up()
        {
            var settings = KeyRing();
            settings["DataProtection:Typo"] = "x";

            Assert.Throws<InvalidOperationException>(() => AddKeyRing(settings));
        }

        // Telemetry

        private static int TelemetryServiceCount(Dictionary<string, string> settings)
        {
            var services = new ServiceCollection();
            services.AddMtsApplicationInsights(Config(settings));

            return services.Count;
        }

        private const string Connection = "InstrumentationKey=11111111-1111-1111-1111-111111111111;IngestionEndpoint=https://example.invalid/";

        [Test]
        public void Telemetry_stays_off_without_a_connection_string()
        {
            Assert.AreEqual(0, TelemetryServiceCount([]));
        }

        [Test]
        public void The_azure_standard_setting_enables_telemetry()
        {
            Assert.Greater(TelemetryServiceCount(new() { ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = Connection }), 0);
        }

        [Test]
        public void The_nested_setting_still_enables_telemetry()
        {
            Assert.Greater(TelemetryServiceCount(new() { ["ApplicationInsights:ConnectionString"] = Connection }), 0);
        }

        [Test]
        public void An_ingestion_identity_with_a_connection_string_is_accepted()
        {
            Assert.Greater(TelemetryServiceCount(new()
            {
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = Connection,
                ["Telemetry:ManagedIdentityClientId"] = "11111111-1111-1111-1111-111111111111"
            }), 0);
        }

        [Test]
        public void An_ingestion_identity_without_a_connection_string_fails_start_up()
        {
            Assert.Throws<InvalidOperationException>(() =>
                TelemetryServiceCount(new() { ["Telemetry:ManagedIdentityClientId"] = "11111111-1111-1111-1111-111111111111" }));
        }

        [TestCase("")]
        [TestCase("not-a-guid")]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        public void An_invalid_ingestion_identity_fails_start_up(string identity)
        {
            Assert.Throws<InvalidOperationException>(() => TelemetryServiceCount(new()
            {
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = Connection,
                ["Telemetry:ManagedIdentityClientId"] = identity
            }));
        }
    }
}
