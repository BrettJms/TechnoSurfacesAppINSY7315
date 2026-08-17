using Microsoft.AspNetCore.Mvc;
using TechnoSurfacesApp.Data;
using TechnoSurfaces.Services;

namespace TechnoSurfacesApp.Controllers;

/// <summary>
/// Authentication screens. There is deliberately NO registration action - the
/// client confirmed all accounts are created by the MD and there is no public
/// sign-up page. New users arrive via Activate, from an emailed invite link.
/// </summary>
public class AccountController : Controller
{
    private readonly DemoSession _session;

    public AccountController(DemoSession session) => _session = session;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string? email, string? password)
    {
        // No authentication in the prototype - match on email, fall back to the MD.
        var user = Db.ActiveUsers.FirstOrDefault(u =>
                       u.Email.Equals(email?.Trim() ?? "", StringComparison.OrdinalIgnoreCase))
                   ?? Db.Users.First();

        _session.SignIn(user.Id);
        return RedirectToAction("Dashboard", "Home");
    }

    /// <summary>Demo-only role switch from the top bar.</summary>
    [HttpPost]
    public IActionResult Switch(int userId)
    {
        _session.SwitchTo(userId);
        var back = Request.Headers.Referer.ToString();
        return string.IsNullOrEmpty(back)
            ? RedirectToAction("Dashboard", "Home")
            : Redirect(back);
    }

    public IActionResult Logout()
    {
        _session.SignOut();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ActionName("ForgotPassword")]
    public IActionResult ForgotPasswordPost(string? email)
    {
        ViewData["Sent"] = true;
        ViewData["Email"] = email;
        return View("ForgotPassword");
    }

    /// <summary>Where an admin-invited user lands to set their first password.</summary>
    [HttpGet]
    public IActionResult Activate() => View();

    [HttpPost]
    [ActionName("Activate")]
    public IActionResult ActivatePost()
    {
        ViewData["Done"] = true;
        return View("Activate");
    }
}