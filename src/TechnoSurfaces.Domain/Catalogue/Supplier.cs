namespace TechnoSurfaces.Domain.Catalogue;

/// <summary>
/// An organisation Techno Surfaces buys material from and pays invoices to.
/// Modelled separately from the material brand because a single supplier may
/// distribute several brands: the Staron list and the Perago and Magicstone list
/// are both issued on Salvocorp letterhead.
/// </summary>
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>The name on the letterhead, where it differs from the brand.</summary>
    public string? TradingAs { get; set; }

    /// <summary>
    /// Selects the price resolution strategy for this supplier. See
    /// <c>IPriceResolutionStrategy</c> in the Application layer.
    /// </summary>
    public PricingStructure PricingStructure { get; set; }

    /// <summary>
    /// Date on the supplier's current price list. Surfaced in the interface because
    /// a list dated June 2023 is a business risk worth showing to the estimator.
    /// </summary>
    public DateOnly PriceListDated { get; set; }

    /// <summary>
    /// Adhesive and seamkit pricing follows the supplier of the material being
    /// quoted rather than a single global rate: R130, R250 and R299 all occur.
    /// </summary>
    public decimal AdhesivePrice { get; set; }

    public string DeliveryTerms { get; set; } = "";
    public string? Notes { get; set; }

    public ICollection<ProductLine> ProductLines { get; set; } = new List<ProductLine>();
    public ICollection<PriceBand> PriceBands { get; set; } = new List<PriceBand>();
}
