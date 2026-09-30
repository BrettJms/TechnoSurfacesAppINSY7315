using Microsoft.AspNetCore.Identity;

namespace TechnoSurfacesApp.Identity;

/// <summary>
/// Creates the two roles in every environment, and - in Development only - the
/// prototype's three users so the app can be signed in to locally.
///
/// The development password is read from user secrets, never from source control.
/// Real accounts are created by the Managing Director through the Users screen.
/// </summary>
public static class IdentitySeeder
{
    private static readonly (string Email, string Role)[] DevelopmentAccounts =
    {
        ("paul@technosurfaces.co.za",   Roles.ManagingDirector),
        ("lerato@technosurfaces.co.za", Roles.Estimator),
        ("devan@technosurfaces.co.za",  Roles.Estimator),
    };

    public static async Task SeedAsync(
        IServiceProvider services, IConfiguration configuration, ILogger logger, bool includeDevelopmentAccounts)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!includeDevelopmentAccounts)
            return;

        var password = configuration["Seed:DevelopmentPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:DevelopmentPassword is not set in user secrets; development accounts were not created.");
            return;
        }

        foreach (var (email, role) in DevelopmentAccounts)
        {
            if (await userManager.FindByEmailAsync(email) is not null)
                continue;

            var account = new UserAccount
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                MustChangePassword = false
            };

            var created = await userManager.CreateAsync(account, password);
            if (!created.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create development account {email}: " +
                    string.Join("; ", created.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(account, role);
        }
    }
}