using TechnoSurfaces.Models;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;

namespace TechnoSurfaces.Data;

public static class SeedUsers
{
    public static void Load()
    {
        Db.Users.AddRange(new[]
        {
            new AppUser
            {
                Id = 1, FullName = "Paul Schluter", Email = "paul@technosurfaces.co.za",
                Role = UserRole.ManagingDirector, IsActive = true,
                CreatedOn = new DateTime(2026, 1, 12),
                LastLogin = DateTime.Today.AddHours(-2)
            },
            // Placeholder estimators - the client confirmed the roles, not the names.
            new AppUser
            {
                Id = 2, FullName = "Lerato Mokoena", Email = "lerato@technosurfaces.co.za",
                Role = UserRole.Estimator, IsActive = true,
                CreatedOn = new DateTime(2026, 2, 3),
                LastLogin = DateTime.Today.AddHours(-5)
            },
            new AppUser
            {
                Id = 3, FullName = "Devan Naidoo", Email = "devan@technosurfaces.co.za",
                Role = UserRole.Estimator, IsActive = true,
                CreatedOn = new DateTime(2026, 4, 18),
                LastLogin = DateTime.Today.AddDays(-1)
            },
            new AppUser
            {
                Id = 4, FullName = "Renaldo Fisher", Email = "renaldo@technosurfaces.co.za",
                Role = UserRole.Estimator, IsActive = false,
                CreatedOn = new DateTime(2025, 9, 2),
                LastLogin = new DateTime(2026, 6, 11)
            }
        });
    }
}