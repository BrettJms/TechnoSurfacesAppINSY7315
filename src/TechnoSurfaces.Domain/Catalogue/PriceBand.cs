namespace TechnoSurfaces.Domain.Catalogue;

/// <summary>
/// A supplier's own grouping of colours that share a price: the Colour Category
/// column on the Staron list, or groups A1 to A4 and M1 to M4 on the Surface Studio
/// list.
///
/// Optional, because only three of the five suppliers use bands. A band is never
/// the sole holder of a price on its own account; the price still belongs to a
/// <see cref="MaterialPrice"/> row, which points either here or at a colour.
/// </summary>
public class PriceBand
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int ProductLineId { get; set; }
    public ProductLine? ProductLine { get; set; }

    /// <summary>The supplier's own band code, for example A1, M4 or Supreme.</summary>
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public ICollection<Colour> Colours { get; set; } = new List<Colour>();
}
