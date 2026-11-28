using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RPA.MTSInspections.DAL;

// Used only by `dotnet ef`; the app itself is configured in ApplicationServices.
public class MTSInspectionsContextFactory : IDesignTimeDbContextFactory<MTSInspectionsContext>
{
    public MTSInspectionsContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MTSInspectionsContext>()
            .UseSqlServer("Server=(local);Database=MTSInspections;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new MTSInspectionsContext(options);
    }
}
