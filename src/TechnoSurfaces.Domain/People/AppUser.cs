namespace TechnoSurfaces.Domain.People;

/// <summary>
/// A person who signs in to the system.
///
/// This is the domain view of a user. The credential store is ASP.NET Core Identity
/// in the Web layer, which owns password hashing, lockout and session management.
/// Accounts are created by the Managing Director; there is no self-registration.
/// </summary>
public class AppUser
{
    /// <summary>Matches the Identity user id.</summary>
    public string Id { get; set; } = "";

    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }

    public UserRole Role { get; set; } = UserRole.Estimator;

    /// <summary>A deactivated account cannot sign in.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public bool IsManagingDirector => Role == UserRole.ManagingDirector;
}
