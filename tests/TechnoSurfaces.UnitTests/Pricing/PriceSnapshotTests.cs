using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Costing;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Catalogue;
using TechnoSurfaces.Domain.Quoting;
using TechnoSurfaces.Infrastructure.Data;

namespace TechnoSurfaces.UnitTests.Pricing;

/// <summary>
/// US-22 and NFR-11: a quote keeps the prices it was created with, so reopening it
/// does not silently change the figures.
///
/// This exercises the behaviour end to end against a real catalogue: resolve a
/// price, build a line from it, then change the catalogue price underneath and
/// confirm the saved line and its totals do not move.
/// </summary>
public sealed class PriceSnapshotTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private TechnoSurfacesDbContext _db = null!;
    private IPriceResolver _resolver = null!;
    private readonly QuoteCalculationService _calculator = new();

    private static readonly DateOnly QuoteDate = new(2026, 9, 29);

    private int _colourId;
    private int _sizeId;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _db = new TechnoSurfacesDbContext(
            new DbContextOptionsBuilder<TechnoSurfacesDbContext>().UseSqlite(_connection).Options);
        await _db.Database.EnsureCreatedAsync();

        var supplier = new Supplier { Name = "Staron", PricingStructure = PricingStructure.Band, PriceListDated = new DateOnly(2025, 3, 1) };
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        var line = new ProductLine { SupplierId = supplier.Id, Name = "Staron", ThicknessMm = 12 };
        _db.ProductLines.Add(line);
        await _db.SaveChangesAsync();

        var size = new SheetSize { ProductLineId = line.Id, LengthMm = 3680, WidthMm = 760 };
        var band = new PriceBand { SupplierId = supplier.Id, ProductLineId = line.Id, Code = "Bright White", Name = "Bright White" };
        _db.SheetSizes.Add(size);
        _db.PriceBands.Add(band);
        await _db.SaveChangesAsync();

        var colour = new Colour { ProductLineId = line.Id, PriceBandId = band.Id, Name = "Bright White", SupplierCode = "BW" };
        _db.Colours.Add(colour);
        _db.MaterialPrices.Add(new MaterialPrice
        {
            PriceBandId = band.Id,
            SheetSizeId = size.Id,
            PricePerSqm = 1550.00m,
            EffectiveFrom = new DateOnly(2025, 3, 1)
        });
        await _db.SaveChangesAsync();

        _colourId = colour.Id;
        _sizeId = size.Id;

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
    public async Task A_catalogue_price_increase_does_not_move_an_existing_quote()
    {
        var resolution = await _resolver.ResolveAsync(new PriceKey(_colourId, _sizeId), QuoteDate);
        Assert.Equal(4335.04m, resolution.UnitPrice);

        var version = new QuoteVersion(versionNo: 1, createdByUserId: "estimator", markupPercent: 20m);
        version.AddCostingLine(CostingLine.ForMaterial(
            materialPriceId: 1,
            description: "Staron Bright White 3680 x 760",
            resolvedUnitPrice: resolution.UnitPrice,
            priceOrigin: resolution.Origin,
            quantity: 2m,
            sheetAreaM2: 2.7968m));

        var totalWhenQuoted = _calculator.Calculate(version).TotalIncVat;

        // The Managing Director puts the price up by roughly sixteen per cent.
        var current = await _db.MaterialPrices.FirstAsync(p => p.PriceBandId != null);
        current.PricePerSqm = 1800.00m;
        await _db.SaveChangesAsync();

        // A new quote picks up the increase.
        var today = await _resolver.ResolveAsync(new PriceKey(_colourId, _sizeId), QuoteDate);
        Assert.Equal(5034.24m, today.UnitPrice);

        // The existing quote does not.
        var totalNow = _calculator.Calculate(version).TotalIncVat;

        Assert.Equal(totalWhenQuoted, totalNow);
        Assert.Equal(4335.04m, version.CostingLines.Single().ResolvedUnitPrice);
    }

    [Fact]
    public async Task A_sealed_version_cannot_be_altered_by_a_later_revision()
    {
        var resolution = await _resolver.ResolveAsync(new PriceKey(_colourId, _sizeId), QuoteDate);

        var quote = new Quote("Q-2026-0001", customerId: 1, contactId: 1, createdByUserId: "estimator", issueDate: QuoteDate);
        var first = quote.StartNewVersion("estimator", markupPercent: 20m);
        first.AddCostingLine(CostingLine.ForMaterial(
            materialPriceId: 1, description: "Staron Bright White", resolvedUnitPrice: resolution.UnitPrice,
            priceOrigin: resolution.Origin, quantity: 2m, sheetAreaM2: 2.7968m));

        var originalTotal = _calculator.Calculate(first).TotalIncVat;

        // The customer counter-offers, so the quote is reopened and revised.
        var second = quote.StartNewVersion("estimator", markupPercent: 12m);
        second.AddCostingLine(CostingLine.ForMaterial(
            materialPriceId: 1, description: "Staron Bright White", resolvedUnitPrice: resolution.UnitPrice,
            priceOrigin: resolution.Origin, quantity: 2m, sheetAreaM2: 2.7968m));

        Assert.Equal(2, quote.Versions.Count);
        Assert.True(first.IsSealed);
        Assert.Equal(originalTotal, _calculator.Calculate(first).TotalIncVat);
        Assert.NotEqual(originalTotal, _calculator.Calculate(second).TotalIncVat);

        // The original offer is a read-only record from here on.
        Assert.Throws<InvalidOperationException>(() => first.SetMarkupPercent(5m));
        Assert.Throws<InvalidOperationException>(() => first.AddCostingLine(
            CostingLine.ForRate(1, "Fabrication", 450m, "Rate card", 1m, false)));
    }
}
