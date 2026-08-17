using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// The audit trail. Because the MD corrects an estimator's quote silently rather
/// than rejecting it, this is the only place a correction becomes visible to the
/// person who wrote the quote.
/// </summary>
public static class SeedAudit
{
    private static int _id = 1;

    private static void A(int daysAgo, int hoursAgo, string user, string action,
        string entityType, string entityRef,
        string? field = null, string? oldVal = null, string? newVal = null)
    {
        Db.Audit.Add(new AuditEntry
        {
            Id = _id++,
            When = DateTime.Today.AddDays(-daysAgo).AddHours(-hoursAgo),
            UserName = user,
            Action = action,
            EntityType = entityType,
            EntityRef = entityRef,
            Field = field,
            OldValue = oldVal,
            NewValue = newVal
        });
    }

    public static void Load()
    {
        A(0, 2, "Lerato Mokoena", "Created quote", "Quote", "TS-2026-0141");
        A(0, 1, "Lerato Mokoena", "Added material line", "Quote", "TS-2026-0141",
            "Material", null, "Infinito Modified \u2014 CONCRETE GREY, 3680 \u00D7 760 \u00D7 12mm");

        A(1, 4, "Paul Schluter", "Changed material price", "Catalogue", "Max on Top \u00B7 Glacier White 8016 (3680 \u00D7 920)",
            "Price per sheet", "R4 655,00", "R4 831,00");
        A(1, 4, "Paul Schluter", "Changed material price", "Catalogue", "Max on Top \u00B7 Aspen 5230 (3680 \u00D7 920)",
            "Price per sheet", "R5 480,00", "R5 654,00");

        A(2, 6, "Devan Naidoo", "Reopened quote after counter-offer", "Quote", "TS-2026-0134",
            "Version", "2", "3");
        A(2, 6, "Devan Naidoo", "Changed markup", "Quote", "TS-2026-0134",
            "Markup %", "30%", "28%");

        A(3, 3, "Devan Naidoo", "Submitted for approval", "Quote", "TS-2026-0140");

        A(5, 7, "Lerato Mokoena", "Submitted for approval", "Quote", "TS-2026-0139");

        // The silent correction the audit trail exists to expose.
        A(8, 5, "Paul Schluter", "Corrected and approved quote", "Quote", "TS-2026-0138",
            "Supplier discount", "5%", "12,5%");
        A(8, 5, "Paul Schluter", "Approved quote", "Quote", "TS-2026-0138");

        A(10, 2, "Paul Schluter", "Approved quote", "Quote", "TS-2026-0137");
        A(11, 8, "Devan Naidoo", "Marked quote as sent", "Quote", "TS-2026-0137");

        A(14, 5, "Paul Schluter", "Retired catalogue entry", "Catalogue", "Max on Top \u00B7 GetaCore Dusk GC4143",
            "Status", "Active", "Phasing out");

        A(18, 4, "Paul Schluter", "Approved quote", "Quote", "TS-2026-0136");
        A(19, 6, "Lerato Mokoena", "Recorded customer acceptance", "Quote", "TS-2026-0136",
            "Status", "Sent", "Accepted");

        A(21, 3, "Paul Schluter", "Changed rate", "Rate card", "Fabrication \u2014 with backsplash, normal",
            "Rate", "R420,00", "R445,00");

        A(27, 5, "Paul Schluter", "Recorded Pastel invoice", "Quote", "TS-2026-0135",
            "Invoice no.", null, "IN114317");

        A(31, 2, "Paul Schluter", "Deactivated user", "User", "Renaldo Fisher",
            "Active", "Yes", "No");
        A(45, 6, "Paul Schluter", "Created user", "User", "Devan Naidoo",
            "Role", null, "Estimator");
    }
}