namespace TechnoSurfaces.Domain.People;

/// <summary>
/// A person at the customer organisation who requests and receives quotations.
/// Modelled separately from <see cref="Customer"/> because, as the client stated,
/// some customers have four or five different estimators who send through requests.
/// A quotation is addressed to a contact and billed to a customer.
///
/// Holds personal information, so access is restricted by role and the data is
/// hosted in the South Africa North region. See NFR-09.
/// </summary>
public class Contact
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;
}
