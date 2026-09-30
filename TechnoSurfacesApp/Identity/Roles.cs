namespace TechnoSurfacesApp.Identity;

/// <summary>
/// The two roles the client defined (Task 1 2.2). Held as constants so that
/// authorisation policies and seeding refer to one spelling.
/// </summary>
public static class Roles
{
    public const string ManagingDirector = "ManagingDirector";
    public const string Estimator = "Estimator";

    public static readonly string[] All = { ManagingDirector, Estimator };
}