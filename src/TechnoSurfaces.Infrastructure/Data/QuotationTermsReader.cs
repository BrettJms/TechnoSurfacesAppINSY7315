using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Quoting;

namespace TechnoSurfaces.Infrastructure.Data;

public sealed class QuotationTermsReader : IQuotationTermsReader
{
    private readonly TechnoSurfacesDbContext _db;

    public QuotationTermsReader(TechnoSurfacesDbContext db) => _db = db;

    public async Task<IReadOnlyList<StandingTerm>> GetStandingTermsAsync(CancellationToken ct = default) =>
        await _db.QuotationTerms.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Section).ThenBy(t => t.SortOrder)
            .Select(t => new StandingTerm(t.Section, t.Text))
            .ToListAsync(ct);

    public async Task<BrandWarranty?> GetWarrantyForProductLineAsync(int productLineId, CancellationToken ct = default)
    {
        var brand = await _db.ProductLines.AsNoTracking()
            .Where(p => p.Id == productLineId)
            .Select(p => p.Brand)
            .FirstOrDefaultAsync(ct);

        if (brand is null || brand.MaterialWarranty is null || brand.WorkmanshipWarranty is null)
            return null;

        return new BrandWarranty(brand.Name, brand.MaterialWarranty, brand.WorkmanshipWarranty);
    }
}
