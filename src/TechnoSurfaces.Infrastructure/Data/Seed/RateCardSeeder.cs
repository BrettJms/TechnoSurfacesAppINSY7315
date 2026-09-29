using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Catalogue;

namespace TechnoSurfaces.Infrastructure.Data.Seed;

/// <summary>
/// Seeds the rate card structure: the chargeable lines that are not material.
///
/// The rate card is held once here and applied to every quote. The spreadsheet's
/// core weakness was a rate card duplicated across twelve sheets that then drifted
/// apart, so overtime and installation are expressed as relationships to the
/// fabrication rate rather than as separately maintained figures.
///
/// Amounts are seeded only where a real figure exists in a supplier price list.
/// The labour, sanding, cut-out and wood rates are not in any document we hold: the
/// rand values in the client's January costing workbook are sample data. Those
/// items are therefore seeded without a price, so the system reports them as
/// unresolved rather than pricing them at a plausible but invented figure. The
/// Managing Director enters the real rates through the rate card screen.
/// </summary>
public static class RateCardSeeder
{
    /// <summary>
    /// Rate items left deliberately unpriced, pending the client's real figures.
    /// Surfaced here so the gap is visible rather than buried in the data.
    /// </summary>
    public static readonly string[] AwaitingClientRates =
    {
        "Fabrication", "Fabrication overtime", "Installation", "Sanding and polishing",
        "Cut-out", "Wood substrate", "Sink", "Tap hole"
    };

    public static async Task SeedAsync(TechnoSurfacesDbContext db, CancellationToken ct = default)
    {
        if (await db.RateItems.AnyAsync(ct)) return;

        var effective = new DateOnly(2026, 1, 1);

        // ---- Labour, priced by the client through the rate card screen ----

        var fabrication = Add(db, "Fabrication", RateCategory.Fabrication, ChargeUnit.Hour);
        await db.SaveChangesAsync(ct);

        // Overtime is normal fabrication multiplied by 1,5 and installation mirrors
        // fabrication. Expressed as rules so that a change to the fabrication rate
        // carries through instead of being applied in several places.
        var overtime = Add(db, "Fabrication overtime", RateCategory.Fabrication, ChargeUnit.Hour);
        overtime.DerivedFromRateItemId = fabrication.Id;
        overtime.DerivedFromRateItemMultiplier = 1.5m;

        var installation = Add(db, "Installation", RateCategory.Installation, ChargeUnit.Hour);
        installation.DerivedFromRateItemId = fabrication.Id;
        installation.DerivedFromRateItemMultiplier = 1.0m;

        Add(db, "Sanding and polishing", RateCategory.Fabrication, ChargeUnit.Hour);
        Add(db, "Cut-out", RateCategory.Extras, ChargeUnit.Each);
        Add(db, "Wood substrate", RateCategory.Wood, ChargeUnit.SquareMetre);
        Add(db, "Sink", RateCategory.SinksAndHardware, ChargeUnit.Each);
        Add(db, "Tap hole", RateCategory.SinksAndHardware, ChargeUnit.Each);
        await db.SaveChangesAsync(ct);

        // ---- Derived quantities, reproducing the spreadsheet's behaviour ----

        var consumables = Add(db, "Sandpaper and consumables", RateCategory.Extras, ChargeUnit.SquareMetre,
            derivation: DerivationRule.FromTotalAreaM2, belowTheLine: true);

        var transport = Add(db, "Transport", RateCategory.Extras, ChargeUnit.Sheet,
            derivation: DerivationRule.FromSheetCount, belowTheLine: true);

        // Quantity is sheets multiplied by two, subject to estimator override. This
        // is a team assumption and has not been confirmed by the client.
        var silicon = Add(db, "Silicon and sealing", RateCategory.Extras, ChargeUnit.Each,
            derivation: DerivationRule.FromSheetCount, belowTheLine: true);

        // ---- Adhesive, where the price genuinely varies by supplier ----

        var adhesive = Add(db, "Adhesive and seamkit", RateCategory.Extras, ChargeUnit.Each, belowTheLine: true);
        await db.SaveChangesAsync(ct);

        // R130 from two suppliers, R250 from a third, R299 from Max on Top when
        // used on another supplier's material. The seamkit rate is not one global
        // figure; it follows the supplier of the material being quoted.
        var suppliers = await db.Suppliers.AsNoTracking().ToListAsync(ct);
        foreach (var s in suppliers)
            db.RatePrices.Add(new RatePrice
            {
                RateItemId = adhesive.Id,
                SupplierId = s.Id,
                Amount = s.AdhesivePrice,
                EffectiveFrom = effective
            });

        // ---- Transport thresholds published by the suppliers ----

        var maxOnTop = suppliers.FirstOrDefault(s => s.Name == "Max on Top");
        if (maxOnTop is not null)
            db.RatePrices.Add(new RatePrice { RateItemId = transport.Id, SupplierId = maxOnTop.Id, Amount = 1050.00m, EffectiveFrom = effective });

        var surfaceStudio = suppliers.FirstOrDefault(s => s.Name == "Surface Studio");
        if (surfaceStudio is not null)
            db.RatePrices.Add(new RatePrice { RateItemId = transport.Id, SupplierId = surfaceStudio.Id, Amount = 550.00m, EffectiveFrom = effective });

        var perago = suppliers.FirstOrDefault(s => s.Name == "Perago and Magicstone");
        if (perago is not null)
            db.RatePrices.Add(new RatePrice { RateItemId = transport.Id, SupplierId = perago.Id, Amount = 510.00m, EffectiveFrom = effective });

        await db.SaveChangesAsync(ct);

        _ = consumables;
        _ = silicon;
    }

    private static RateItem Add(
        TechnoSurfacesDbContext db, string name, RateCategory category, ChargeUnit unit,
        DerivationRule derivation = DerivationRule.Entered, bool belowTheLine = false)
    {
        var item = new RateItem
        {
            Name = name,
            Category = category,
            Unit = unit,
            Derivation = derivation,
            IsBelowTheLine = belowTheLine
        };
        db.RateItems.Add(item);
        return item;
    }
}
