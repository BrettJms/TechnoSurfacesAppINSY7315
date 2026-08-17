using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfaces.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// Demo quotes spanning the whole lifecycle. Material prices are real; the labour
/// and consumable rates behind the costing lines are indicative placeholders.
/// </summary>
public static class SeedQuotes
{
    private const int Paul = 1, Lerato = 2, Devan = 3;
    private static int _lineId = 1;

    public static void Load()
    {
        // 1 - Draft, estimator's work in progress
        var q1 = New(1, "TS-2026-0141", customer: 1, contact: 1, owner: Lerato,
            site: "Bootleggers Claremont, Cavendish Connect",
            project: "Service counter and two bar tops",
            issued: Days(-1), status: QuoteStatus.Draft, markup: 35m, petrol: 850m);
        Mat(q1, "Infinito Modified", "CONCRETE GREY", 12, 760, sheets: 4m, disc: 5m);
        Cost(q1, "Fabrication \u2014 with backsplash, normal", 14m);
        Cost(q1, "Sanding time", 5m);
        Cost(q1, "Seamkit", 6m);
        Cost(q1, "Silicon + sealing", 8m);
        Cost(q1, "MDF Bison 16mm", 3m);
        Cost(q1, "Installation \u2014 normal", 8m);
        Cost(q1, "Sink / vanity cut out", 1m);
        Finalise(q1);

        // 2 - Pending approval. RA Woodcraft, two materials on one job -
        // exactly what invoice IN114317 shows is possible.
        var q2 = New(2, "TS-2026-0140", customer: 3, contact: 6, owner: Devan,
            site: "Sweet Valley Farm, Tokai",
            project: "Guest room vanities and splash backs",
            issued: Days(-3), status: QuoteStatus.PendingApproval, markup: 38m, petrol: 1050m,
            customerRef: "SWEET VALLEY FARM",
            delivery: "Sweet Valley Farm, Tokai, 7945");
        Mat(q2, "Staron 12mm", "Sanded", 12, 760, sheets: 2m);
        Mat(q2, "Max Modified Acrylic", "Simply Summit 8905", 12, 920, sheets: 1m, disc: 7.5m);
        Cost(q2, "Fabrication \u2014 with backsplash, normal", 11m);
        Cost(q2, "Fabrication \u2014 no backsplash, normal", 4m);
        Cost(q2, "Sanding time", 3.5m);
        Cost(q2, "Seamkit", 4m);
        Cost(q2, "MDF Bison 16mm white face", 2m);
        Cost(q2, "Installation \u2014 normal", 6m);
        Cost(q2, "Underslung sink / vanity", 3m);
        Cost(q2, "Drainer grooves", 2m);
        Quo(q2, "Guest Room Vanities", "2 \u00D7 1 200 \u00D7 500 \u00D7 12mm with 100mm backsplash. Material: Staron, Colour: Sanded", 2m, 4310m);
        Quo(q2, "Shiloh's Bedroom Vanity", "1 200 \u00D7 500 \u00D7 100mm", 1m, 3180m);
        Quo(q2, "Splash back", "1 200 \u00D7 300mm. Material: Max on Top, Colour: Simply Summit", 1m, 2480m);
        Quo(q2, "", "Fabricate and install. Templates by Techno Surfaces.", null, null);
        Quo(q2, "", "Includes 16mm MDF support board.", null, null);
        Finalise(q2);

        // 3 - Pending approval
        var q3 = New(3, "TS-2026-0139", customer: 2, contact: 3, owner: Lerato,
            site: "Vida e Caff\u00E8, Canal Walk",
            project: "New store front counter and back bar",
            issued: Days(-5), status: QuoteStatus.PendingApproval, markup: 32m, petrol: 550m);
        Mat(q3, "Infinito Full Acrylic", "PURE BLACK", 12, 760, sheets: 5m, disc: 10m);
        Cost(q3, "Fabrication \u2014 with backsplash, normal", 18m);
        Cost(q3, "Fabrication \u2014 with backsplash, overtime", 4m);
        Cost(q3, "Thermoforming", 2m);
        Cost(q3, "Sanding time", 7m);
        Cost(q3, "Seamkit", 8m);
        Cost(q3, "Silicon + sealing", 10m);
        Cost(q3, "MDF Bison 16mm", 4m);
        Cost(q3, "Installation \u2014 normal", 12m);
        Cost(q3, "Hob cut out", 1m);
        Finalise(q3);

        // 4 - Approved by the MD (his own quote, self-approved)
        var q4 = New(4, "TS-2026-0138", customer: 4, contact: 7, owner: Paul,
            site: "Cape Town International Airport, Domestic Terminal",
            project: "Food court servery counters, phase 2",
            issued: Days(-8), status: QuoteStatus.Approved, markup: 30m, petrol: 1400m,
            customerRef: "ACSA-FC-P2");
        q4.ApprovedByUserId = Paul;
        q4.ApprovedAt = Days(-8).AddHours(2);
        Mat(q4, "Staron 12mm", "Solid", 12, 760, sheets: 12m, disc: 12.5m);
        Mat(q4, "Staron 12mm", "Metallic", 12, 760, sheets: 3m, disc: 12.5m);
        Cost(q4, "Fabrication \u2014 with backsplash, normal", 46m);
        Cost(q4, "Fabrication \u2014 with backsplash, overtime", 12m);
        Cost(q4, "Vacuum press", 4m);
        Cost(q4, "Sanding time", 16m);
        Cost(q4, "Seamkit", 22m);
        Cost(q4, "Silicon + sealing", 30m);
        Cost(q4, "MDF Bison 16mm white face", 11m);
        Cost(q4, "Marine Ply 18mm", 4m);
        Cost(q4, "Installation \u2014 normal", 24m);
        Cost(q4, "Installation \u2014 overtime", 8m);
        Cost(q4, "Sink / vanity", 4m);
        Cost(q4, "Brackets", 18m);
        Cost(q4, "Sink / vanity cut out", 4m);
        Cost(q4, "Drainer grooves", 6m);
        Finalise(q4);

        // 5 - Sent, waiting on the customer
        var q5 = New(5, "TS-2026-0137", customer: 6, contact: 11, owner: Devan,
            site: "18 Buitengracht Close, Pinelands",
            project: "Kitchen worktop and island",
            issued: Days(-11), status: QuoteStatus.Sent, markup: 40m, petrol: 480m);
        q5.ApprovedByUserId = Paul;
        q5.ApprovedAt = Days(-10);
        Mat(q5, "SchemaR Full Acrylic", "Soft White", 12, 760, sheets: 2.5m);
        Cost(q5, "Fabrication \u2014 with backsplash, normal", 9m);
        Cost(q5, "Sanding time", 3m);
        Cost(q5, "Seamkit", 3m);
        Cost(q5, "Silicon + sealing", 5m);
        Cost(q5, "MDF Bison 16mm", 2m);
        Cost(q5, "Installation \u2014 normal", 5m);
        Cost(q5, "Sink / vanity cut out", 1m);
        Cost(q5, "Hob cut out", 1m);
        Quo(q5, "Kitchen worktop", "3 400 \u00D7 620 \u00D7 12mm with 100mm backsplash. Material: SchemaR, Colour: Soft White", 1m, 15400m);
        Quo(q5, "Island top", "1 800 \u00D7 900 \u00D7 12mm, mitred 40mm edge", 1m, 9800m);
        Quo(q5, "", "Sink and hob cut-outs included. Sealing between countertop and walls excluded.", null, null);
        Finalise(q5);

        // 6 - Accepted in writing, not yet invoiced
        var q6 = New(6, "TS-2026-0136", customer: 1, contact: 2, owner: Lerato,
            site: "Bootleggers Sea Point, Regent Road",
            project: "Counter refurbishment",
            issued: Days(-19), status: QuoteStatus.Accepted, markup: 35m, petrol: 620m);
        q6.ApprovedByUserId = Paul;
        q6.ApprovedAt = Days(-18);
        Mat(q6, "Surwell Modified Acrylic", "MET BLACK", 12, 760, sheets: 3m, disc: 5m);
        Cost(q6, "Fabrication \u2014 no backsplash, normal", 12m);
        Cost(q6, "Sanding time", 4m);
        Cost(q6, "Seamkit", 5m);
        Cost(q6, "MDF Bison 16mm", 2m);
        Cost(q6, "Installation \u2014 normal", 7m);
        Finalise(q6);

        // 7 - Invoiced. Pastel remains the book of record - we only hold the reference.
        var q7 = New(7, "TS-2026-0135", customer: 3, contact: 6, owner: Paul,
            site: "Sweet Valley Farm, Tokai",
            project: "Guest room vanities \u2014 phase 1",
            issued: Days(-27), status: QuoteStatus.Invoiced, markup: 36m, petrol: 510m,
            customerRef: "SWEET VALLEY FARM");
        q7.ApprovedByUserId = Paul;
        q7.ApprovedAt = Days(-27);
        q7.PastelInvoiceNo = "IN114317";
        q7.PastelInvoiceDate = new DateTime(2026, 8, 13);
        q7.PastelInvoiceAmount = 6997.49m;
        Mat(q7, "Staron 12mm", "Sanded", 12, 760, sheets: 1m);
        Mat(q7, "Max Modified Acrylic", "Simply Summit 8905", 12, 920, sheets: 0.5m);
        Cost(q7, "Fabrication \u2014 with backsplash, normal", 6m);
        Cost(q7, "Sanding time", 2m);
        Cost(q7, "Seamkit", 2m);
        Cost(q7, "MDF Bison 16mm white face", 1m);
        Cost(q7, "Installation \u2014 normal", 4m);
        Cost(q7, "Underslung sink / vanity", 2m);
        Quo(q7, "Guest Room Vanities", "2 \u00D7 1 200 \u00D7 500 \u00D7 12mm with 100mm backsplash. Material: Staron, Colour: Sanded", 2m, 1251.53m);
        Quo(q7, "Shiloh's Bedroom Vanity", "1 200 \u00D7 500 \u00D7 100mm", 1m, 1601.72m);
        Quo(q7, "Splash back", "1 200 \u00D7 300mm. Material: Max on Top, Colour: Simply Summit", 1m, 1980m);
        Finalise(q7);

        // 8 - Reopened after a customer counter-offer, now on version 3.
        // Original versions are retained - the quote is not a write-once document.
        var q8 = New(8, "TS-2026-0134", customer: 2, contact: 4, owner: Devan,
            site: "Vida e Caff\u00E8, Tyger Valley",
            project: "Counter and condiment station",
            issued: Days(-2), status: QuoteStatus.Draft, markup: 28m, petrol: 780m);
        q8.VersionNumber = 3;
        Mat(q8, "Infinito Modified", "IVORY", 12, 760, sheets: 3m, disc: 8m);
        Cost(q8, "Fabrication \u2014 with backsplash, normal", 13m);
        Cost(q8, "Sanding time", 4m);
        Cost(q8, "Seamkit", 5m);
        Cost(q8, "Silicon + sealing", 6m);
        Cost(q8, "MDF Bison 12mm", 3m);
        Cost(q8, "Installation \u2014 normal", 7m);
        Finalise(q8);

        q8.Versions.AddRange(new[]
        {
            new QuoteVersion { VersionNumber = 1, CreatedAt = Days(-16), CreatedBy = "Devan Naidoo",
                Note = "Original quotation issued to customer", Total = 41280.50m, StatusAtSnapshot = QuoteStatus.Sent },
            new QuoteVersion { VersionNumber = 2, CreatedAt = Days(-7), CreatedBy = "Devan Naidoo",
                Note = "Counter-offer received \u2014 markup reduced from 35% to 30%, sheet count trimmed", Total = 37940.15m, StatusAtSnapshot = QuoteStatus.Sent },
            new QuoteVersion { VersionNumber = 3, CreatedAt = Days(-2), CreatedBy = "Devan Naidoo",
                Note = "Second counter-offer \u2014 markup 28%, condiment station reduced to one sheet", Total = q8.Total, StatusAtSnapshot = QuoteStatus.Draft }
        });

        // Every other quote carries at least its issued version.
        foreach (var q in Db.Quotes.Where(x => x.Versions.Count == 0))
        {
            q.Versions.Add(new QuoteVersion
            {
                VersionNumber = 1,
                CreatedAt = q.IssueDate,
                CreatedBy = Db.UserName(q.OwnerUserId),
                Note = "Quote created",
                Total = q.Total,
                StatusAtSnapshot = q.Status
            });
        }
    }

    // ---- Builders ----

    private static DateTime Days(int offset) => DateTime.Today.AddDays(offset);

    private static Quote New(int id, string reference, int customer, int contact, int owner,
        string site, string project, DateTime issued, QuoteStatus status,
        decimal markup, decimal petrol, string? customerRef = null, string? delivery = null)
    {
        var q = new Quote
        {
            Id = id,
            Ref = reference,
            CustomerId = customer,
            ContactId = contact,
            OwnerUserId = owner,
            Site = site,
            Project = project,
            CustomerReference = customerRef,
            DeliveryAddress = delivery,
            IssueDate = issued,
            ValidUntil = issued.AddDays(30),   // their standing terms: valid 30 days
            Status = status,
            MarkupPct = markup,
            PetrolDelivery = petrol
        };
        Db.Quotes.Add(q);
        return q;
    }

    /// <summary>Adds a material line, snapshotting the resolved price at quote time.</summary>
    private static void Mat(Quote q, string productLine, string colour, int thickness, int width,
        decimal sheets, decimal disc = 0m)
    {
        var e = Db.FindEntry(productLine, colour, thickness, width);
        q.MaterialLines.Add(new QuoteMaterialLine
        {
            Id = _lineId++,
            CatalogueEntryId = e.Id,
            SupplierName = Db.SupplierName(e.SupplierId),
            ProductLineName = Db.ProductLineName(e.ProductLineId),
            ColourName = e.ColourName,
            SupplierCode = e.SupplierCode,
            BandCode = e.BandCode,
            SheetLengthMm = e.SheetLengthMm,
            SheetWidthMm = e.SheetWidthMm,
            ThicknessMm = e.ThicknessMm,
            ResolvedPricePerSqm = e.EffectivePricePerSqm,
            ResolvedPricePerSheet = e.EffectivePricePerSheet,
            PriceResolvedAt = q.IssueDate,
            SheetsToOrder = sheets,
            SupplierDiscountPct = disc
        });
    }

    private static void Cost(Quote q, string rateName, decimal qty)
    {
        var r = Db.FindRate(rateName);
        q.CostingLines.Add(new QuoteCostingLine
        {
            Id = _lineId++,
            RateItemId = r.Id,
            Group = r.Group,
            Name = r.Name,
            Unit = r.Unit,
            Rate = r.Rate,
            Quantity = qty,
            IsDerived = r.IsDerived,
            DerivationNote = r.DerivationNote,
            IsMarkedUp = r.IsMarkedUp
        });
    }

    private static void Quo(Quote q, string item, string description, decimal? qty, decimal? rate)
    {
        q.QuotationLines.Add(new QuotationLine
        {
            Id = _lineId++,
            Item = item,
            Description = description,
            Qty = qty,
            Rate = rate
        });
    }

    /// <summary>
    /// Adds the two derived costing lines. Sandpaper is total m2, transport is the
    /// sheet count - both computed, never typed.
    /// </summary>
    private static void Finalise(Quote q)
    {
        var sqm = q.MaterialLines.Sum(l => l.SquareMetres);
        var sheets = q.MaterialLines.Sum(l => l.SheetsToOrder);

        Cost(q, "Sandpaper & consumables", Math.Round(sqm, 2));
        Cost(q, "Transport", sheets);
    }
}