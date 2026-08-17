using Microsoft.AspNetCore.Mvc;
using TechnoSurfacesApp.Data;
using TechnoSurfacesApp.Models;
using TechnoSurfaces.Services;
using TechnoSurfacesApp.Controllers;
using static System.Collections.Specialized.BitVector32;
using TechnoSurfaces.Models;

namespace TechnoSurfacesApp.Controllers;

/// <summary>
/// Customers and their contacts. Seeded from a one-off Pastel Customer Masterfile
/// export at cut-over rather than a live integration.
/// </summary>
public class CustomersController : AppController
{
    public CustomersController(DemoSession session) : base(session) { }

    public IActionResult Index(string? q)
    {
        var list = Db.Customers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim();
            list = list.Where(c =>
                c.CompanyName.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                c.AccountCode.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                c.Contacts.Any(k => k.Name.Contains(t, StringComparison.OrdinalIgnoreCase)));
        }

        ViewData["Title"] = "Customers";
        ViewData["Page"] = "customers";
        ViewData["Crumb"] = "Data";

        return View(new CustomerListVm
        {
            Rows = list
                .OrderBy(c => c.CompanyName)
                .Select(c => new CustomerRow(c, Db.Quotes.Where(x => x.CustomerId == c.Id).ToList()))
                .ToList(),
            Search = q,
            CanManage = Session.IsMd
        });
    }

    public IActionResult Details(int id)
    {
        var customer = Db.GetCustomer(id);
        if (customer is null) return RedirectToAction(nameof(Index));

        ViewData["Title"] = customer.CompanyName;
        ViewData["Page"] = "customers";
        ViewData["Crumb"] = "Data \u203A Customers";

        return View(new CustomerDetailVm
        {
            Customer = customer,
            Quotes = Db.Quotes
                .Where(x => x.CustomerId == id)
                .OrderByDescending(x => x.IssueDate)
                .ToList(),
            CanManage = Session.IsMd
        });
    }
}

// ==========================================================================
//  View models
// ==========================================================================

public class CustomerRow
{
    public CustomerRow(Customer customer, List<Quote> quotes)
    {
        Customer = customer;
        Quotes = quotes;
    }

    public Customer Customer { get; }
    public List<Quote> Quotes { get; }

    public decimal TotalValue => Quotes.Sum(q => q.Total);

    public decimal WonValue => Quotes
        .Where(q => q.Status is QuoteStatus.Accepted or QuoteStatus.Invoiced)
        .Sum(q => q.Total);

    public DateTime? LastQuoted => Quotes.Any() ? Quotes.Max(q => q.IssueDate) : null;
}

public class CustomerListVm
{
    public List<CustomerRow> Rows { get; set; } = new();
    public string? Search { get; set; }
    public bool CanManage { get; set; }

    public int TotalContacts => Rows.Sum(r => r.Customer.Contacts.Count);
    public decimal TotalValue => Rows.Sum(r => r.TotalValue);
}

public class CustomerDetailVm
{
    public Customer Customer { get; set; } = null!;
    public List<Quote> Quotes { get; set; } = new();
    public bool CanManage { get; set; }

    public decimal TotalValue => Quotes.Sum(q => q.Total);

    public decimal WonValue => Quotes
        .Where(q => q.Status is QuoteStatus.Accepted or QuoteStatus.Invoiced)
        .Sum(q => q.Total);

    public int OpenCount => Quotes.Count(q =>
        q.Status is QuoteStatus.Draft or QuoteStatus.PendingApproval
                 or QuoteStatus.Approved or QuoteStatus.Sent);
}