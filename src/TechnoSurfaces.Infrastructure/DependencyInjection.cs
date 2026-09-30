using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechnoSurfaces.Application.Costing;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Infrastructure.Data;

namespace TechnoSurfaces.Infrastructure;

/// <summary>
/// Registers the data access, the pricing strategies and the calculation engine.
///
/// Call this from the web application's Program.cs. The connection string comes
/// from Azure Key Vault through the App Service managed identity; no credential is
/// held in source control or in plain configuration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTechnoSurfaces(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TechnoSurfacesDbContext>(o => o.UseSqlServer(connectionString));

        services.AddScoped<ICatalogueReader, CatalogueReader>();

        // Both strategies are registered, and PriceResolver picks the one matching
        // the supplier's pricing scheme. A sixth supplier on a new scheme means
        // adding a strategy here, not editing the resolver.
        services.AddScoped<IPriceResolutionStrategy, BandPricedStrategy>();
        services.AddScoped<IPriceResolutionStrategy, ItemPricedStrategy>();

        services.AddScoped<IPriceResolver, PriceResolver>();
        services.AddScoped<IRateResolver, RateResolver>();
        services.AddScoped<IQuoteCalculationService, QuoteCalculationService>();

        return services;
    }
}
