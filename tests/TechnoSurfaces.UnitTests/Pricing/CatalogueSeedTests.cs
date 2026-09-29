using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Infrastructure.Data;
using TechnoSurfaces.Infrastructure.Data.Seed;

namespace TechnoSurfaces.UnitTests.Pricing;

/// <summary>
/// Loads the real catalogue from the client's supplier price lists and resolves
/// prices against it. This proves the model accommodates all five suppliers, three
/// of whom price by band and two by item, and that the published figures come back
/// to the cent.
/// </summary>
public sealed class CatalogueSeedTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private TechnoSurfacesDbContext _db = null!;
    private IPriceResolver _resolver = null!;

    private static readonly DateOnly Today = new(2026, 9, 29);

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TechnoSurfacesDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new TechnoSurfacesDbContext(options);
        await _db.Database.EnsureCreatedAsync();

        await CatalogueSeeder.SeedAsync(_db);
        await RateCardSeeder.SeedAsync(_db);

        var catalogue = new CatalogueReader(_db);
        _resolver = new PriceResolver(catalogue, new IPriceResolutionStrategy[]
        {
            new BandPricedStrategy(catalogue),
            new ItemPricedStrategy(catalogue)
        });
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task All_five_suppliers_are_seeded_and_both_pricing_schemes_are_represented()
    {
        var suppliers = await _db.Suppliers.AsNoTracking().ToListAsync();

        Assert.Equal(5, suppliers.Count);
        Assert.Equal(2, suppliers.Count(s => s.PricingStructure == PricingStructure.Band));
        Assert.Equal(3, suppliers.Count(s => s.PricingStructure == PricingStructure.Item));
    }

    [Fact]
    public async Task Every_seeded_colour_resolves_to_a_price()
    {
        // Nothing in the catalogue may be unpriceable. A colour the system cannot
        // price is the failure this project exists to make visible, so it must not
        // be shipped in the seed data.
        var colours = await _db.Colours
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.ProductLineId })
            .ToListAsync();

        Assert.NotEmpty(colours);

        var unresolved = new List<string>();

        foreach (var colour in colours)
        {
            var sizes = await _db.SheetSizes
                .AsNoTracking()
                .Where(s => s.ProductLineId == colour.ProductLineId)
                .Select(s => s.Id)
                .ToListAsync();

            var anyResolved = false;
            foreach (var sizeId in sizes)
            {
                var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, sizeId), Today);
                if (result.Resolved) { anyResolved = true; break; }
            }

            if (!anyResolved) unresolved.Add(colour.Name);
        }

        Assert.True(unresolved.Count == 0,
            "These seeded colours cannot be priced: " + string.Join(", ", unresolved));
    }

    [Fact]
    public async Task A_staron_band_price_matches_the_published_sheet_price()
    {
        var colour = await _db.Colours
            .AsNoTracking()
            .FirstAsync(c => c.Name == "Supreme");

        var size = await _db.SheetSizes
            .AsNoTracking()
            .FirstAsync(s => s.ProductLineId == colour.ProductLineId && s.LengthMm == 3680 && s.WidthMm == 760);

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.True(result.Resolved);
        Assert.Equal(7719.17m, result.UnitPrice);   // published on the March 2025 list
    }

    [Fact]
    public async Task A_max_on_top_item_price_matches_the_published_sheet_price()
    {
        var colour = await _db.Colours
            .AsNoTracking()
            .FirstAsync(c => c.SupplierCode == "WSOLIDSURFACE80167");

        var size = await _db.SheetSizes
            .AsNoTracking()
            .FirstAsync(s => s.ProductLineId == colour.ProductLineId && s.LengthMm == 3680 && s.WidthMm == 760);

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.True(result.Resolved);
        Assert.Equal(3991.00m, result.UnitPrice);   // published on the August 2026 list
    }

    [Fact]
    public async Task Adhesive_is_priced_per_supplier_and_not_as_one_global_rate()
    {
        // R130 from two suppliers, R250 from Woodcentre. The seamkit rate follows
        // the supplier of the material being quoted.
        var adhesive = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == "Adhesive and seamkit");

        var prices = await _db.RatePrices
            .AsNoTracking()
            .Where(p => p.RateItemId == adhesive.Id)
            .Join(_db.Suppliers.AsNoTracking(), p => p.SupplierId, s => s.Id, (p, s) => new { s.Name, p.Amount })
            .ToListAsync();

        Assert.Equal(250.00m, prices.Single(p => p.Name == "Woodcentre CPT").Amount);
        Assert.Equal(130.00m, prices.Single(p => p.Name == "Max on Top").Amount);
        Assert.True(prices.Select(p => p.Amount).Distinct().Count() > 1);
    }

    [Fact]
    public async Task Overtime_is_expressed_as_a_multiple_of_fabrication_rather_than_a_separate_figure()
    {
        // The spreadsheet's core weakness was a rate card duplicated across twelve
        // sheets that drifted apart. The relationship is held once.
        var fabrication = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == "Fabrication");
        var overtime = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == "Fabrication overtime");

        Assert.Equal(fabrication.Id, overtime.DerivedFromRateItemId);
        Assert.Equal(1.5m, overtime.DerivedFromRateItemMultiplier);
    }

    [Fact]
    public async Task The_rates_awaiting_the_client_are_seeded_without_a_price()
    {
        // These figures are not in any document we hold. They are left unpriced so
        // the system reports them as unresolved rather than inventing a plausible
        // number, which is the failure the project exists to remove.
        foreach (var name in RateCardSeeder.AwaitingClientRates)
        {
            var item = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == name);
            var hasPrice = await _db.RatePrices.AsNoTracking().AnyAsync(p => p.RateItemId == item.Id);

            Assert.False(hasPrice, $"{name} should have no seeded price until the client supplies one.");
        }
    }
}
