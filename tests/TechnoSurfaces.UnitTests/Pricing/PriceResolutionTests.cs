using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Catalogue;
using TechnoSurfaces.Infrastructure.Data;

namespace TechnoSurfaces.UnitTests.Pricing;

/// <summary>
/// Exercises the pricing strategies against the real EF Core queries on a SQLite
/// database held in memory, rather than against a hand-written fake. The
/// effective-dating behaviour is in the query, so testing it against a stub would
/// only prove the stub works.
/// </summary>
public sealed class PriceResolutionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TechnoSurfacesDbContext _db;
    private readonly IPriceResolver _resolver;

    public PriceResolutionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TechnoSurfacesDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new TechnoSurfacesDbContext(options);
        _db.Database.EnsureCreated();

        var catalogue = new CatalogueReader(_db);
        _resolver = new PriceResolver(catalogue, new IPriceResolutionStrategy[]
        {
            new BandPricedStrategy(catalogue),
            new ItemPricedStrategy(catalogue)
        });
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static readonly DateOnly Today = new(2026, 9, 29);

    // --------------------------------------------------------------- fixtures

    private (Colour Colour, SheetSize Size) GivenBandPricedMaterial(
        decimal perSqm, int lengthMm = 3680, int widthMm = 760, DateOnly? from = null, DateOnly? to = null)
    {
        var supplier = new Supplier { Name = $"Staron {Guid.NewGuid():N}", PricingStructure = PricingStructure.Band, PriceListDated = new DateOnly(2025, 3, 1) };
        _db.Suppliers.Add(supplier);
        _db.SaveChanges();

        var line = new ProductLine { SupplierId = supplier.Id, Name = "Staron", ThicknessMm = 12 };
        _db.ProductLines.Add(line);
        _db.SaveChanges();

        var size = new SheetSize { ProductLineId = line.Id, LengthMm = lengthMm, WidthMm = widthMm };
        var band = new PriceBand { SupplierId = supplier.Id, ProductLineId = line.Id, Code = "Bright White", Name = "Bright White" };
        _db.SheetSizes.Add(size);
        _db.PriceBands.Add(band);
        _db.SaveChanges();

        var colour = new Colour { ProductLineId = line.Id, PriceBandId = band.Id, Name = "Bright White", SupplierCode = "BW" };
        _db.Colours.Add(colour);
        _db.MaterialPrices.Add(new MaterialPrice
        {
            PriceBandId = band.Id,
            SheetSizeId = size.Id,
            PricePerSqm = perSqm,
            EffectiveFrom = from ?? new DateOnly(2025, 3, 1),
            EffectiveTo = to
        });
        _db.SaveChanges();

        return (colour, size);
    }

    private (Colour Colour, SheetSize Size) GivenItemPricedMaterial(decimal sheetPrice, int lengthMm = 3680, int widthMm = 760)
    {
        var supplier = new Supplier { Name = $"Max on Top {Guid.NewGuid():N}", PricingStructure = PricingStructure.Item, PriceListDated = new DateOnly(2026, 8, 3) };
        _db.Suppliers.Add(supplier);
        _db.SaveChanges();

        var line = new ProductLine { SupplierId = supplier.Id, Name = "Max Pure Solid Surface", ThicknessMm = 12 };
        _db.ProductLines.Add(line);
        _db.SaveChanges();

        var size = new SheetSize { ProductLineId = line.Id, LengthMm = lengthMm, WidthMm = widthMm };
        _db.SheetSizes.Add(size);
        _db.SaveChanges();

        var colour = new Colour { ProductLineId = line.Id, Name = "Glacier White 8016", SupplierCode = "WSOLIDSURFACE80167" };
        _db.Colours.Add(colour);
        _db.SaveChanges();

        _db.MaterialPrices.Add(new MaterialPrice
        {
            ColourId = colour.Id,
            SheetSizeId = size.Id,
            PricePerSqm = MaterialPrice.PerSqmFromSheetPrice(sheetPrice, size),
            EffectiveFrom = new DateOnly(2026, 8, 3)
        });
        _db.SaveChanges();

        return (colour, size);
    }

    // ------------------------------------------------------------------ tests

    [Fact]
    public async Task A_band_priced_colour_resolves_through_its_band()
    {
        // Staron Bright White 12mm: R1 550,00 per square metre over 2,7968 square
        // metres is R4 335,04, which is the figure published on the list.
        var (colour, size) = GivenBandPricedMaterial(perSqm: 1550.00m);

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.True(result.Resolved);
        Assert.Equal(4335.04m, result.UnitPrice);
        Assert.Contains("price band Bright White", result.Origin);
    }

    [Fact]
    public async Task An_item_priced_colour_resolves_directly_on_the_colour()
    {
        // Max on Top publish only a sheet price. It must round-trip to the cent.
        var (colour, size) = GivenItemPricedMaterial(sheetPrice: 3991m);

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.True(result.Resolved);
        Assert.Equal(3991.00m, result.UnitPrice);
        Assert.Contains("Glacier White", result.Origin);
    }

    [Fact]
    public async Task The_same_material_at_a_different_sheet_size_is_a_different_price()
    {
        // Glacier White 8016 is R3 991 at 3680 x 760 and R2 287 at 2440 x 920.
        var (narrow, narrowSize) = GivenItemPricedMaterial(sheetPrice: 3991m, 3680, 760);
        var (wide, wideSize) = GivenItemPricedMaterial(sheetPrice: 2287m, 2440, 920);

        var a = await _resolver.ResolveAsync(new PriceKey(narrow.Id, narrowSize.Id), Today);
        var b = await _resolver.ResolveAsync(new PriceKey(wide.Id, wideSize.Id), Today);

        Assert.Equal(3991.00m, a.UnitPrice);
        Assert.Equal(2287.00m, b.UnitPrice);
    }

    [Fact]
    public async Task A_colour_with_no_price_row_does_not_resolve_and_is_never_zero()
    {
        var (colour, size) = GivenItemPricedMaterial(sheetPrice: 3991m);

        // Remove the price, leaving the colour in the catalogue with nothing to
        // price it by. The spreadsheet would return a plausible number here.
        _db.MaterialPrices.RemoveRange(_db.MaterialPrices.Where(p => p.ColourId == colour.Id));
        _db.SaveChanges();

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.False(result.Resolved);
        Assert.NotNull(result.FailureReason);
        Assert.Throws<PriceNotResolvedException>(() => result.UnitPrice);
    }

    [Fact]
    public async Task A_price_that_has_been_superseded_is_not_returned()
    {
        // The row in force on the quote's date wins, not the newest row.
        var (colour, size) = GivenBandPricedMaterial(
            perSqm: 1550.00m,
            from: new DateOnly(2025, 3, 1),
            to: new DateOnly(2026, 2, 28));

        var band = _db.Colours.Single(c => c.Id == colour.Id).PriceBandId!.Value;
        _db.MaterialPrices.Add(new MaterialPrice
        {
            PriceBandId = band,
            SheetSizeId = size.Id,
            PricePerSqm = 1800.00m,
            EffectiveFrom = new DateOnly(2026, 3, 1)
        });
        _db.SaveChanges();

        var current = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);
        var historic = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), new DateOnly(2025, 6, 1));

        Assert.Equal(5034.24m, current.UnitPrice);    // 1 800,00 x 2,7968
        Assert.Equal(4335.04m, historic.UnitPrice);   // 1 550,00 x 2,7968
    }

    [Fact]
    public async Task A_quote_dated_before_any_price_existed_does_not_resolve()
    {
        var (colour, size) = GivenBandPricedMaterial(perSqm: 1550.00m, from: new DateOnly(2025, 3, 1));

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), new DateOnly(2024, 1, 1));

        Assert.False(result.Resolved);
    }

    [Fact]
    public async Task A_discontinued_colour_still_resolves_on_an_existing_quote()
    {
        // Retired, never deleted. A discontinued colour cannot be chosen on a new
        // quote, but a quote that already references it must stay readable.
        var (colour, size) = GivenItemPricedMaterial(sheetPrice: 4671m);

        var tracked = _db.Colours.Single(c => c.Id == colour.Id);
        tracked.Status = CatalogueStatus.Discontinued;
        _db.SaveChanges();

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.True(result.Resolved);
        Assert.False(tracked.IsSelectable);
    }

    [Fact]
    public async Task A_band_priced_colour_with_no_band_assigned_reports_why()
    {
        var (colour, size) = GivenBandPricedMaterial(perSqm: 1550.00m);

        var tracked = _db.Colours.Single(c => c.Id == colour.Id);
        tracked.PriceBandId = null;
        _db.SaveChanges();

        var result = await _resolver.ResolveAsync(new PriceKey(colour.Id, size.Id), Today);

        Assert.False(result.Resolved);
        Assert.Contains("price band", result.FailureReason!);
    }

    [Fact]
    public async Task A_colour_that_is_not_in_the_catalogue_does_not_resolve()
    {
        var result = await _resolver.ResolveAsync(new PriceKey(999_999, 1), Today);

        Assert.False(result.Resolved);
    }

    [Fact]
    public void Price_per_sheet_is_the_per_square_metre_price_times_the_sheet_area()
    {
        // The identity every supplier list satisfies, verified to the cent against
        // the published Staron, Perago and Surface Studio figures.
        var cases = new (int Length, int Width, decimal PerSqm, decimal Expected)[]
        {
            (3680, 760, 1550.00m, 4335.04m),   // Staron Bright White 12mm
            (3680, 760, 2760.00m, 7719.17m),   // Staron Supreme 12mm
            (2500, 760, 1485.00m, 2821.50m),   // Staron Bright White 6mm
            (3660, 760, 1520.00m, 4228.03m),   // Perago Classic White 12mm
            (3680, 760, 1540.00m, 4307.07m),   // Surface Studio group A2
            (3680, 760, 1280.00m, 3579.90m)    // Surface Studio group M1
        };

        foreach (var (length, width, perSqm, expected) in cases)
        {
            var size = new SheetSize { LengthMm = length, WidthMm = width };
            var price = new MaterialPrice { PricePerSqm = perSqm };

            Assert.Equal(expected, price.PricePerSheet(size));
        }
    }
}
