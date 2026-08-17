using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Models;

/// <summary>
/// Accounts are created by the MD only - there is no self-registration and no
/// public sign-up page.
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedOn { get; set; }

    public string RoleLabel => Role == UserRole.ManagingDirector ? "Managing Director" : "Estimator";

    public string Initials => string.Concat(
        FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(p => p[0]))
        .ToUpperInvariant();

    public bool CanApprove => Role == UserRole.ManagingDirector;
    public bool CanEditPrices => Role == UserRole.ManagingDirector;
    public bool CanManageUsers => Role == UserRole.ManagingDirector;
}

/// <summary>
/// The client called this quality-of-life, but because the MD corrects an
/// estimator's quote silently rather than rejecting it, this is the only thing
/// that makes a correction visible to the person who wrote it.
/// </summary>
public class AuditEntry
{
    public int Id { get; set; }
    public DateTime When { get; set; }
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityRef { get; set; } = "";
    public string? Field { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public bool IsPriceChange => Field is not null &&
        (Field.Contains("price", StringComparison.OrdinalIgnoreCase) ||
         Field.Contains("rate", StringComparison.OrdinalIgnoreCase) ||
         Field.Contains("markup", StringComparison.OrdinalIgnoreCase));
}