using Microsoft.AspNetCore.Identity;
using TechnoSurfacesApp.Identity;

namespace TechnoSurfacesApp.Services;

/// <summary>
/// Sign-in through ASP.NET Core Identity. Identity verifies the PBKDF2 password
/// hash and applies the lockout policy configured in Program.cs.
/// </summary>
public sealed class SignInService : ISignInService
{
    private readonly SignInManager<UserAccount> _signInManager;
    private readonly ILogger<SignInService> _logger;

    public SignInService(SignInManager<UserAccount> signInManager, ILogger<SignInService> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<SignInOutcome> SignInAsync(string email, string password, bool rememberMe)
    {
        // lockoutOnFailure: true is what makes the lockout policy take effect.
        // Passing false here would silently disable it.
        var result = await _signInManager.PasswordSignInAsync(
            email.Trim(), password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return SignInOutcome.Succeeded;

        if (result.IsLockedOut)
        {
            // No email address or name in the log (POPIA, Task 1 §8.5).
            _logger.LogWarning("Sign-in refused: account is locked out.");
            return SignInOutcome.LockedOut;
        }

        _logger.LogInformation("Sign-in failed: invalid credentials.");
        return SignInOutcome.InvalidCredentials;
    }

    public Task SignOutAsync() => _signInManager.SignOutAsync();
}