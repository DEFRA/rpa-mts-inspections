using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace RPA.MTSInspections.DAL
{
    // Code-based replacement for the <entityFramework> section of Web.config.
    public class MTSInspectionsDbConfiguration : DbConfiguration
    {
        public MTSInspectionsDbConfiguration()
        {
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
            SetDefaultConnectionFactory(new SqlConnectionFactory());
        }
    }
}
