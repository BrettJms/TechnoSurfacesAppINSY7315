namespace TechnoSurfaces.Domain.Quoting;

/// <summary>
/// A reference to the tax invoice raised in the client's accounting system for an
/// accepted quote.
///
/// The application records the invoice; it does not produce it. Sage Pastel remains
/// the book of record, and this row exists so that the two systems can be
/// reconciled. An accepted quote has at most one invoice, and none until one is
/// raised.
/// </summary>
public class InvoiceRecord
{
    private InvoiceRecord() { }

    public InvoiceRecord(int quoteId, string invoiceNumber, DateOnly invoiceDate, decimal amountIncVat, string recordedByUserId)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("An invoice record needs the Pastel invoice number.", nameof(invoiceNumber));
        if (amountIncVat < 0)
            throw new ArgumentOutOfRangeException(nameof(amountIncVat), "An invoice amount cannot be negative.");

        QuoteId = quoteId;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        AmountIncVat = amountIncVat;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public int QuoteId { get; private set; }
    public Quote? Quote { get; private set; }

    /// <summary>The number as it appears in Sage Pastel.</summary>
    public string InvoiceNumber { get; private set; } = "";

    public DateOnly InvoiceDate { get; private set; }

    public decimal AmountIncVat { get; private set; }

    public string RecordedByUserId { get; private set; } = "";
    public DateTime RecordedAtUtc { get; private set; }
}
