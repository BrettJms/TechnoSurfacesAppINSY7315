using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Models;

/// <summary>
/// One line of the central rate card. In the spreadsheet this card is duplicated
/// on all twelve material sheets; holding it once is what removes that whole class
/// of error.
/// </summary>
public class RateItem
{
    public int Id { get; set; }
    public RateGroup Group { get; set; }
    public string Name { get; set; } = "";

    /// <summary>"per hour", "each", "per sheet", "per m2", "amount".</summary>
    public string Unit { get; set; } = "";

    public decimal Rate { get; set; }

    /// <summary>
    /// True where the quantity is computed, not typed - sandpaper is total m2,
    /// transport is the sheet count. These render read-only.
    /// </summary>
    public bool IsDerived { get; set; }

    /// <summary>Shown next to a derived row so the estimator can see the rule.</summary>
    public string? DerivationNote { get; set; }

    /// <summary>
    /// False for petrol, drainer grooves, transport and cut-outs. The client
    /// confirmed these sit below the markup as cost recovery with padding already
    /// built into the rate.
    /// </summary>
    public bool IsMarkedUp { get; set; } = true;

    /// <summary>Where one rate is defined as a multiple of another, e.g. overtime = normal x 1.5.</summary>
    public string? RateRule { get; set; }

    public string GroupLabel => Group switch
    {
        RateGroup.Fabrication => "Fabrication",
        RateGroup.Consumables => "Consumables",
        RateGroup.Installation => "Installation",
        RateGroup.WoodSubstrate => "Wood & substrate",
        RateGroup.SinksHardware => "Sinks & hardware",
        _ => "Below the line \u2014 not marked up"
    };
}

/// <summary>Shared label helpers for anything carrying a RateGroup.</summary>
public static class RateGroupExtensions
{
    public static string Label(this RateGroup g) => g switch
    {
        RateGroup.Fabrication => "Fabrication",
        RateGroup.Consumables => "Consumables",
        RateGroup.Installation => "Installation",
        RateGroup.WoodSubstrate => "Wood & substrate",
        RateGroup.SinksHardware => "Sinks & hardware",
        _ => "Below the line"
    };

    public static string GroupLabelShort(this QuoteCostingLine line) => line.Group.Label();
}