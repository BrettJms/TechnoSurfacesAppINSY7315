namespace TechnoSurfacesApp.Models;

/// <summary>
/// Who Techno Surfaces buys from and pays. Modelled separately from ProductLine
/// (what the material is called) because Salvocorp appear to distribute Staron,
/// Perago and Magicstone - so five brands may be four commercial relationships.
/// </summary>
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? TradingAs { get; set; }
    public PricingStructure PricingStructure { get; set; }

    /// <summary>Date on the supplier's current price list. Surfaced in the UI - a
    /// June 2023 list is a business risk worth showing.</summary>
    public DateTime PriceListDated { get; set; }

    /// <summary>Seamkit/adhesive price is NOT one global rate - it follows the
    /// supplier of the material being quoted. R130 / R250 / R299 in practice.</summary>
    public decimal AdhesivePrice { get; set; }

    public string DeliveryTerms { get; set; } = "";
    public string? Notes { get; set; }

    public bool IsStale => (DateTime.Today - PriceListDated).TotalDays > 365;
}

/// <summary>
/// A named material range within a supplier, e.g. "Infinito Full Acrylic".
/// Part of the price key: ASPEN CORAL is band A3 under Full Acrylic but M2 under
/// Modified, so the product line genuinely changes the price.
/// </summary>
public class ProductLine
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>
/// An optional grouping some suppliers apply to their colours. Never the sole
/// holder of a price - it is a display and bulk-update convenience.
/// </summary>
public class PriceBand
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal PricePerSqm { get; set; }
}

/// <summary>
/// One priced row of the catalogue. The natural key that works for all five
/// suppliers is (Supplier, ProductLine, Colour, SheetWidth, Thickness) - which is
/// why size and thickness are on this class and not on some parent.
/// </summary>
public class CatalogueEntry
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int ProductLineId { get; set; }

    public string ColourName { get; set; } = "";

    /// <summary>The supplier's own product code - this is what gets used when ordering.</summary>
    public string SupplierCode { get; set; } = "";

    /// <summary>Named colour range, e.g. SOLID / NEBULA / STELLA / MET. Display grouping only.</summary>
    public string? Range { get; set; }

    /// <summary>Band code where the supplier prices by band (A1-A4, M1-M4, "Supreme"...). Null for per-item suppliers.</summary>
    public string? BandCode { get; set; }

    public int SheetLengthMm { get; set; }
    public int SheetWidthMm { get; set; }
    public int ThicknessMm { get; set; }

    /// <summary>Set where the supplier publishes a per-square-metre rate.</summary>
    public decimal? PricePerSqm { get; set; }

    /// <summary>Set where the supplier publishes a per-sheet price instead.</summary>
    public decimal? PublishedPricePerSheet { get; set; }

    /// <summary>Live stock, where the supplier publishes it. Fractional - Woodcentre list Soft White at 63.5.</summary>
    public decimal? StockQty { get; set; }

    public CatalogueStatus Status { get; set; } = CatalogueStatus.Active;
    public DateTime EffectiveFrom { get; set; }

    // ---- Derived ----

    public decimal SheetAreaSqm =>
        Math.Round((SheetLengthMm / 1000m) * (SheetWidthMm / 1000m), 4);

    /// <summary>price per sheet = price per m2 x area. Verified against every
    /// supplier that publishes both figures.</summary>
    public decimal EffectivePricePerSheet =>
        PublishedPricePerSheet ?? Math.Round((PricePerSqm ?? 0m) * SheetAreaSqm, 2);

    public decimal EffectivePricePerSqm =>
        PricePerSqm ?? (SheetAreaSqm == 0 ? 0m : Math.Round(PublishedPricePerSheet!.Value / SheetAreaSqm, 2));

    /// <summary>Shown in the UI so the estimator can see where a number came from.</summary>
    public string PriceBasis => PublishedPricePerSheet.HasValue
        ? "Supplier publishes price per sheet"
        : "Derived from price per m\u00B2 \u00D7 sheet area";

    public string SizeLabel => $"{SheetLengthMm} \u00D7 {SheetWidthMm} \u00D7 {ThicknessMm}mm";
    public string ShortSizeLabel => $"{SheetLengthMm} \u00D7 {SheetWidthMm}";

    public string StatusLabel => Status switch
    {
        CatalogueStatus.Active => "Active",
        CatalogueStatus.PhasingOut => "Phasing out",
        _ => "Discontinued"
    };
}