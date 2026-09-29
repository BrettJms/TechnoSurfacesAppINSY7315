using TechnoSurfaces.Domain.Catalogue;

namespace TechnoSurfaces.Domain.Quoting;

/// <summary>
/// A single line of the internal costing: what was used, in what quantity, at what
/// unit cost, and with any discount received from the supplier. Never shown to the
/// customer.
///
/// The resolved unit price is copied onto the line when it is created. A later
/// catalogue change therefore cannot alter an existing quote, which is US-22 and
/// NFR-11. <see cref="MaterialPriceId"/> is retained only so the price row that was
/// used can be traced; it is not read back when totalling.
/// </summary>
public class CostingLine
{
    private CostingLine() { }

    private CostingLine(
        CostingLineType lineType,
        string description,
        decimal resolvedUnitPrice,
        string priceOrigin,
        decimal quantity,
        decimal supplierDiscountPercent,
        bool isBelowTheLine,
        DerivationRule derivation)
    {
        if (resolvedUnitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(resolvedUnitPrice), "A unit price cannot be negative.");
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantity cannot be negative.");
        if (supplierDiscountPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(supplierDiscountPercent), "A discount must be between 0 and 100 per cent.");
        if (string.IsNullOrWhiteSpace(priceOrigin))
            throw new ArgumentException("Every priced line must record where its price came from.", nameof(priceOrigin));

        LineType = lineType;
        Description = description;
        ResolvedUnitPrice = resolvedUnitPrice;
        PriceOrigin = priceOrigin;
        Quantity = quantity;
        SupplierDiscountPercent = supplierDiscountPercent;
        IsBelowTheLine = isBelowTheLine;
        Derivation = derivation;
    }

    public int Id { get; private set; }
    public int QuoteVersionId { get; private set; }

    public CostingLineType LineType { get; private set; }

    /// <summary>Set on a material line. Traceability only; the price is copied below.</summary>
    public int? MaterialPriceId { get; private set; }

    /// <summary>Set on a rate line.</summary>
    public int? RateItemId { get; private set; }

    /// <summary>What the estimator sees. Captured at creation so it survives a catalogue change.</summary>
    public string Description { get; private set; } = "";

    /// <summary>
    /// The price actually used, copied from the catalogue at creation. This is what
    /// the calculator reads.
    /// </summary>
    public decimal ResolvedUnitPrice { get; private set; }

    /// <summary>
    /// Where the price came from, shown on the line. NFR-01 requires the origin to
    /// be visible, for example "Staron price band Supreme, 3680 x 760, effective
    /// 2025-03-01".
    /// </summary>
    public string PriceOrigin { get; private set; } = "";

    /// <summary>
    /// Not an integer. Woodcentre quote stock in half sheets, and area-derived
    /// quantities are fractional.
    /// </summary>
    public decimal Quantity { get; private set; }

    public decimal SupplierDiscountPercent { get; private set; }

    /// <summary>
    /// Snapshot of the rate item's below-the-line flag. Items below the line are
    /// cost recovery and are not marked up.
    /// </summary>
    public bool IsBelowTheLine { get; private set; }

    public DerivationRule Derivation { get; private set; }

    /// <summary>Sheet area, on a material line. Drives the area-derived quantities.</summary>
    public decimal? SheetAreaM2 { get; private set; }

    public int SortOrder { get; set; }

    /// <summary>
    /// effectiveUnitPrice = resolvedUnitPrice x (1 - supplierDiscountPercent / 100)
    /// </summary>
    public decimal EffectiveUnitPrice() =>
        decimal.Round(ResolvedUnitPrice * (1m - SupplierDiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// lineTotal = effectiveUnitPrice x quantity
    /// </summary>
    public decimal LineTotal() =>
        decimal.Round(EffectiveUnitPrice() * Quantity, 2, MidpointRounding.AwayFromZero);

    /// <summary>Total area contributed by this line, used for derived quantities.</summary>
    public decimal AreaM2() =>
        LineType == CostingLineType.Material && SheetAreaM2 is not null
            ? decimal.Round(SheetAreaM2.Value * Quantity, 4)
            : 0m;

    /// <summary>Sheets contributed by this line, used for derived quantities.</summary>
    public decimal SheetCount() =>
        LineType == CostingLineType.Material ? Quantity : 0m;

    public static CostingLine ForMaterial(
        int materialPriceId,
        string description,
        decimal resolvedUnitPrice,
        string priceOrigin,
        decimal quantity,
        decimal sheetAreaM2,
        decimal supplierDiscountPercent = 0m)
    {
        var line = new CostingLine(
            CostingLineType.Material, description, resolvedUnitPrice, priceOrigin,
            quantity, supplierDiscountPercent, isBelowTheLine: false, DerivationRule.Entered)
        {
            MaterialPriceId = materialPriceId,
            SheetAreaM2 = sheetAreaM2
        };
        return line;
    }

    public static CostingLine ForRate(
        int rateItemId,
        string description,
        decimal resolvedUnitPrice,
        string priceOrigin,
        decimal quantity,
        bool isBelowTheLine,
        DerivationRule derivation = DerivationRule.Entered)
    {
        var line = new CostingLine(
            CostingLineType.Rate, description, resolvedUnitPrice, priceOrigin,
            quantity, supplierDiscountPercent: 0m, isBelowTheLine, derivation)
        {
            RateItemId = rateItemId
        };
        return line;
    }

    /// <summary>
    /// Sets a quantity that follows from the job rather than from the estimator.
    /// Used by the calculator before totals are computed.
    /// </summary>
    public void SetDerivedQuantity(decimal quantity)
    {
        if (Derivation == DerivationRule.Entered)
            throw new InvalidOperationException("An entered quantity is not derived.");
        Quantity = quantity;
    }
}
