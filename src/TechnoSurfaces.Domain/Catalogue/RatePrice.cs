namespace TechnoSurfaces.Domain.Catalogue;

/// <summary>
/// The amount charged for a rate item over a period, optionally varying by
/// supplier. Adhesive is R130 from two suppliers, R250 from a third and R299 from
/// Max on Top when used on another supplier's material, so the seamkit rate is not
/// a single global figure.
///
/// Rates change over time and the history is retained so that historic quotes
/// remain explicable.
/// </summary>
public class RatePrice
{
    public int Id { get; set; }

    public int RateItemId { get; set; }
    public RateItem? RateItem { get; set; }

    /// <summary>Null where the rate does not vary by supplier, which is the usual case.</summary>
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public decimal Amount { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    /// <summary>Null means currently in force.</summary>
    public DateOnly? EffectiveTo { get; set; }

    public bool IsInForceOn(DateOnly date) =>
        EffectiveFrom <= date && (EffectiveTo is null || EffectiveTo >= date);
}
