namespace TechnoSurfaces.Domain.Catalogue;

/// <summary>
/// A chargeable line that is not material: fabrication, installation, wood
/// substrate, consumables, cut-outs and transport.
///
/// The rate card is held once here and applied to every quote. The spreadsheet's
/// core weakness was a rate card duplicated across twelve sheets that then drifted
/// apart, so a rate exists in exactly one place.
/// </summary>
public class RateItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public RateCategory Category { get; set; }
    public ChargeUnit Unit { get; set; }

    /// <summary>
    /// Whether the estimator enters the quantity or it follows from the job.
    /// Consumables follow total area; transport follows sheet count.
    /// </summary>
    public DerivationRule Derivation { get; set; } = DerivationRule.Entered;

    /// <summary>
    /// Multiplies the derived quantity. Silicon and sealing is sheets multiplied by
    /// two, so it carries a factor of 2 against <see cref="DerivationRule.FromSheetCount"/>.
    /// Consumables and transport take the figure as it stands and carry 1.
    ///
    /// The sheets-times-two rule is a team assumption and has not been confirmed by
    /// the client.
    /// </summary>
    public decimal DerivationFactor { get; set; } = 1m;

    /// <summary>
    /// Multiplier applied to a related rate rather than a separately maintained
    /// figure. Overtime is normal fabrication multiplied by 1.5 and installation
    /// mirrors fabrication, so those relationships are expressed as rules here and
    /// are not duplicated numbers.
    /// </summary>
    public decimal? DerivedFromRateItemMultiplier { get; set; }

    public int? DerivedFromRateItemId { get; set; }
    public RateItem? DerivedFromRateItem { get; set; }

    /// <summary>
    /// Items below the line are cost recovery and are not marked up. Confirmed by
    /// the client: markup applies to the sub-total only.
    /// </summary>
    public bool IsBelowTheLine { get; set; }

    public CatalogueStatus Status { get; set; } = CatalogueStatus.Active;

    public ICollection<RatePrice> Prices { get; set; } = new List<RatePrice>();
}
