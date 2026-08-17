using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// Real catalogue data extracted from the client's five supplier price lists.
/// Prices are ex-VAT, as every supplier quotes them.
/// </summary>
public static class SeedCatalogue
{
    // Supplier ids
    private const int SurfaceStudio = 1, MaxOnTop = 2, Woodcentre = 3, Perago = 4, Staron = 5;

    // Product line ids
    private const int InfAcrylic = 11, InfModified = 12;
    private const int MotPure = 21, MotModified = 22, MotGetacore = 23;
    private const int Surwell = 31, SchemaR = 32;
    private const int PeragoClassic = 41, Magicstone = 42;
    private const int Staron12 = 51, Staron6 = 52;

    private static readonly DateTime SsDate = new(2026, 3, 1);
    private static readonly DateTime MotDate = new(2026, 8, 3);
    private static readonly DateTime WcDate = new(2026, 7, 1);
    private static readonly DateTime PgDate = new(2023, 6, 1);
    private static readonly DateTime StDate = new(2025, 3, 1);

    private static int _id = 1;

    public static void Load()
    {
        LoadSuppliers();
        LoadProductLines();
        LoadBands();
        LoadSurfaceStudio();
        LoadMaxOnTop();
        LoadWoodcentre();
        LoadPerago();
        LoadStaron();
    }

    private static void LoadSuppliers()
    {
        Db.Suppliers.AddRange(new[]
        {
            new Supplier
            {
                Id = SurfaceStudio, Name = "Surface Studio", TradingAs = "Infinito",
                PricingStructure = PricingStructure.Band, PriceListDated = SsDate,
                AdhesivePrice = 130m,
                DeliveryTerms = "R550 for orders under 10 sheets; outside Gauteng quoted per order",
                Notes = "Ex-VAT, EXW Johannesburg. 50ml applicator gun R600."
            },
            new Supplier
            {
                Id = MaxOnTop, Name = "Max on Top",
                PricingStructure = PricingStructure.Item, PriceListDated = MotDate,
                AdhesivePrice = 130m,
                DeliveryTerms = "R1 050 ex VAT on orders under 5 sheets",
                Notes = "Adhesive used on another supplier's material is charged at R299. Items marked phasing out are being discontinued."
            },
            new Supplier
            {
                Id = Woodcentre, Name = "Woodcentre", TradingAs = "SchemaR / Surwell",
                PricingStructure = PricingStructure.Item, PriceListDated = WcDate,
                AdhesivePrice = 250m,
                DeliveryTerms = "Quoted per order",
                Notes = "Publishes live stock, quoted in half sheets. If stock is depleted and they buy out, price will differ."
            },
            new Supplier
            {
                Id = Perago, Name = "Perago / Magicstone", TradingAs = "via Salvocorp",
                PricingStructure = PricingStructure.Item, PriceListDated = PgDate,
                AdhesivePrice = 130m,
                DeliveryTerms = "JHB metro R510 \u00B7 Pretoria R650 \u00B7 Western Cape R510 within 40km, R950 beyond",
                Notes = "Oldest price list held. Confirm currency with the supplier before quoting."
            },
            new Supplier
            {
                Id = Staron, Name = "Staron", TradingAs = "via Salvocorp",
                PricingStructure = PricingStructure.Band, PriceListDated = StDate,
                AdhesivePrice = 130m,
                DeliveryTerms = "Quoted per order",
                Notes = "Priced by colour category. Individual Staron colour names map into these categories."
            }
        });
    }

    private static void LoadProductLines()
    {
        Db.ProductLines.AddRange(new[]
        {
            new ProductLine { Id = InfAcrylic,   SupplierId = SurfaceStudio, Name = "Infinito Full Acrylic", Description = "Groups A1\u2013A4" },
            new ProductLine { Id = InfModified,  SupplierId = SurfaceStudio, Name = "Infinito Modified",     Description = "Groups M1\u2013M4" },
            new ProductLine { Id = MotPure,      SupplierId = MaxOnTop,      Name = "Max Pure Solid Surface" },
            new ProductLine { Id = MotModified,  SupplierId = MaxOnTop,      Name = "Max Modified Acrylic" },
            new ProductLine { Id = MotGetacore,  SupplierId = MaxOnTop,      Name = "Max GetaCore 3mm" },
            new ProductLine { Id = Surwell,      SupplierId = Woodcentre,    Name = "Surwell Modified Acrylic" },
            new ProductLine { Id = SchemaR,      SupplierId = Woodcentre,    Name = "SchemaR Full Acrylic" },
            new ProductLine { Id = PeragoClassic,SupplierId = Perago,        Name = "Perago 100% Acrylic" },
            new ProductLine { Id = Magicstone,   SupplierId = Perago,        Name = "Magicstone Modified" },
            new ProductLine { Id = Staron12,     SupplierId = Staron,        Name = "Staron 12mm" },
            new ProductLine { Id = Staron6,      SupplierId = Staron,        Name = "Staron 6mm" }
        });
    }

    private static void LoadBands()
    {
        Db.PriceBands.AddRange(new[]
        {
            new PriceBand { Id = 1,  SupplierId = SurfaceStudio, Code = "A1", Name = "Full Acrylic Group A1", PricePerSqm = 1200m },
            new PriceBand { Id = 2,  SupplierId = SurfaceStudio, Code = "A2", Name = "Full Acrylic Group A2", PricePerSqm = 1540m },
            new PriceBand { Id = 3,  SupplierId = SurfaceStudio, Code = "A3", Name = "Full Acrylic Group A3", PricePerSqm = 1800m },
            new PriceBand { Id = 4,  SupplierId = SurfaceStudio, Code = "A4", Name = "Full Acrylic Group A4", PricePerSqm = 2255m },
            new PriceBand { Id = 5,  SupplierId = SurfaceStudio, Code = "M1", Name = "Modified Group M1",     PricePerSqm = 1280m },
            new PriceBand { Id = 6,  SupplierId = SurfaceStudio, Code = "M2", Name = "Modified Group M2",     PricePerSqm = 1530m },
            new PriceBand { Id = 7,  SupplierId = SurfaceStudio, Code = "M3", Name = "Modified Group M3",     PricePerSqm = 1830m },
            new PriceBand { Id = 8,  SupplierId = SurfaceStudio, Code = "M4", Name = "Modified Group M4",     PricePerSqm = 1980m },

            new PriceBand { Id = 20, SupplierId = Staron, Code = "BRIGHTWHITE", Name = "Bright White",  PricePerSqm = 1550m },
            new PriceBand { Id = 21, SupplierId = Staron, Code = "SOLID",       Name = "Solid",         PricePerSqm = 1683m },
            new PriceBand { Id = 22, SupplierId = Staron, Code = "SANDED",      Name = "Sanded",        PricePerSqm = 1800m },
            new PriceBand { Id = 23, SupplierId = Staron, Code = "ASPEN",       Name = "Aspen",         PricePerSqm = 1880m },
            new PriceBand { Id = 24, SupplierId = Staron, Code = "PEBBLE",      Name = "Pebble",        PricePerSqm = 1975m },
            new PriceBand { Id = 25, SupplierId = Staron, Code = "METALLIC",    Name = "Metallic",      PricePerSqm = 2150m },
            new PriceBand { Id = 26, SupplierId = Staron, Code = "TERRAZZO",    Name = "Terrazzo",      PricePerSqm = 2353m },
            new PriceBand { Id = 27, SupplierId = Staron, Code = "QUARRY",      Name = "Quarry",        PricePerSqm = 2353m },
            new PriceBand { Id = 28, SupplierId = Staron, Code = "TEMPEST",     Name = "Tempest",       PricePerSqm = 2663m },
            new PriceBand { Id = 29, SupplierId = Staron, Code = "SUPREME",     Name = "Supreme",       PricePerSqm = 2760m },
            new PriceBand { Id = 30, SupplierId = Staron, Code = "PREMIER",     Name = "Premier range", PricePerSqm = 2850m }
        });
    }

    /// <summary>Row helper - keeps every catalogue line to a single readable statement.</summary>
    private static void E(int sup, int pl, string colour, string code,
        int len, int wid, int thk,
        decimal? perSqm = null, decimal? perSheet = null,
        string? band = null, string? range = null, decimal? stock = null,
        CatalogueStatus status = CatalogueStatus.Active, DateTime? eff = null)
    {
        Db.Catalogue.Add(new CatalogueEntry
        {
            Id = _id++,
            SupplierId = sup,
            ProductLineId = pl,
            ColourName = colour,
            SupplierCode = code,
            SheetLengthMm = len,
            SheetWidthMm = wid,
            ThicknessMm = thk,
            PricePerSqm = perSqm,
            PublishedPricePerSheet = perSheet,
            BandCode = band,
            Range = range,
            StockQty = stock,
            Status = status,
            EffectiveFrom = eff ?? DateTime.Today
        });
    }

    // ---- Surface Studio (Infinito) - priced by band ----
    // Group A1 is the 6mm sheet: R2 781,60 / R1 200 = 2,318 m2 = 3050 x 760.
    private static void LoadSurfaceStudio()
    {
        E(SurfaceStudio, InfAcrylic, "WHITE", "SS-A-INF-003", 3050, 760, 6, perSqm: 1200m, band: "A1", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "WHITE", "SS-A-INF-003", 3680, 760, 12, perSqm: 1540m, band: "A2", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "ASPEN CORAL", "SS-A-INF-368", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "BLACK NIGHT", "SS-A-INF-101", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "BOSTON", "SS-A-INF-3403", 3680, 760, 12, perSqm: 2255m, band: "A4", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "CLOUDY CALACATTA", "SS-A-INF-1016", 3680, 760, 12, perSqm: 2255m, band: "A4", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "CONCRETE GREY", "SS-A-INF-028", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "CREAM", "SS-A-INF-105", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "DOVE", "SS-A-INF-415", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "LATTE", "SS-A-INF-1009", 3680, 760, 12, perSqm: 2255m, band: "A4", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "LIMISTONE", "SS-A-INF-331", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "MILK WHITE", "SS-A-INF-309", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "NUTELLA", "SS-A-INF-1021", 3680, 760, 12, perSqm: 2255m, band: "A4", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "PURE BLACK", "SS-A-INF-002", 3680, 760, 12, perSqm: 1800m, band: "A3", eff: SsDate);
        E(SurfaceStudio, InfAcrylic, "VANILLA", "SS-A-INF-0005", 3680, 760, 12, perSqm: 2255m, band: "A4", eff: SsDate);

        E(SurfaceStudio, InfModified, "WHITE", "SS-M-INF-003", 3680, 760, 12, perSqm: 1280m, band: "M1", eff: SsDate);
        E(SurfaceStudio, InfModified, "ASPEN CORAL", "SS-M-INF-368", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "ASPEN SAND", "SS-M-INF-178", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "ASTERIX", "SS-M-INF-005", 3680, 760, 12, perSqm: 1830m, band: "M3", eff: SsDate);
        E(SurfaceStudio, InfModified, "ATLANTA", "SS-M-INF-3402", 3680, 760, 12, perSqm: 1830m, band: "M3", eff: SsDate);
        E(SurfaceStudio, InfModified, "BLACK CALACATTA", "SS-M-INF-3516", 3680, 760, 12, perSqm: 1980m, band: "M4", eff: SsDate);
        E(SurfaceStudio, InfModified, "BLACK NIGHT", "SS-M-INF-101", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "CHICAGO", "SS-M-INF-3401", 3680, 760, 12, perSqm: 1830m, band: "M3", eff: SsDate);
        E(SurfaceStudio, InfModified, "CONCRETE GREY", "SS-M-INF-028", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "CREAM", "SS-M-INF-105", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "CRYSTAL BEIGE", "SS-M-INF-323", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "DAYLIGHT", "SS-M-INF-1295", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "DREAMY WHITE", "SS-M-INF-2502", 3680, 760, 12, perSqm: 1830m, band: "M3", eff: SsDate);
        E(SurfaceStudio, InfModified, "FROST", "SS-M-INF-307", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "IVORY", "SS-M-INF-002", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "LIGHT CALACATTA", "SS-M-INF-3509", 3680, 760, 12, perSqm: 1980m, band: "M4", eff: SsDate);
        E(SurfaceStudio, InfModified, "LIGHT GREY", "SS-M-INF-S006", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "LIMISTONE", "SS-M-INF-331", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "MILK WHITE", "SS-M-INF-309", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "MINT", "SS-M-INF-1296", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "NIGHT GLEAM", "SS-M-INF-007", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "RED", "SS-M-INF-1265", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "RIMY CONCRETE", "SS-M-INF-C002", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
        E(SurfaceStudio, InfModified, "WHITE QUARTZ", "SS-M-INF-535", 3680, 760, 12, perSqm: 1530m, band: "M2", eff: SsDate);
    }

    // ---- Max on Top - priced per item. Note the same colour at three prices. ----
    private static void LoadMaxOnTop()
    {
        E(MaxOnTop, MotPure, "Alaskan Stone 4312", "GAVONITE\u00AE4312472", 3658, 760, 12, perSheet: 8293m, eff: MotDate);
        E(MaxOnTop, MotPure, "Alpine Shimmer 8206", "WSOLIDSURFACE8206", 3680, 760, 12, perSheet: 8343m, eff: MotDate);
        E(MaxOnTop, MotPure, "Arctica 9015", "WSOLIDSURFACE9015", 3680, 760, 12, perSheet: 4671m, eff: MotDate);
        E(MaxOnTop, MotPure, "Arctica 9015", "WSOLIDSURFACE90156", 2440, 1220, 6, perSheet: 3688m, eff: MotDate);
        E(MaxOnTop, MotPure, "Aspen 5230", "WSOLIDSURFACE5230", 3680, 760, 12, perSheet: 4671m, eff: MotDate);
        E(MaxOnTop, MotPure, "Aspen 5230", "WWSOLIDSURF5230920", 3680, 920, 12, perSheet: 5654m, eff: MotDate);
        E(MaxOnTop, MotPure, "Avalanche 7502", "WSOLIDSURFACE7502", 3680, 760, 12, perSheet: 5803m, eff: MotDate);
        E(MaxOnTop, MotPure, "Bone 8010", "WSOLIDSURFACE8010", 3680, 760, 12, perSheet: 4671m, eff: MotDate);
        E(MaxOnTop, MotPure, "Bronze 7830", "GAVONITE\u00AE7830472", 3658, 760, 12, perSheet: 8293m, eff: MotDate);
        E(MaxOnTop, MotPure, "Cameo White 8106", "GAVONITE\u00AE8106472", 3680, 760, 12, perSheet: 4671m, eff: MotDate);
        E(MaxOnTop, MotPure, "Casablanca 9137", "WSOLIDSURFACE9137", 3680, 760, 12, perSheet: 4894m, eff: MotDate);
        E(MaxOnTop, MotPure, "Cloud 8292", "GAVONITE\u00AE8292472", 3680, 760, 12, perSheet: 4894m, eff: MotDate);
        E(MaxOnTop, MotPure, "Eclipse 8240", "WSOLIDSURFACE8240", 3680, 760, 12, perSheet: 5034m, eff: MotDate);
        E(MaxOnTop, MotPure, "Fuego 8248", "WSOLIDSURFACE8248", 3680, 760, 12, perSheet: 4671m, eff: MotDate);
        E(MaxOnTop, MotPure, "Glacier White 8016", "WSOLIDSURFACE80167", 3680, 760, 12, perSheet: 3991m, eff: MotDate);
        E(MaxOnTop, MotPure, "Glacier White 8016", "WSOLIDSURFACE8016", 3680, 920, 12, perSheet: 4831m, eff: MotDate);
        E(MaxOnTop, MotPure, "Glacier White 8016", "WSOLIDSURFACE80166", 2440, 920, 6, perSheet: 2287m, eff: MotDate);
        E(MaxOnTop, MotPure, "Industrial 7849", "WSOLIDSURFACE7849", 3680, 760, 12, perSheet: 5803m, eff: MotDate);
        E(MaxOnTop, MotPure, "Jurassic 7711", "GAVONITE\u00AE7711472", 3658, 760, 12, perSheet: 8293m, eff: MotDate);
        E(MaxOnTop, MotPure, "Kokoura 9117", "WSOLIDSURFACE9117", 3680, 760, 12, perSheet: 5803m, eff: MotDate);
        E(MaxOnTop, MotPure, "Malt 7810", "GAVONITE\u00AE7810472", 3680, 760, 12, perSheet: 6670m, eff: MotDate);
        E(MaxOnTop, MotPure, "Mango 8268", "WSOLIDSURFACE8268", 3680, 760, 12, perSheet: 5034m, eff: MotDate);
        E(MaxOnTop, MotPure, "New Concrete 7842", "WSOLIDSURFACE7842", 3680, 760, 12, perSheet: 5803m, eff: MotDate);
        E(MaxOnTop, MotPure, "Snowfall 8090", "GAVONITE\u00AE8090472", 3680, 760, 12, perSheet: 8343m, eff: MotDate);
        E(MaxOnTop, MotPure, "Starshine 7820", "WSOLIDSURFACE7820", 3680, 760, 12, perSheet: 5803m, eff: MotDate);

        E(MaxOnTop, MotModified, "Simply Altitude 2501", "WSOLIDSURFACE2501", 3680, 760, 12, perSheet: 5454m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Arctica 2028", "WSOLIDSURFACE2028", 3680, 760, 12, perSheet: 4055m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Dusk LM106", "WSOLIDSURFACELM106", 3680, 760, 12, perSheet: 4335m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Grey Terrazzo 8968", "GAVONITE\u00AERMS8968", 3680, 760, 12, perSheet: 6142m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Light Cement 8961", "WSOLIDSURFACE8961", 3680, 760, 12, perSheet: 4335m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Marbled Grey 8969", "WSOLIDSURFACE8969", 3680, 760, 12, perSheet: 7109m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Marble Whisp 3522", "WSOLIDSURFACE3522", 3680, 760, 12, perSheet: 7109m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Morning Mist 8904", "WSOLIDSURFACE8904", 3680, 760, 12, perSheet: 5454m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Speckled Cr\u00E8me 8967", "WSOLIDSURFACE8967", 3680, 760, 12, perSheet: 4055m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply Summit 8905", "WSOLIDSURFACE8905", 3680, 920, 12, perSheet: 8606m, eff: MotDate);
        E(MaxOnTop, MotModified, "Simply White 8960", "WSOLIDSURFACE8960", 3680, 760, 12, perSheet: 2900m, eff: MotDate);

        // Status below is illustrative - the source list marks phased-out items in red,
        // which the text extract did not preserve. Set properly from the admin screen.
        E(MaxOnTop, MotGetacore, "GetaCore Snowdrift GC2252", "FG25207W2630W42", 2040, 1250, 3, perSheet: 4919m, eff: MotDate);
        E(MaxOnTop, MotGetacore, "GetaCore Dusk GC4143", "FGC4107W2630W42", 2040, 1250, 3, perSheet: 4919m, eff: MotDate, status: CatalogueStatus.PhasingOut);
        E(MaxOnTop, MotGetacore, "GetaCore Glacier White GC2011", "FG01107W2630W42", 2040, 1250, 3, perSheet: 3902m, eff: MotDate);
        E(MaxOnTop, MotGetacore, "GetaCore Terrazzo Pebble CGT244", "FT24407W2630W42", 2040, 1250, 3, perSheet: 5811m, eff: MotDate, status: CatalogueStatus.PhasingOut);
    }

    // ---- Woodcentre - per item, with live stock. Note Soft White at 63.5 sheets. ----
    private static void LoadWoodcentre()
    {
        E(Woodcentre, Surwell, "SKY WHITE", "3202217", 3680, 760, 12, perSheet: 2950m, stock: 74m, eff: WcDate);
        E(Woodcentre, Surwell, "ALMOND", "3202212", 3680, 760, 12, perSheet: 3500m, range: "SOLID", stock: 10m, eff: WcDate);
        E(Woodcentre, Surwell, "BLACK BALL", "3202203", 3680, 760, 12, perSheet: 3500m, range: "SOLID", stock: 10m, eff: WcDate);
        E(Woodcentre, Surwell, "SLATE", "3202211", 3680, 760, 12, perSheet: 3500m, range: "SOLID", stock: 38m, eff: WcDate);
        E(Woodcentre, Surwell, "STEEL", "3202210", 3680, 760, 12, perSheet: 3500m, range: "SOLID", stock: 8m, eff: WcDate);
        E(Woodcentre, Surwell, "REES RED", "3202202", 3680, 760, 12, perSheet: 3500m, range: "SOLID", stock: 41m, eff: WcDate);
        E(Woodcentre, Surwell, "NEBULA BALL", "3202204", 3680, 760, 12, perSheet: 3800m, range: "NEBULA", stock: 10m, eff: WcDate);
        E(Woodcentre, Surwell, "NEBULA MUDDY", "3202205", 3680, 760, 12, perSheet: 3800m, range: "NEBULA", stock: 21m, eff: WcDate);
        E(Woodcentre, Surwell, "POPLAR ROCKY", "3202213", 3680, 760, 12, perSheet: 4000m, range: "POPLAR", stock: 12m, eff: WcDate);
        E(Woodcentre, Surwell, "STELLA BIRCH", "3202206", 3680, 760, 12, perSheet: 4500m, range: "STELLA", stock: 20m, eff: WcDate);
        E(Woodcentre, Surwell, "STELLA DOVE", "3202207", 3680, 760, 12, perSheet: 4500m, range: "STELLA", stock: 10m, eff: WcDate);
        E(Woodcentre, Surwell, "STELLA FAME", "3202208", 3680, 760, 12, perSheet: 4500m, range: "STELLA", stock: 26m, eff: WcDate);
        E(Woodcentre, Surwell, "MET BLACK", "3202209", 3680, 760, 12, perSheet: 5500m, range: "MET", stock: 98m, eff: WcDate);
        E(Woodcentre, Surwell, "MET GREY", "3202214", 3680, 760, 12, perSheet: 5500m, range: "MET", stock: 14m, eff: WcDate);

        E(Woodcentre, SchemaR, "Black Pitch", "3202032", 3680, 760, 12, perSheet: 5600m, stock: 5m, eff: WcDate);
        E(Woodcentre, SchemaR, "Galaxy Ball", "3202020", 3680, 760, 12, perSheet: 5800m, stock: 14m, eff: WcDate);
        E(Woodcentre, SchemaR, "Galaxy Muddy", "3202023", 3680, 760, 12, perSheet: 4850m, stock: 17m, eff: WcDate);
        E(Woodcentre, SchemaR, "Iron", "3202039", 3680, 760, 12, perSheet: 4850m, stock: 31m, eff: WcDate);
        E(Woodcentre, SchemaR, "Millet Fantasia", "3202037", 3680, 760, 12, perSheet: 5000m, stock: 18m, eff: WcDate);
        E(Woodcentre, SchemaR, "Millet Rocky", "3202034", 3680, 760, 12, perSheet: 5100m, stock: 25m, eff: WcDate);
        E(Woodcentre, SchemaR, "Millet Smokestone", "3202036", 3680, 760, 12, perSheet: 3800m, stock: 15m, eff: WcDate);
        E(Woodcentre, SchemaR, "Moire Shade Concrete", "3202053", 3680, 760, 12, perSheet: 8650m, stock: 6m, eff: WcDate);
        E(Woodcentre, SchemaR, "Moire White Onyx", "3202052", 3680, 760, 12, perSheet: 8650m, stock: 8m, eff: WcDate);
        E(Woodcentre, SchemaR, "Pearl Grey", "3202047", 3680, 760, 12, perSheet: 7600m, stock: 15m, eff: WcDate);
        E(Woodcentre, SchemaR, "Shingle Fame", "3202027", 3680, 760, 12, perSheet: 5400m, stock: 15m, eff: WcDate);
        E(Woodcentre, SchemaR, "Shingle Birch", "3202035", 3680, 760, 12, perSheet: 5400m, stock: 5m, eff: WcDate);
        E(Woodcentre, SchemaR, "Soft White", "3202031", 3680, 760, 12, perSheet: 3900m, stock: 63.5m, eff: WcDate);
        E(Woodcentre, SchemaR, "Zebra Stone", "3202021", 3680, 760, 12, perSheet: 5250m, stock: 6m, eff: WcDate);
    }

    // ---- Perago / Magicstone - 3660mm sheets, not 3680. The 0,5% that matters. ----
    private static void LoadPerago()
    {
        E(Perago, PeragoClassic, "Perago Classic White", "", 3660, 760, 12, perSqm: 1520m, eff: PgDate);
        E(Perago, PeragoClassic, "Perago Classic White", "", 3660, 900, 12, perSqm: 2140m, eff: PgDate);
        E(Perago, PeragoClassic, "Perago Classic White", "", 3660, 760, 6, perSqm: 1295m, eff: PgDate);

        E(Perago, Magicstone, "Magicstone Snow White", "", 3660, 760, 12, perSqm: 1450m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Moss", "", 3660, 760, 12, perSqm: 1595m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Sand Stone", "", 3660, 760, 12, perSqm: 1595m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone White Jasper", "", 3660, 760, 12, perSqm: 1595m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Bamboo", "", 3660, 760, 12, perSqm: 1595m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Stardust", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Moonstone", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Opaline", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Amazon", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Windswept", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Thunder", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Gypsum", "", 3660, 760, 12, perSqm: 2085m, eff: PgDate);
        E(Perago, Magicstone, "Magicstone Grey Pearl", "", 3660, 760, 12, perSqm: 2100m, eff: PgDate);
    }

    // ---- Staron - the "colour" IS the price category on their list. ----
    private static void LoadStaron()
    {
        E(Staron, Staron12, "Bright White", "", 3680, 760, 12, perSqm: 1550m, band: "BRIGHTWHITE", eff: StDate);
        E(Staron, Staron12, "Solid", "", 3680, 760, 12, perSqm: 1683m, band: "SOLID", eff: StDate);
        E(Staron, Staron12, "Sanded", "", 3680, 760, 12, perSqm: 1800m, band: "SANDED", eff: StDate);
        E(Staron, Staron12, "Aspen", "", 3680, 760, 12, perSqm: 1880m, band: "ASPEN", eff: StDate);
        E(Staron, Staron12, "Pebble", "", 3680, 760, 12, perSqm: 1975m, band: "PEBBLE", eff: StDate);
        E(Staron, Staron12, "Metallic", "", 3680, 760, 12, perSqm: 2150m, band: "METALLIC", eff: StDate);
        E(Staron, Staron12, "Terrazzo", "", 3680, 760, 12, perSqm: 2353m, band: "TERRAZZO", eff: StDate);
        E(Staron, Staron12, "Quarry", "", 3680, 760, 12, perSqm: 2353m, band: "QUARRY", eff: StDate);
        E(Staron, Staron12, "Tempest", "", 3680, 760, 12, perSqm: 2663m, band: "TEMPEST", eff: StDate);
        E(Staron, Staron12, "Supreme", "", 3680, 760, 12, perSqm: 2760m, band: "SUPREME", eff: StDate);
        E(Staron, Staron12, "Premier range", "", 3680, 760, 12, perSqm: 2850m, band: "PREMIER", eff: StDate);

        E(Staron, Staron6, "Bright White", "", 2500, 760, 6, perSqm: 1485m, band: "BRIGHTWHITE", eff: StDate);
    }
}