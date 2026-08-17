namespace TechnoSurfacesApp.Models;

/// <summary>
/// Seeded from a one-off Pastel Customer Masterfile export at cut-over.
/// AccountCode and VatNumber come straight off the Pastel invoice.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string AccountCode { get; set; } = "";
    public string? VatNumber { get; set; }
    public string BillingAddress { get; set; } = "";
    public List<Contact> Contacts { get; set; } = new();
}

/// <summary>
/// A quote is addressed to a person, not a company - the quotation's "Attention"
/// field. Some customers have four or five people who send requests.
/// </summary>
public class Contact
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string? JobTitle { get; set; }
    public string? Tel { get; set; }
    public string? Email { get; set; }
}