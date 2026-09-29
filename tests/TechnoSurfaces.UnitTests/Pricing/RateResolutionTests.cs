using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Catalogue;
using TechnoSurfaces.Infrastructure.Data;
using TechnoSurfaces.Infrastructure.Data.Seed;

namespace TechnoSurfaces.UnitTests.Pricing;

/// <summary>
/// Covers the rate card: supplier-specific amounts, and rates that are derived from
/// another rate rather than maintained separately.
/// </summary>
public sealed class RateResolutionTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private TechnoSurfacesDbContext _db = null!;
    private IRateResolver _rates = null!;

    private static readonly DateOnly Today = new(2026, 9, 29);

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _db = new TechnoSurfacesDbContext(
            new DbContextOptionsBuilder<TechnoSurfacesDbContext>().UseSqlite(_connection).Options);
        await _db.Database.EnsureCreatedAsync();

        await CatalogueSeeder.SeedAsync(_db);
        await RateCardSeeder.SeedAsync(_db);

        _rates = new RateResolver(new CatalogueReader(_db));
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<int> RateIdAsync(string name) =>
        (await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == name)).Id;

    private async Task<int> SupplierIdAsync(string name) =>
        (await _db.Suppliers.AsNoTracking().FirstAsync(s => s.Name == name)).Id;

    [Fact]
    public async Task A_supplier_specific_rate_is_used_in_place_of_the_general_one()
    {
        // Adhesive is R130 from Max on Top and R250 from Woodcentre. The seamkit
        // rate follows the supplier of the material being quoted.
        var adhesive = await RateIdAsync("Adhesive and seamkit");

        var maxOnTop = await _rates.ResolveAsync(adhesive, await SupplierIdAsync("Max on Top"), Today);
        var woodcentre = await _rates.ResolveAsync(adhesive, await SupplierIdAsync("Woodcentre CPT"), Today);

        Assert.Equal(130.00m, maxOnTop.UnitPrice);
        Assert.Equal(250.00m, woodcentre.UnitPrice);
    }

    [Fact]
    public async Task Overtime_resolves_as_one_and_a_half_times_the_fabrication_rate()
    {
        // The relationship is held once. Setting the fabrication rate carries
        // through to overtime rather than being applied in two places.
        var fabrication = await RateIdAsync("Fabrication");
        _db.RatePrices.Add(new RatePrice
        {
            RateItemId = fabrication,
            Amount = 450.00m,
            EffectiveFrom = new DateOnly(2026, 1, 1)
        });
        await _db.SaveChangesAsync();

        var overtime = await _rates.ResolveAsync(await RateIdAsync("Fabrication overtime"), null, Today);

        Assert.True(overtime.Resolved);
        Assert.Equal(675.00m, overtime.UnitPrice);          // 450,00 x 1,5
        Assert.Contains("Fabrication x 1.5", overtime.Origin);
    }

    [Fact]
    public async Task Installation_mirrors_the_fabrication_rate()
    {
        var fabrication = await RateIdAsync("Fabrication");
        _db.RatePrices.Add(new RatePrice
        {
            RateItemId = fabrication,
            Amount = 380.00m,
            EffectiveFrom = new DateOnly(2026, 1, 1)
        });
        await _db.SaveChangesAsync();

        var installation = await _rates.ResolveAsync(await RateIdAsync("Installation"), null, Today);

        Assert.Equal(380.00m, installation.UnitPrice);
        Assert.Contains("mirrors", installation.Origin);
    }

    [Fact]
    public async Task Changing_the_fabrication_rate_carries_through_to_overtime()
    {
        var fabrication = await RateIdAsync("Fabrication");
        var overtimeId = await RateIdAsync("Fabrication overtime");

        _db.RatePrices.Add(new RatePrice { RateItemId = fabrication, Amount = 400.00m, EffectiveFrom = new DateOnly(2026, 1, 1), EffectiveTo = new DateOnly(2026, 6, 30) });
        _db.RatePrices.Add(new RatePrice { RateItemId = fabrication, Amount = 500.00m, EffectiveFrom = new DateOnly(2026, 7, 1) });
        await _db.SaveChangesAsync();

        var before = await _rates.ResolveAsync(overtimeId, null, new DateOnly(2026, 3, 1));
        var after = await _rates.ResolveAsync(overtimeId, null, Today);

        Assert.Equal(600.00m, before.UnitPrice);   // 400,00 x 1,5
        Assert.Equal(750.00m, after.UnitPrice);    // 500,00 x 1,5
    }

    [Fact]
    public async Task A_rate_the_client_has_not_supplied_does_not_resolve_and_says_so()
    {
        // The fabrication, installation and sanding rates are not in any document we
        // hold. They must report as unresolved rather than being invented.
        foreach (var name in RateCardSeeder.AwaitingClientRates)
        {
            var result = await _rates.ResolveAsync(await RateIdAsync(name), null, Today);

            Assert.False(result.Resolved);
            Assert.Throws<PriceNotResolvedException>(() => result.UnitPrice);
        }
    }

    [Fact]
    public async Task A_derived_rate_whose_base_is_unpriced_explains_which_rate_is_missing()
    {
        var result = await _rates.ResolveAsync(await RateIdAsync("Fabrication overtime"), null, Today);

        Assert.False(result.Resolved);
        Assert.Contains("Fabrication", result.FailureReason!);
    }

    [Fact]
    public async Task A_rate_card_configured_to_derive_from_itself_is_refused_rather_than_looping()
    {
        var a = new RateItem { Name = "Cycle A", Category = RateCategory.Fabrication, Unit = ChargeUnit.Hour };
        var b = new RateItem { Name = "Cycle B", Category = RateCategory.Fabrication, Unit = ChargeUnit.Hour };
        _db.RateItems.AddRange(a, b);
        await _db.SaveChangesAsync();

        a.DerivedFromRateItemId = b.Id;
        a.DerivedFromRateItemMultiplier = 1m;
        b.DerivedFromRateItemId = a.Id;
        b.DerivedFromRateItemMultiplier = 1m;
        await _db.SaveChangesAsync();

        var result = await _rates.ResolveAsync(a.Id, null, Today);

        Assert.False(result.Resolved);
        Assert.Contains("derives from itself", result.FailureReason!);
    }

    [Fact]
    public async Task Silicon_and_sealing_carries_a_factor_of_two()
    {
        // Sheets multiplied by two, subject to estimator override. A team
        // assumption, not client-confirmed.
        var silicon = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == "Silicon and sealing");
        var transport = await _db.RateItems.AsNoTracking().FirstAsync(r => r.Name == "Transport");

        Assert.Equal(DerivationRule.FromSheetCount, silicon.Derivation);
        Assert.Equal(2m, silicon.DerivationFactor);
        Assert.Equal(1m, transport.DerivationFactor);
    }
}
