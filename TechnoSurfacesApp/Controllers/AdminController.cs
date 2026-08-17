using Microsoft.AspNetCore.Mvc;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;
using TechnoSurfaces.Services;
using static System.Collections.Specialized.BitVector32;

namespace TechnoSurfacesApp.Controllers;

/// <summary>
/// Managing Director only. Price changes live in CatalogueController; this
/// controller covers the rate card, user accounts, quotation boilerplate and
/// the audit trail.
/// </summary>
public class AdminController : AppController
{
    public AdminController(DemoSession session) : base(session) { }

    // ======================================================================
    //  Rate card
    // ======================================================================

    public IActionResult Rates()
    {
        ViewData["Title"] = "Rate card";
        ViewData["Page"] = "rates";
        ViewData["Crumb"] = "Administration";

        return View(new RatesVm
        {
            CanEdit = Session.IsMd,
            Groups = RateGroupOrder
                .Select(g => (g, Db.RatesIn(g)))
                .Where(x => x.Item2.Count > 0)
                .ToList()
        });
    }

    public static readonly RateGroup[] RateGroupOrder =
    {
        RateGroup.Fabrication, RateGroup.Consumables, RateGroup.Installation,
        RateGroup.WoodSubstrate, RateGroup.SinksHardware, RateGroup.BelowTheLine
    };

    // ======================================================================
    //  Users
    // ======================================================================

    public IActionResult Users()
    {
        ViewData["Title"] = "Users";
        ViewData["Page"] = "users";
        ViewData["Crumb"] = "Administration";

        return View(new UsersVm
        {
            Users = Db.Users.OrderByDescending(u => u.IsActive).ThenBy(u => u.FullName).ToList(),
            CanManage = Session.IsMd,
            Me = Session.User
        });
    }

    // ======================================================================
    //  Quotation terms - the standing content on every customer quotation
    // ======================================================================

    public IActionResult Terms()
    {
        ViewData["Title"] = "Quotation terms";
        ViewData["Page"] = "terms";
        ViewData["Crumb"] = "Administration";

        return View(new TermsVm { CanEdit = Session.IsMd });
    }

    // ======================================================================
    //  Audit trail
    // ======================================================================

    public IActionResult Audit(string? type, string? user, string? priceOnly)
    {
        var list = Db.Audit.AsEnumerable();

        if (!string.IsNullOrEmpty(type))
            list = list.Where(a => a.EntityType == type);

        if (!string.IsNullOrEmpty(user))
            list = list.Where(a => a.UserName == user);

        if (priceOnly == "1")
            list = list.Where(a => a.IsPriceChange);

        ViewData["Title"] = "Audit trail";
        ViewData["Page"] = "audit";
        ViewData["Crumb"] = "Administration";

        return View(new AuditVm
        {
            Entries = list.OrderByDescending(a => a.When).ToList(),
            Type = type,
            User = user,
            PriceOnly = priceOnly == "1",
            EntityTypes = Db.Audit.Select(a => a.EntityType).Distinct().OrderBy(t => t).ToList(),
            Users = Db.Audit.Select(a => a.UserName).Distinct().OrderBy(u => u).ToList()
        });
    }
}

// ==========================================================================
//  View models
// ==========================================================================

public class RatesVm
{
    public bool CanEdit { get; set; }
    public List<(RateGroup Group, List<RateItem> Rows)> Groups { get; set; } = new();

    public string GroupName(RateGroup g) => g switch
    {
        RateGroup.Fabrication => "Fabrication",
        RateGroup.Consumables => "Consumables",
        RateGroup.Installation => "Installation",
        RateGroup.WoodSubstrate => "Wood & substrate",
        RateGroup.SinksHardware => "Sinks & hardware",
        _ => "Below the line \u2014 cost recovery, not marked up"
    };
}

public class UsersVm
{
    public List<AppUser> Users { get; set; } = new();
    public bool CanManage { get; set; }
    public AppUser Me { get; set; } = null!;

    public int ActiveCount => Users.Count(u => u.IsActive);
    public int MdCount => Users.Count(u => u.Role == UserRole.ManagingDirector);
    public int EstimatorCount => Users.Count(u => u.Role == UserRole.Estimator && u.IsActive);
}

public class TermsVm
{
    public bool CanEdit { get; set; }
}

public class AuditVm
{
    public List<AuditEntry> Entries { get; set; } = new();
    public string? Type { get; set; }
    public string? User { get; set; }
    public bool PriceOnly { get; set; }
    public List<string> EntityTypes { get; set; } = new();
    public List<string> Users { get; set; } = new();

    public bool AnyFilter => !string.IsNullOrEmpty(Type) || !string.IsNullOrEmpty(User) || PriceOnly;

    public int PriceChangeCount => Entries.Count(e => e.IsPriceChange);
}