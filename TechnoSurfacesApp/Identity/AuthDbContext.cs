using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TechnoSurfacesApp.Identity;

/// <summary>
/// The credential store. Kept in its own context and its own "auth" schema so that
/// password hashes and lockout data are separated from catalogue and quote data,
/// and so that its migrations never collide with the domain model's.
/// </summary>
public class AuthDbContext : IdentityDbContext<UserAccount>
{
    public const string Schema = "auth";

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
    }
}