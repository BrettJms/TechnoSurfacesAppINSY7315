using Microsoft.AspNetCore.Mvc;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;
using TechnoSurfacesApp.Services;
using TechnoSurfacesApp.Controllers;
using TechnoSurfacesApp.Models;
using static System.Collections.Specialized.BitVector32;

namespace TechnoSurfacesApp.Controllers;

public class HomeController : AppController
{
    public HomeController(DemoSession session) : base(session) { }

    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    public IActionResult Dashboard()
    {
        var me = Session.User;

        var vm = new DashboardVm
        {
            User = me,
            Pending = Db.QuotesAwaitingApproval,
            MyQuotes = Db.QuotesOwnedBy(me.Id).Take(6).ToList(),
            RecentActivity = Db.Audit.OrderByDescending(a => a.When).Take(8).ToList(),
            AllQuotes = Db.Quotes,
            StaleSuppliers = Db.Suppliers.Where(s => s.IsStale).ToList()
        };

        ViewData["Title"] = "Dashboard";
        ViewData["Page"] = "dashboard";
        return View(vm);
    }
}

public class DashboardVm
{
    public AppUser User { get; set; } = null!;
    public List<Quote> Pending { get; set; } = new();
    public List<Quote> MyQuotes { get; set; } = new();
    public List<AuditEntry> RecentActivity { get; set; } = new();
    public List<Quote> AllQuotes { get; set; } = new();
    public List<Supplier> StaleSuppliers { get; set; } = new();

    public int CountIn(QuoteStatus s) => AllQuotes.Count(q => q.Status == s);

    public decimal OpenValue => AllQuotes
        .Where(q => q.Status is QuoteStatus.Sent or QuoteStatus.Approved or QuoteStatus.PendingApproval)
        .Sum(q => q.Total);

    public decimal AcceptedValue => AllQuotes
        .Where(q => q.Status is QuoteStatus.Accepted or QuoteStatus.Invoiced)
        .Sum(q => q.Total);

    public int ExpiringSoon => AllQuotes
        .Count(q => q.Status == QuoteStatus.Sent && q.DaysRemaining <= 7 && q.DaysRemaining >= 0);
}