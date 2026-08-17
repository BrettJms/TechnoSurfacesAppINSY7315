using TechnoSurfaces.Data;
using TechnoSurfaces.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;

namespace TechnoSurfaces.Services;

/// <summary>
/// Stands in for authentication. The prototype has no back end, so "who is signed
/// in" is just a user id on the session. Real authentication is application-managed
/// usernames and passwords - see the Security section of the report.
/// </summary>
public class DemoSession
{
    private const string Key = "ts_user_id";
    private readonly IHttpContextAccessor _http;

    public DemoSession(IHttpContextAccessor http) => _http = http;

    private ISession? Session => _http.HttpContext?.Session;

    public bool IsSignedIn => Session?.GetInt32(Key) is not null;

    public AppUser? CurrentUser
    {
        get
        {
            var id = Session?.GetInt32(Key);
            return id is null ? null : Db.GetUser(id.Value);
        }
    }

    /// <summary>Falls back to the MD so a deep link never renders a broken page.</summary>
    public AppUser User => CurrentUser ?? Db.Users.First();

    public bool IsMd => User.Role == UserRole.ManagingDirector;

    public void SignIn(int userId) => Session?.SetInt32(Key, userId);

    public void SignOut() => Session?.Remove(Key);

    /// <summary>Demo-only: flip role without going back through the login screen.</summary>
    public void SwitchTo(int userId) => SignIn(userId);
}