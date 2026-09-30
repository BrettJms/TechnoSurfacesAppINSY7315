namespace TechnoSurfaces.Domain.Catalogue;

/// <summary>
/// A physical sheet size in which a product line is supplied. Held explicitly
/// because sheet dimensions differ materially between suppliers and the sheet area
/// drives every derived figure in the costing.
///
/// Staron and Surface Studio sheets are 3680mm long; Perago sheets are 3660mm.
/// Using one length for everything introduces roughly a half per cent error on
/// every material line.
/// </summary>
public class SheetSize
{
    public int Id { get; set; }
    public int ProductLineId { get; set; }
    public ProductLine? ProductLine { get; set; }

    /// <summary>Millimetres.</summary>
    public int LengthMm { get; set; }

    /// <summary>Millimetres.</summary>
    public int WidthMm { get; set; }

    /// <summary>
    /// Sheet area in square metres, derived from the dimensions. The supplier price
    /// lists satisfy the identity
    /// <c>price per sheet = price per square metre x (length x width)</c>, verified
    /// to the cent against Staron, Perago and Surface Studio.
    /// </summary>
    public decimal AreaM2 => decimal.Round(LengthMm / 1000m * (WidthMm / 1000m), 4);

    public override string ToString() => $"{LengthMm} x {WidthMm}";
}
