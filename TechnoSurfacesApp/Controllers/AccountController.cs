using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Services;

namespace TechnoSurfacesApp.Controllers;

/// <summary>
/// Authentication screens. There is deliberately no registration action: all
/// accounts are created by the Managing Director (US-26). The sign-in rules live
/// in ISignInService; this controller only validates input and chooses the page.
/// </summary>
public class AccountController : Controller
{
    private readonly ISignInService _signIn;

    public AccountController(ISignInService signIn) => _signIn = signIn;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var outcome = await _signIn.SignInAsync(model.Email, model.Password, model.RememberMe);

        if (outcome == SignInOutcome.Succeeded)
        {
            // Only return to a page on this site - blocks open-redirect attacks.
            return Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl!)
                : RedirectToAction("Dashboard", "Home");
        }

        // One message for an unknown email and a wrong password, so the form
        // cannot be used to discover which accounts exist.
        ModelState.AddModelError(string.Empty, outcome == SignInOutcome.LockedOut
            ? "Too many failed attempts. This account is temporarily locked - try again later or ask the Managing Director."
            : "The email address or password is incorrect.");

        model.Password = string.Empty;
        return View(model);
    }

    /// <summary>POST only, so another site cannot sign a user out with a link or image tag.</summary>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // Forgot-password and activation are rebuilt on Thursday (user admin).
    // They remain the prototype's placeholder screens until then.

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous]
    [HttpPost]
    [ActionName("ForgotPassword")]
    public IActionResult ForgotPasswordPost(string? email)
    {
        ViewData["Sent"] = true;
        ViewData["Email"] = email;
        return View("ForgotPassword");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Activate() => View();

    [AllowAnonymous]
    [HttpPost]
    [ActionName("Activate")]
    public IActionResult ActivatePost()
    {
        ViewData["Done"] = true;
        return View("Activate");
    }
}