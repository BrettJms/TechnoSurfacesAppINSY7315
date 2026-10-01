using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using TechnoSurfaces.Application.Auditing;
using TechnoSurfaces.Domain.Auditing;
using TechnoSurfaces.Domain.Catalogue;
using TechnoSurfaces.Domain.Quoting;

namespace TechnoSurfaces.Infrastructure.Data.Auditing;

/// <summary>
/// Writes an AuditEntry for every change to a price or a quote (NFR-04).
///
/// An interceptor rather than calls in each service, so the audit trail cannot be
/// bypassed by forgetting to call it - the point of an audit trail as a control.
/// The audit rows are written in the same transaction as the change they record:
/// if they cannot be written, the change is rolled back too.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    /// <summary>Width of AuditEntry.OldValue / NewValue.</summary>
    private const int MaxValueLength = 1000;

    private static readonly HashSet<Type> AuditedTypes = new()
    {
        typeof(MaterialPrice), typeof(RatePrice), typeof(Quote), typeof(QuoteVersion), typeof(CostingLine)
    };

    private readonly ICurrentUser _currentUser;
    private List<PendingChange>? _pending;
    private IDbContextTransaction? _ownTransaction;
    private bool _writingAudit;

    public AuditInterceptor(ICurrentUser currentUser) => _currentUser = currentUser;

    // ------------------------------------------------ before the save: capture

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } db && Capture(db) && db.Database.CurrentTransaction is null)
            _ownTransaction = db.Database.BeginTransaction();

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db && Capture(db) && db.Database.CurrentTransaction is null)
            _ownTransaction = await db.Database.BeginTransactionAsync(cancellationToken);

        return result;
    }

    // ------------------------------------------------ after the save: record

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_writingAudit || eventData.Context is not { } db || _pending is null)
            return result;

        try
        {
            _writingAudit = true;
            db.Set<AuditEntry>().AddRange(BuildEntries(_pending));
            db.SaveChanges();
            _ownTransaction?.Commit();
        }
        finally
        {
            Reset();
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_writingAudit || eventData.Context is not { } db || _pending is null)
            return result;

        try
        {
            _writingAudit = true;
            db.Set<AuditEntry>().AddRange(BuildEntries(_pending));
            await db.SaveChangesAsync(cancellationToken);
            if (_ownTransaction is not null)
                await _ownTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            Reset();
        }

        return result;
    }

    // ------------------------------------------------ the save failed: roll back

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (!_writingAudit) Reset();
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!_writingAudit) Reset();
        return Task.CompletedTask;
    }

    // ------------------------------------------------ helpers

    private bool Capture(DbContext db)
    {
        _pending = null;
        if (_writingAudit) return false;

        var pending = new List<PendingChange>();

        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (!AuditedTypes.Contains(entry.Metadata.ClrType)) continue;

            IEnumerable<PropertyChange> changes = entry.State switch
            {
                EntityState.Added => entry.Properties
                    .Where(p => !p.Metadata.IsPrimaryKey() && p.CurrentValue is not null)
                    .Select(p => new PropertyChange(p.Metadata.Name, null, Format(p.CurrentValue))),

                EntityState.Modified => entry.Properties
                    .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                    .Select(p => new PropertyChange(p.Metadata.Name, Format(p.OriginalValue), Format(p.CurrentValue))),

                EntityState.Deleted => new[] { new PropertyChange("(deleted)", null, null) },

                _ => Enumerable.Empty<PropertyChange>()
            };

            var list = changes.ToList();
            if (list.Count == 0) continue;

            // A new row's key is only known after the save, so it is read then.
            var keyBeforeSave = entry.State == EntityState.Added ? null : KeyOf(entry);
            pending.Add(new PendingChange(entry, entry.Metadata.ClrType.Name, keyBeforeSave, list));
        }

        _pending = pending.Count > 0 ? pending : null;
        return _pending is not null;
    }

    private IEnumerable<AuditEntry> BuildEntries(List<PendingChange> pending)
    {
        var userId = _currentUser.UserId;

        foreach (var change in pending)
        {
            var key = change.KeyBeforeSave ?? KeyOf(change.Entry);
            foreach (var p in change.Properties)
                yield return new AuditEntry(change.EntityName, key, p.Name, p.OldValue, p.NewValue, userId);
        }
    }

    private void Reset()
    {
        _pending = null;
        _writingAudit = false;
        _ownTransaction?.Dispose();   // disposing without a commit rolls back
        _ownTransaction = null;
    }

    private static string KeyOf(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        return key is null
            ? ""
            : string.Join(",", key.Properties.Select(p => Format(entry.Property(p.Name).CurrentValue)));
    }

    private static string? Format(object? value)
    {
        var text = value switch
        {
            null => null,
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

        return text is { Length: > MaxValueLength } ? text[..(MaxValueLength - 3)] + "..." : text;
    }

    private sealed record PropertyChange(string Name, string? OldValue, string? NewValue);

    private sealed record PendingChange(
        EntityEntry Entry, string EntityName, string? KeyBeforeSave, List<PropertyChange> Properties);
}