namespace TechnoSurfaces.Domain.Auditing;

/// <summary>
/// A record of which user changed what, and when.
///
/// Necessary because the Managing Director corrects an estimator's quote directly
/// rather than returning it. Without an audit record an estimator would have no way
/// of knowing that their figures had been altered, so this is a control rather than
/// a convenience.
///
/// Rows are insert-only. There is no update or delete path, in code or in the
/// interface. Written by an EF Core SaveChanges interceptor so that it cannot be
/// bypassed by forgetting to call it.
/// </summary>
public class AuditEntry
{
    private AuditEntry() { }

    public AuditEntry(string entityName, string entityKey, string propertyName, string? oldValue, string? newValue, string userId)
    {
        EntityName = entityName;
        EntityKey = entityKey;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
        UserId = userId;
        ChangedAtUtc = DateTime.UtcNow;
    }

    public long Id { get; private set; }

    public string EntityName { get; private set; } = "";
    public string EntityKey { get; private set; } = "";
    public string PropertyName { get; private set; } = "";

    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    public string UserId { get; private set; } = "";
    public DateTime ChangedAtUtc { get; private set; }
}
