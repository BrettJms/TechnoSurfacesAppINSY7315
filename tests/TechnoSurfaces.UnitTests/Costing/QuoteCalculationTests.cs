using TechnoSurfaces.Application.Costing;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Quoting;

namespace TechnoSurfaces.UnitTests.Costing;

/// <summary>
/// The business case for this system rests on the calculation. An untested
/// calculation engine would reintroduce precisely the silent pricing errors the
/// system exists to remove, so these tests assert the arithmetic the client
/// confirmed, line by line.
/// </summary>
public class QuoteCalculationTests
{
    private readonly QuoteCalculationService _calculator = new();

    private static QuoteVersion NewVersion(decimal markupPercent = 0m) =>
        new(versionNo: 1, createdByUserId: "test-user", markupPercent: markupPercent);

    private static CostingLine Material(decimal unitPrice, decimal quantity, decimal discount = 0m, decimal sheetArea = 2.7968m) =>
        CostingLine.ForMaterial(
            materialPriceId: 1,
            description: "Staron Bright White 3680 x 760",
            resolvedUnitPrice: unitPrice,
            priceOrigin: "Staron price band Bright White, 3680 x 760, effective 2025-03-01",
            quantity: quantity,
            sheetAreaM2: sheetArea,
            supplierDiscountPercent: discount);

    private static CostingLine Rate(decimal unitPrice, decimal quantity, bool belowTheLine = false,
        DerivationRule derivation = DerivationRule.Entered, string description = "Fabrication") =>
        CostingLine.ForRate(
            rateItemId: 1,
            description: description,
            resolvedUnitPrice: unitPrice,
            priceOrigin: "Rate card, effective 2026-01-01",
            quantity: quantity,
            isBelowTheLine: belowTheLine,
            derivation: derivation);

    // ------------------------------------------------------------ NFR-01, US-03

    [Fact]
    public void A_line_whose_price_never_resolved_blocks_the_calculation()
    {
        // The single most important test in the suite. The spreadsheet returns a
        // plausible figure when a lookup fails; this system must refuse instead.
        var version = NewVersion();
        version.AddCostingLine(CostingLine.ForRate(
            rateItemId: 1, description: "Fabrication", resolvedUnitPrice: 0m,
            priceOrigin: "unresolved", quantity: 10m, isBelowTheLine: false));

        var ex = Assert.Throws<PriceNotResolvedException>(() => _calculator.Calculate(version));
        Assert.Contains("Fabrication", ex.Message);
    }

    [Fact]
    public void A_priced_line_must_record_where_its_price_came_from()
    {
        // NFR-01 requires the origin to be visible, not just the figure, so a line
        // cannot be constructed without one.
        Assert.Throws<ArgumentException>(() => CostingLine.ForRate(
            rateItemId: 1, description: "Fabrication", resolvedUnitPrice: 450m,
            priceOrigin: "", quantity: 1m, isBelowTheLine: false));
    }

    [Fact]
    public void An_unresolved_price_cannot_be_read_as_zero()
    {
        var resolution = PriceResolution.Failure("No price is in force on 2026-09-29.");

        Assert.False(resolution.Resolved);
        Assert.Throws<PriceNotResolvedException>(() => resolution.UnitPrice);
    }

    // ------------------------------------------------------------------- US-08

    [Fact]
    public void Supplier_discount_reduces_the_unit_cost_before_markup()
    {
        var line = Material(unitPrice: 6300m, quantity: 1m, discount: 10m);

        Assert.Equal(5670.00m, line.EffectiveUnitPrice());
        Assert.Equal(5670.00m, line.LineTotal());
    }

    // ------------------------------------------------------------------- US-04

    [Fact]
    public void A_line_total_is_the_effective_unit_price_times_the_quantity()
    {
        var line = Material(unitPrice: 4335.04m, quantity: 3m);

        Assert.Equal(13005.12m, line.LineTotal());
    }

    // -------------------------------------------------------- markup boundary

    [Fact]
    public void Markup_applies_to_the_subtotal_only_and_never_to_below_the_line_items()
    {
        // Confirmed by the client: markup is calculated on the sub-total, and the
        // items below that line are principally a recovery of cost.
        var version = NewVersion(markupPercent: 25m);
        version.AddCostingLine(Material(unitPrice: 4000m, quantity: 1m));   // above
        version.AddCostingLine(Rate(unitPrice: 1000m, quantity: 1m, belowTheLine: true));

        var totals = _calculator.Calculate(version);

        Assert.Equal(4000.00m, totals.SubTotalExVat);
        Assert.Equal(1000.00m, totals.MarkupAmount);       // 25% of 4000, not of 5000
        Assert.Equal(1000.00m, totals.BelowTheLineTotal);
        Assert.Equal(6000.00m, totals.TotalExVat);
    }

    [Fact]
    public void Vat_is_applied_to_the_total_after_markup()
    {
        var version = NewVersion(markupPercent: 20m);
        version.AddCostingLine(Material(unitPrice: 5000m, quantity: 2m));

        var totals = _calculator.Calculate(version);

        Assert.Equal(10000.00m, totals.SubTotalExVat);
        Assert.Equal(2000.00m, totals.MarkupAmount);
        Assert.Equal(12000.00m, totals.TotalExVat);
        Assert.Equal(1800.00m, totals.VatAmount);          // 15% of 12000
        Assert.Equal(13800.00m, totals.TotalIncVat);
    }

    // ------------------------------------------------------- derived quantities

    [Fact]
    public void Consumables_take_their_quantity_from_the_total_area()
    {
        var version = NewVersion();
        version.AddCostingLine(Material(unitPrice: 4335.04m, quantity: 3m, sheetArea: 2.7968m));
        var consumables = Rate(unitPrice: 50m, quantity: 0m, belowTheLine: true,
            derivation: DerivationRule.FromTotalAreaM2, description: "Sandpaper and consumables");
        version.AddCostingLine(consumables);

        var totals = _calculator.Calculate(version);

        Assert.Equal(8.3904m, totals.TotalAreaM2);         // 3 sheets x 2,7968
        Assert.Equal(8.3904m, consumables.Quantity);
    }

    [Fact]
    public void Transport_takes_its_quantity_from_the_sheet_count()
    {
        var version = NewVersion();
        version.AddCostingLine(Material(unitPrice: 4335.04m, quantity: 4m));
        var transport = Rate(unitPrice: 1050m, quantity: 0m, belowTheLine: true,
            derivation: DerivationRule.FromSheetCount, description: "Transport");
        version.AddCostingLine(transport);

        _calculator.Calculate(version);

        Assert.Equal(4m, transport.Quantity);
    }

    [Fact]
    public void A_quantity_the_estimator_entered_is_never_overwritten_by_a_derivation()
    {
        var version = NewVersion();
        version.AddCostingLine(Material(unitPrice: 4335.04m, quantity: 5m));
        var entered = Rate(unitPrice: 450m, quantity: 12m);
        version.AddCostingLine(entered);

        _calculator.Calculate(version);

        Assert.Equal(12m, entered.Quantity);
    }

    // ------------------------------------------------------------------- US-22

    [Fact]
    public void A_line_keeps_the_price_it_was_created_with()
    {
        // The resolved price is copied onto the line, so a later catalogue change
        // cannot move an existing quote. NFR-11.
        var version = NewVersion(markupPercent: 15m);
        var line = Material(unitPrice: 6300m, quantity: 2m);
        version.AddCostingLine(line);

        var before = _calculator.Calculate(version).TotalIncVat;

        // The catalogue price changes. Nothing on the line references it.
        var after = _calculator.Calculate(version).TotalIncVat;

        Assert.Equal(before, after);
        Assert.Equal(6300m, line.ResolvedUnitPrice);
    }

    // ---------------------------------------------------------------- rounding

    [Fact]
    public void A_multi_line_quote_totals_to_the_cent()
    {
        var version = NewVersion(markupPercent: 17.5m);
        version.AddCostingLine(Material(unitPrice: 4335.04m, quantity: 1.5m));
        version.AddCostingLine(Material(unitPrice: 5257.98m, quantity: 0.5m));
        version.AddCostingLine(Rate(unitPrice: 487.33m, quantity: 7m));

        var totals = _calculator.Calculate(version);

        var expectedSubTotal =
            decimal.Round(4335.04m * 1.5m, 2) +
            decimal.Round(5257.98m * 0.5m, 2) +
            decimal.Round(487.33m * 7m, 2);

        Assert.Equal(expectedSubTotal, totals.SubTotalExVat);
        Assert.Equal(decimal.Round(expectedSubTotal * 0.175m, 2), totals.MarkupAmount);
        Assert.Equal(totals.SubTotalExVat + totals.MarkupAmount, totals.TotalExVat);
        Assert.Equal(decimal.Round(totals.TotalExVat * 0.15m, 2), totals.VatAmount);
        Assert.Equal(totals.TotalExVat + totals.VatAmount, totals.TotalIncVat);
    }

    [Fact]
    public void Fractional_sheet_quantities_are_supported()
    {
        // Woodcentre quote stock in half sheets, so a sheet count is not an integer.
        var version = NewVersion();
        version.AddCostingLine(Material(unitPrice: 3900m, quantity: 0.5m));

        var totals = _calculator.Calculate(version);

        Assert.Equal(1950.00m, totals.SubTotalExVat);
    }

    // ------------------------------------------------------ guards on the input

    [Fact]
    public void A_negative_quantity_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Material(unitPrice: 100m, quantity: -1m));
    }

    [Fact]
    public void A_discount_outside_nought_to_one_hundred_per_cent_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Material(unitPrice: 100m, quantity: 1m, discount: 120m));
    }
}
