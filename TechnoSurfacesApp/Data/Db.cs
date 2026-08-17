using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// In-memory demo store standing in for Azure SQL. The prototype has no back end,
/// so everything lives here for the lifetime of the process.
/// </summary>
public static class Db
{
    private static bool _loaded;

    public static List<Supplier> Suppliers { get; } = new();
    public static List<ProductLine> ProductLines { get; } = new();
    public static List<PriceBand> PriceBands { get; } = new();
    public static List<CatalogueEntry> Catalogue { get; } = new();
    public static List<RateItem> Rates { get; } = new();
    public static List<AppUser> Users { get; } = new();
    public static List<Customer> Customers { get; } = new();
    public static List<Quote> Quotes { get; } = new();
    public static List<AuditEntry> Audit { get; } = new();

    public static void Initialise()
    {
        if (_loaded) return;
        SeedCatalogue.Load();
        SeedRates.Load();
        SeedUsers.Load();
        SeedCustomers.Load();
        SeedQuotes.Load();   // must run after the catalogue and rates
        SeedAudit.Load();
        _loaded = true;
    }

    // ---- Lookups ----

    public static Supplier? GetSupplier(int id) => Suppliers.FirstOrDefault(s => s.Id == id);

    public static ProductLine? GetProductLine(int id) => ProductLines.FirstOrDefault(p => p.Id == id);

    public static string SupplierName(int id) => GetSupplier(id)?.Name ?? "\u2014";

    public static string ProductLineName(int id) => GetProductLine(id)?.Name ?? "\u2014";

    /// <summary>Level 2 of the cascade.</summary>
    public static List<ProductLine> ProductLinesFor(int supplierId) =>
        ProductLines.Where(p => p.SupplierId == supplierId)
                    .OrderBy(p => p.Name)
                    .ToList();

    /// <summary>
    /// Level 3. Distinct colour names within a product line - a colour can appear
    /// more than once because it exists in several sizes or thicknesses.
    /// </summary>
    public static List<string> ColoursFor(int productLineId) =>
        Catalogue.Where(c => c.ProductLineId == productLineId)
                 .Select(c => c.ColourName)
                 .Distinct()
                 .OrderBy(n => n)
                 .ToList();

    /// <summary>
    /// Level 4. Every size/thickness variant of one colour. This is where the price
    /// finally resolves - Glacier White is R3 991, R4 831 or R2 287 depending on
    /// which of these the estimator picks.
    /// </summary>
    public static List<CatalogueEntry> VariantsFor(int productLineId, string colourName) =>
        Catalogue.Where(c => c.ProductLineId == productLineId &&
                             c.ColourName.Equals(colourName, StringComparison.OrdinalIgnoreCase))
                 .OrderByDescending(c => c.ThicknessMm)
                 .ThenBy(c => c.SheetWidthMm)
                 .ToList();

    public static CatalogueEntry? GetEntry(int id) => Catalogue.FirstOrDefault(c => c.Id == id);

    public static List<CatalogueEntry> EntriesForSupplier(int supplierId) =>
        Catalogue.Where(c => c.SupplierId == supplierId).ToList();

    public static PriceBand? GetBand(int supplierId, string code) =>
        PriceBands.FirstOrDefault(b => b.SupplierId == supplierId &&
                                       b.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static RateItem? GetRate(int id) => Rates.FirstOrDefault(r => r.Id == id);

    public static List<RateItem> RatesIn(RateGroup group) =>
        Rates.Where(r => r.Group == group).ToList();

    /// <summary>Counts for the catalogue admin screen header.</summary>
    public static int ActiveEntryCount => Catalogue.Count(c => c.Status == CatalogueStatus.Active);
    public static AppUser? GetUser(int id) => Users.FirstOrDefault(u => u.Id == id);

    public static string UserName(int id) => GetUser(id)?.FullName ?? "\u2014";

    public static List<AppUser> ActiveUsers => Users.Where(u => u.IsActive).ToList();
    /// <summary>
    /// Resolves a catalogue entry the way the cascade does - product line, colour,
    /// then thickness and width. Width is optional where a colour has only one.
    /// </summary>
    public static CatalogueEntry FindEntry(string productLineName, string colour,
        int thicknessMm, int? widthMm = null)
    {
        var plIds = ProductLines
            .Where(p => p.Name.Equals(productLineName, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Id).ToList();

        return Catalogue.First(c =>
            plIds.Contains(c.ProductLineId) &&
            c.ColourName.Equals(colour, StringComparison.OrdinalIgnoreCase) &&
            c.ThicknessMm == thicknessMm &&
            (widthMm is null || c.SheetWidthMm == widthMm));
    }

    public static RateItem FindRate(string name) =>
        Rates.First(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static Customer? GetCustomer(int id) => Customers.FirstOrDefault(c => c.Id == id);

    public static string CustomerName(int id) => GetCustomer(id)?.CompanyName ?? "\u2014";

    public static Contact? GetContact(int customerId, int contactId) =>
        GetCustomer(customerId)?.Contacts.FirstOrDefault(c => c.Id == contactId);

    public static Quote? GetQuote(int id) => Quotes.FirstOrDefault(q => q.Id == id);

    public static Quote? GetQuoteByRef(string reference) =>
        Quotes.FirstOrDefault(q => q.Ref.Equals(reference, StringComparison.OrdinalIgnoreCase));

    public static List<Quote> QuotesAwaitingApproval =>
        Quotes.Where(q => q.Status == QuoteStatus.PendingApproval)
              .OrderBy(q => q.IssueDate).ToList();

    public static List<Quote> QuotesOwnedBy(int userId) =>
        Quotes.Where(q => q.OwnerUserId == userId)
              .OrderByDescending(q => q.IssueDate).ToList();
}