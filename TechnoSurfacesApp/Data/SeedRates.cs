using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// The central rate card. In the spreadsheet this is duplicated on all twelve
/// material sheets; holding it once removes that entire class of error.
///
/// IMPORTANT: the rand values here are INDICATIVE PLACEHOLDERS. The client
/// confirmed every rate in the sample workbook is fake, and the real rates have
/// not yet been supplied. The line items, units and derivation rules ARE real -
/// they come from the structure of the client's own costing sheet.
/// </summary>
public static class SeedRates
{
    private static int _id = 1;

    private static void R(RateGroup group, string name, string unit, decimal rate,
        bool derived = false, string? note = null, bool markedUp = true, string? rule = null)
    {
        Db.Rates.Add(new RateItem
        {
            Id = _id++,
            Group = group,
            Name = name,
            Unit = unit,
            Rate = rate,
            IsDerived = derived,
            DerivationNote = note,
            IsMarkedUp = markedUp,
            RateRule = rule
        });
    }

    public static void Load()
    {
        // ---- Fabrication ----
        R(RateGroup.Fabrication, "Fabrication \u2014 no backsplash, normal", "per hour", 385m);
        R(RateGroup.Fabrication, "Fabrication \u2014 with backsplash, normal", "per hour", 445m);
        R(RateGroup.Fabrication, "Fabrication \u2014 no backsplash, overtime", "per hour", 577.50m,
            rule: "Normal rate \u00D7 1,5");
        R(RateGroup.Fabrication, "Fabrication \u2014 with backsplash, overtime", "per hour", 667.50m,
            rule: "Normal rate \u00D7 1,5");
        R(RateGroup.Fabrication, "Thermoforming", "each", 950m);
        R(RateGroup.Fabrication, "Vacuum press", "each", 720m);
        R(RateGroup.Fabrication, "Sanding time", "per hour", 320m);

        // ---- Consumables ----
        R(RateGroup.Consumables, "Seamkit", "each", 130m,
            note: "Rate follows the supplier of the material quoted \u2014 R130 / R250 / R299");
        R(RateGroup.Consumables, "Sandpaper & consumables", "per m\u00B2", 55m,
            derived: true, note: "Quantity = total m\u00B2 across all material lines");
        R(RateGroup.Consumables, "Silicon + sealing", "each", 95m,
            note: "Costing sheet labels this 2 per sheet \u2014 overridable");
        R(RateGroup.Consumables, "Genkem", "each", 210m);

        // ---- Installation ----
        R(RateGroup.Installation, "Installation \u2014 normal", "per hour", 385m,
            rule: "Mirrors fabrication no-backsplash normal rate");
        R(RateGroup.Installation, "Installation \u2014 overtime", "per hour", 577.50m,
            rule: "Mirrors fabrication overtime rate");

        // ---- Wood & substrate ----
        R(RateGroup.WoodSubstrate, "MDF Bison 16mm white face", "per sheet", 685m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "MDF Bison 16mm", "per sheet", 610m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "MDF Bison 12mm", "per sheet", 505m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "MDF Bison 9mm", "per sheet", 430m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "Chipboard 16mm", "per sheet", 395m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "Chipboard 32mm Bison", "per sheet", 760m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "MFC White Std", "per sheet", 545m, note: "2,75 \u00D7 1,83");
        R(RateGroup.WoodSubstrate, "Hardboard std 3.2", "per sheet", 185m, note: "2,44 \u00D7 1,22");
        R(RateGroup.WoodSubstrate, "Plywood Pine 18mm", "per sheet", 890m, note: "2,44 \u00D7 1,22");
        R(RateGroup.WoodSubstrate, "5mm plywood bend", "per sheet", 340m, note: "2,44 \u00D7 1,22");
        R(RateGroup.WoodSubstrate, "Marine Ply 9mm", "per sheet", 720m, note: "2,44 \u00D7 1,22");
        R(RateGroup.WoodSubstrate, "Marine Ply 18mm", "per sheet", 1180m, note: "2,44 \u00D7 1,22");

        // ---- Sinks & hardware ----
        R(RateGroup.SinksHardware, "Sink / vanity", "each", 1300m);
        R(RateGroup.SinksHardware, "Brackets", "each", 165m);
        R(RateGroup.SinksHardware, "Hardware", "each", 240m);

        // ---- Below the line - cost recovery, NOT marked up ----
        R(RateGroup.BelowTheLine, "Drainer grooves", "each", 480m, markedUp: false);
        R(RateGroup.BelowTheLine, "Sink / vanity cut out", "each", 550m, markedUp: false);
        R(RateGroup.BelowTheLine, "Underslung sink / vanity", "each", 780m, markedUp: false);
        R(RateGroup.BelowTheLine, "Hob cut out", "each", 550m, markedUp: false);
        R(RateGroup.BelowTheLine, "Transport", "per sheet", 145m, markedUp: false,
            derived: true, note: "Quantity = total sheets across all material lines");
    }
}