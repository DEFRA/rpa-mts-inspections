using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Tests.Hosting
{
    [TestFixture]
    public class ApplicationServicesTests
    {
        private const string ManagedIdentitySql =
            "Server=tcp:sql.example.invalid,1433;Database=D;Authentication=Active Directory Managed Identity;User Id=11111111-1111-1111-1111-111111111111;Encrypt=True;TrustServerCertificate=False";

        internal static Dictionary<string, string> ValidSettings() => new()
        {
            ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
            ["AzureAd:TenantId"] = "22222222-2222-2222-2222-222222222222",
            ["AzureAd:ClientId"] = "33333333-3333-3333-3333-333333333333",
            ["AzureAd:ClientSecret"] = "test-secret",
            ["Authorization:Roles:BasicAccess"] = "MTS.BasicAccess",
            ["Authorization:Roles:SupportTeam"] = "MTS.SupportTeam",
            ["Authorization:Roles:Inspector"] = "MTS.Inspector",
            ["Authorization:Roles:Admin"] = "MTS.Admin",
            ["Identity:AuditNameClaimType"] = "preferred_username",
            ["ConnectionStrings:MTSInspectionsContext"] = ManagedIdentitySql,
            ["AllowedHosts"] = "app.example.invalid"
        };

        private static IHostEnvironment Environment(string name)
        {
            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(x => x.EnvironmentName).Returns(name);

            return environment.Object;
        }

        private static InvalidOperationException Register(Dictionary<string, string> settings, string environment = "Production")
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            return Assert.Throws<InvalidOperationException>(
                () => new ServiceCollection().AddMtsApplication(configuration, Environment(environment)));
        }

        [Test]
        public void Valid_configuration_registers()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(ValidSettings()).Build();

            Assert.DoesNotThrow(() => new ServiceCollection().AddMtsApplication(configuration, Environment("Production")));
        }

        [TestCase("AzureAd:TenantId")]
        [TestCase("AzureAd:ClientId")]
        [TestCase("AzureAd:ClientSecret")]
        [TestCase("AzureAd:Instance")]
        [TestCase("Authorization:Roles:BasicAccess")]
        [TestCase("Authorization:Roles:SupportTeam")]
        [TestCase("Authorization:Roles:Inspector")]
        [TestCase("Authorization:Roles:Admin")]
        [TestCase("Identity:AuditNameClaimType")]
        [TestCase("ConnectionStrings:MTSInspectionsContext")]
        [TestCase("AllowedHosts")]
        public void Missing_required_setting_fails_start_up(string key)
        {
            var settings = ValidSettings();
            settings.Remove(key);

            var exception = Register(settings);

            Assert.That(exception.Message, Does.Contain(key));
        }

        [TestCase("AzureAd:TenantId", "common")]
        [TestCase("AzureAd:TenantId", "organizations")]
        [TestCase("AzureAd:TenantId", "00000000-0000-0000-0000-000000000000")]
        [TestCase("AzureAd:ClientId", "not-a-guid")]
        [TestCase("AzureAd:Instance", "http://login.microsoftonline.com/")]
        [TestCase("AzureAd:Instance", "https://login.microsoftonline.com/?x=1")]
        [TestCase("Authorization:Roles:Admin", "MTS Admin")]
        [TestCase("Authorization:Roles:Admin", ".MTS.Admin")]
        [TestCase("Identity:AuditNameClaimType", "preferred username")]
        [TestCase("AllowedHosts", "*")]
        [TestCase("AllowedHosts", "app.example.invalid;*.example.invalid")]
        [TestCase("AllowedHosts", "https://app.example.invalid")]
        [TestCase("HttpsRedirection:HttpsPort", "0")]
        [TestCase("HttpsRedirection:HttpsPort", "65536")]
        [TestCase("ConnectionStrings:MTSInspectionsContext", "not a connection string")]
        public void Invalid_setting_fails_start_up(string key, string value)
        {
            var settings = ValidSettings();
            settings[key] = value;

            Register(settings);
        }

        [Test]
        public void Duplicate_role_values_fail_start_up()
        {
            var settings = ValidSettings();
            settings["Authorization:Roles:Inspector"] = "mts.admin";

            var exception = Register(settings);

            Assert.That(exception.Message, Does.Contain("four different"));
        }

        [TestCase("Server=sql;Database=D;Integrated Security=True")]
        [TestCase("Server=sql;Database=D;Trusted_Connection=Yes")]
        public void Integrated_security_is_rejected_outside_development(string connection)
        {
            var settings = ValidSettings();
            settings["ConnectionStrings:MTSInspectionsContext"] = connection;

            var exception = Register(settings);

            Assert.That(exception.Message, Does.Contain("Integrated Security"));
        }

        [Test]
        public void Trusting_the_server_certificate_is_rejected_outside_development()
        {
            var settings = ValidSettings();
            settings["ConnectionStrings:MTSInspectionsContext"] = ManagedIdentitySql.Replace("TrustServerCertificate=False", "TrustServerCertificate=True");

            var exception = Register(settings);

            Assert.That(exception.Message, Does.Contain("TrustServerCertificate"));
        }

        [Test]
        public void Development_allows_a_local_integrated_security_connection()
        {
            var settings = ValidSettings();
            settings["ConnectionStrings:MTSInspectionsContext"] = "Server=(localdb)\\MSSQLLocalDB;Database=D;Integrated Security=True;TrustServerCertificate=True";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            Assert.DoesNotThrow(() => new ServiceCollection().AddMtsApplication(configuration, Environment("Development")));
        }

        [Test]
        public void The_obsolete_idt_connection_string_is_rejected()
        {
            var settings = ValidSettings();
            settings["ConnectionStrings:IDTSecurityConnection"] = "Server=old;Database=Security;Integrated Security=True";

            var exception = Register(settings);

            Assert.That(exception.Message, Does.Contain("IDTSecurityConnection"));
        }

        [TestCase("Upload:MaxFileBytes", "0")]
        [TestCase("Upload:MaxRowsPerSheet", "-1")]
        public void Non_positive_upload_limits_fail_start_up(string key, string value)
        {
            var settings = ValidSettings();
            settings[key] = value;

            Register(settings);
        }
    }
}
