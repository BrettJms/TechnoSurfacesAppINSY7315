using System.Security.Claims;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Identity;
using TechnoSurfacesApp.Models;

namespace TechnoSurfaces.Services;

/// <summary>
/// Bridges the signed-in Identity user to the prototype's in-memory user record,
/// so the existing screens keep working until they move onto the database-backed
/// domain model. It no longer signs anyone in: that is ISignInService's job.
/// </summary>
public class DemoSession
{
    private readonly IHttpContextAccessor _http;

    public DemoSession(IHttpContextAccessor http) => _http = http;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsSignedIn => Principal?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Taken from the Identity role claim, not the prototype record, because the
    /// claim is what the authorisation policies check.
    /// </summary>
    public bool IsMd => Principal?.IsInRole(Roles.ManagingDirector) == true;

    /// <summary>
    /// The prototype record for the signed-in account. Fails rather than falling
    /// back to another user - the prototype fell back to the MD, which would have
    /// handed MD access to any account without a matching record.
    /// </summary>
    public AppUser User
    {
        get
        {
            var email = Principal?.Identity?.Name ?? "";
            return Db.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("The signed-in account has no matching user record.");
        }
    }
}