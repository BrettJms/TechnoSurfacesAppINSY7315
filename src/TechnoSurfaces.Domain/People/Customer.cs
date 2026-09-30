namespace TechnoSurfaces.Domain.People;

/// <summary>
/// The organisation or individual being quoted. Carries the account code used in
/// the client's accounting system so that records in the two systems can be
/// reconciled.
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    /// <summary>The account code in Sage Pastel.</summary>
    public string? AccountCode { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    public string? VatNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
