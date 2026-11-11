using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Microsoft.Data.SqlClient;

namespace RPA.MTSInspections.DAL
{
    // Code-based replacement for the <entityFramework> section of Web.config. It uses the Microsoft.Data.SqlClient
    // provider so Entra ID authentication (for example "Authentication=Active Directory Managed Identity") can be
    // selected purely through the connection string, and applies the Azure SQL transient-fault retry strategy.
    public class MTSInspectionsDbConfiguration : DbConfiguration
    {
        public MTSInspectionsDbConfiguration()
        {
            SetProviderFactory(MicrosoftSqlProviderServices.ProviderInvariantName, SqlClientFactory.Instance);
            SetProviderServices(MicrosoftSqlProviderServices.ProviderInvariantName, MicrosoftSqlProviderServices.Instance);
            SetMigrationSqlGenerator(MicrosoftSqlProviderServices.ProviderInvariantName, () => new MicrosoftSqlServerMigrationSqlGenerator());
            SetExecutionStrategy(MicrosoftSqlProviderServices.ProviderInvariantName, () => new MicrosoftSqlAzureExecutionStrategy());
        }
    }
}
