using TechnoSurfaces.Domain.People;

namespace TechnoSurfaces.Domain.Quoting;

/// <summary>
/// A priced offer for a single job. Holds the identity of the job, its reference,
/// site, project and period of validity, together with the commercial terms under
/// which it is offered.
///
/// The originator in the Memento pattern: a quote owns an ordered series of
/// immutable <see cref="QuoteVersion"/> snapshots. Reopening a quote after a
/// customer counter-offer produces a new version and leaves the earlier one intact.
/// </summary>
public class Quote
{
    private readonly List<QuoteVersion> _versions = new();

    private Quote() { }

    public Quote(string reference, int customerId, int contactId, string createdByUserId, DateOnly issueDate, int validForDays = 30)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A quote needs a reference.", nameof(reference));

        Reference = reference;
        CustomerId = customerId;
        ContactId = contactId;
        CreatedByUserId = createdByUserId;
        IssueDate = issueDate;
        ValidUntil = issueDate.AddDays(validForDays);
        Status = QuoteStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    /// <summary>Unique. One reference identifies one quote.</summary>
    public string Reference { get; private set; } = "";

    public int CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    /// <summary>The quotation is addressed to a named person.</summary>
    public int ContactId { get; private set; }
    public Contact? Contact { get; private set; }

    public string? Site { get; set; }
    public string? Project { get; set; }

    public DateOnly IssueDate { get; private set; }
    public DateOnly ValidUntil { get; private set; }

    public QuoteStatus Status { get; private set; }

    public string CreatedByUserId { get; private set; } = "";
    public DateTime CreatedAtUtc { get; private set; }

    public string? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }

    public IReadOnlyCollection<QuoteVersion> Versions => _versions.AsReadOnly();

    /// <summary>The version currently being worked on or last issued.</summary>
    public QuoteVersion? CurrentVersion => _versions.OrderByDescending(v => v.VersionNo).FirstOrDefault();

    /// <summary>The original offer, retained so that it can be compared with a revision.</summary>
    public QuoteVersion? OriginalVersion => _versions.OrderBy(v => v.VersionNo).FirstOrDefault();

    /// <summary>
    /// Starts a new snapshot. The previous version is sealed first, so earlier
    /// versions can never be altered by a later revision.
    /// </summary>
    public QuoteVersion StartNewVersion(string createdByUserId, decimal markupPercent)
    {
        CurrentVersion?.Seal();
        var next = new QuoteVersion((CurrentVersion?.VersionNo ?? 0) + 1, createdByUserId, markupPercent);
        _versions.Add(next);
        return next;
    }

    public void SetStatus(QuoteStatus status) => Status = status;

    public void RecordApproval(string userId)
    {
        ApprovedByUserId = userId;
        ApprovedAtUtc = DateTime.UtcNow;
    }
}
