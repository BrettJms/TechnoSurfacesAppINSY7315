using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechnoSurfaces.Infrastructure.Data;

/// <summary>
/// Lets the EF Core tooling build a context when adding or scripting a migration,
/// without starting the web application.
///
/// The connection string here is used only by the tooling on a developer machine.
/// The running application takes its connection string from Azure Key Vault through
/// the App Service managed identity; no credential is held in source control or in
/// plain configuration.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TechnoSurfacesDbContext>
{
    public TechnoSurfacesDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("TECHNOSURFACES_DESIGNTIME_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=TechnoSurfaces;Trusted_Connection=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<TechnoSurfacesDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TechnoSurfacesDbContext(options);
    }
}
