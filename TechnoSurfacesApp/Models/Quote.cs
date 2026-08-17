using TechnoSurfacesApp.Models;

namespace TechnoSurfaces.Models;

public class Quote
{
    public int Id { get; set; }
    public string Ref { get; set; } = "";

    public int CustomerId { get; set; }
    public int ContactId { get; set; }

    /// <summary>Where the work happens. Quote-level, not customer-level.</summary>
    public string Site { get; set; } = "";

    /// <summary>What the job is. Also quote-level.</summary>
    public string Project { get; set; } = "";

    /// <summary>The customer's own reference - "Your Reference" on the Pastel invoice.</summary>
    public string? CustomerReference { get; set; }

    public string? DeliveryAddress { get; set; }

    public DateTime IssueDate { get; set; }

    /// <summary>Their standing terms say "Quotation valid for 30 days only".</summary>
    public DateTime ValidUntil { get; set; }

    public QuoteStatus Status { get; set; }
    public int OwnerUserId { get; set; }
    public int VersionNumber { get; set; } = 1;

    /// <summary>Overridable per quote by the estimator.</summary>
    public decimal MarkupPct { get; set; } = 35m;

    /// <summary>Discount GRANTED to the customer, applied at document level.
    /// Not to be confused with the supplier discount on each material line.</summary>
    public decimal CustomerDiscountPct { get; set; }

    /// <summary>Entered as a rand amount, not a quantity x rate. Below the line.</summary>
    public decimal PetrolDelivery { get; set; }

    public List<QuoteMaterialLine> MaterialLines { get; set; } = new();
    public List<QuoteCostingLine> CostingLines { get; set; } = new();
    public List<QuotationLine> QuotationLines { get; set; } = new();
    public List<QuoteVersion> Versions { get; set; } = new();

    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Pastel remains the book of record for invoicing - we only record the link.
    public string? PastelInvoiceNo { get; set; }
    public DateTime? PastelInvoiceDate { get; set; }
    public decimal? PastelInvoiceAmount { get; set; }

    // ---- Totals, in the confirmed order of operations ----

    public decimal MaterialTotal => MaterialLines.Sum(l => l.LineTotal);

    public decimal MarkedUpLinesTotal => CostingLines.Where(l => l.IsMarkedUp).Sum(l => l.LineTotal);

    /// <summary>material + fabrication + installation + wood + sinks</summary>
    public decimal SubTotal => MaterialTotal + MarkedUpLinesTotal;

    /// <summary>Markup applies to the SUBTOTAL ONLY.</summary>
    public decimal MarkupAmount => Math.Round(SubTotal * (MarkupPct / 100m), 2);

    /// <summary>Cost recovery added after the markup: petrol, grooves, transport, cut-outs.</summary>
    public decimal BelowTheLineTotal =>
        CostingLines.Where(l => !l.IsMarkedUp).Sum(l => l.LineTotal) + PetrolDelivery;

    public decimal Total => SubTotal + MarkupAmount + BelowTheLineTotal;

    /// <summary>The estimator's sanity check at the end - is this in the right range?</summary>
    public decimal TotalSqm => MaterialLines.Sum(l => l.SquareMetres);

    public decimal RandPerSqm => TotalSqm == 0 ? 0m : Math.Round(Total / TotalSqm, 2);

    // ---- Customer facing figures (quotation document) ----

    public decimal QuotationSubTotal => QuotationLines.Sum(l => l.LineTotal);
    public decimal QuotationDiscount => Math.Round(QuotationSubTotal * (CustomerDiscountPct / 100m), 2);
    public decimal QuotationExVat => QuotationSubTotal - QuotationDiscount;
    public decimal QuotationVat => Math.Round(QuotationExVat * 0.15m, 2);
    public decimal QuotationIncVat => QuotationExVat + QuotationVat;

    // ---- Convenience ----

    public bool IsExpired => DateTime.Today > ValidUntil && Status != QuoteStatus.Accepted;
    public int DaysRemaining => (ValidUntil - DateTime.Today).Days;

    public string StatusLabel => Status switch
    {
        QuoteStatus.Draft => "Draft",
        QuoteStatus.PendingApproval => "Pending approval",
        QuoteStatus.Approved => "Approved",
        QuoteStatus.Sent => "Sent",
        QuoteStatus.Accepted => "Accepted",
        _ => "Invoiced"
    };

    /// <summary>Maps to a CSS modifier class on the status pill.</summary>
    public string StatusKey => Status.ToString().ToLowerInvariant();
}

/// <summary>
/// One material on the quote. Every price field is a SNAPSHOT taken when the line
/// was created - never a live lookup, so reopening an old quote cannot silently
/// reprice it.
/// </summary>
public class QuoteMaterialLine
{
    public int Id { get; set; }
    public int CatalogueEntryId { get; set; }

    // Snapshot of the resolved catalogue entry
    public string SupplierName { get; set; } = "";
    public string ProductLineName { get; set; } = "";
    public string ColourName { get; set; } = "";
    public string SupplierCode { get; set; } = "";
    public string? BandCode { get; set; }
    public int SheetLengthMm { get; set; }
    public int SheetWidthMm { get; set; }
    public int ThicknessMm { get; set; }
    public decimal ResolvedPricePerSqm { get; set; }
    public decimal ResolvedPricePerSheet { get; set; }
    public DateTime PriceResolvedAt { get; set; }

    /// <summary>Fractional by design - stock is quoted in half sheets.</summary>
    public decimal SheetsToOrder { get; set; }

    /// <summary>Discount RECEIVED from the supplier, negotiated per job.
    /// Reduces cost before markup, so it also lowers the customer's price.</summary>
    public decimal SupplierDiscountPct { get; set; }

    // ---- Derived ----

    public decimal LinearMetres => Math.Round(SheetsToOrder * (SheetLengthMm / 1000m), 2);

    public decimal SquareMetres =>
        Math.Round(SheetsToOrder * (SheetLengthMm / 1000m) * (SheetWidthMm / 1000m), 4);

    public decimal LineTotal =>
        Math.Round(SheetsToOrder * ResolvedPricePerSheet * (1 - SupplierDiscountPct / 100m), 2);

    public string SizeLabel => $"{SheetLengthMm} \u00D7 {SheetWidthMm} \u00D7 {ThicknessMm}mm";
    public string Description => $"{ProductLineName} \u2014 {ColourName}";
}

/// <summary>A rate-card line on this quote, with the rate snapshotted at quote time.</summary>
public class QuoteCostingLine
{
    public int Id { get; set; }
    public int RateItemId { get; set; }

    public RateGroup Group { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Rate { get; set; }
    public decimal Quantity { get; set; }

    public bool IsDerived { get; set; }
    public string? DerivationNote { get; set; }
    public bool IsMarkedUp { get; set; } = true;

    public decimal LineTotal => Math.Round(Quantity * Rate, 2);
}

/// <summary>
/// A line on the CUSTOMER-FACING quotation. Descriptive, organised per room or
/// element. Carries no cost price and no markup - the estimator composes these
/// from the costing sheet, they do not fall out of it.
/// </summary>
public class QuotationLine
{
    public int Id { get; set; }
    public string Item { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>Null for qualifying notes that sit in the table without a price.</summary>
    public decimal? Qty { get; set; }
    public decimal? Rate { get; set; }

    public decimal LineTotal => (Qty ?? 0m) * (Rate ?? 0m);
    public bool IsNoteOnly => Qty is null || Rate is null;
}

/// <summary>
/// An immutable snapshot. Counter-offers are a first-class flow, so the original
/// offer must survive the edit.
/// </summary>
public class QuoteVersion
{
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public string Note { get; set; } = "";
    public decimal Total { get; set; }
    public QuoteStatus StatusAtSnapshot { get; set; }
}