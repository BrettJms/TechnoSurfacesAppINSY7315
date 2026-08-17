using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Services;

namespace TechnoSurfacesApp.Controllers;

/// <summary>Shared base - supplies the data every page's chrome needs.</summary>
public abstract class AppController : Controller
{
    protected readonly DemoSession Session;

    protected AppController(DemoSession session) => Session = session;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["PendingCount"] = Db.QuotesAwaitingApproval.Count;
        base.OnActionExecuting(context);
    }

    /// <summary>Temporary scaffold for screens still being built.</summary>
    protected IActionResult Soon(string title, string page, string crumb,
        string summary, params string[] contents)
    {
        ViewData["Title"] = title;
        ViewData["Page"] = page;
        ViewData["Crumb"] = crumb;
        ViewData["Summary"] = summary;
        ViewData["Contents"] = contents;
        return View("ComingSoon");
    }
}