using Microsoft.EntityFrameworkCore;
using TechnoSurfaces.Application.Auditing;
using TechnoSurfaces.Application.Catalogue;
using TechnoSurfaces.Application.Pricing;
using TechnoSurfaces.Domain;
using TechnoSurfaces.Domain.Catalogue;

namespace TechnoSurfaces.Infrastructure.Data;

/// <summary>
/// Managing Director only - enforced by the CanEditCatalogue policy on every
/// action that writes through this service.
/// </summary>
public sealed class CatalogueService : ICatalogueService
{
    private readonly TechnoSurfacesDbContext _db;
    private readonly IPriceHistory _prices;
    private readonly ICurrentUser _currentUser;

    public CatalogueService(TechnoSurfacesDbContext db, IPriceHistory prices, ICurrentUser currentUser)
    {
        _db = db;
        _prices = prices;
        _currentUser = currentUser;
    }

    // ------------------------------------------------------------ reads

    public async Task<IReadOnlyList<CatalogueRow>> GetCatalogueAsync(DateOnly asAt, CancellationToken ct = default)
    {
        var colours = await _db.Colours.AsNoTracking()
            .Include(c => c.ProductLine!).ThenInclude(l => l.Supplier)
            .Include(c => c.ProductLine!).ThenInclude(l => l.SheetSizes)
            .Include(c => c.PriceBand)
            .ToListAsync(ct);

        var inForce = await _db.MaterialPrices.AsNoTracking()
            .Where(p => p.EffectiveFrom <= asAt && (p.EffectiveTo == null || p.EffectiveTo >= asAt))
            .ToListAsync(ct);

        var rows = new List<CatalogueRow>();
        foreach (var colour in colours)
        {
            var line = colour.ProductLine!;
            var byBand = line.Supplier!.PricingStructure == PricingStructure.Band;

            foreach (var size in line.SheetSizes)
            {
                var price = inForce.FirstOrDefault(p => p.SheetSizeId == size.Id &&
                    (byBand ? p.PriceBandId == colour.PriceBandId : p.ColourId == colour.Id));

                rows.Add(new CatalogueRow(
                    colour.Id, byBand ? colour.PriceBandId : null, size.Id,
                    line.Supplier.Name, line.Name, line.ThicknessMm,
                    colour.Name, colour.SupplierCode, colour.PriceBand?.Name, size.ToString(),
                    colour.Status == CatalogueStatus.Discontinued || line.Status == CatalogueStatus.Discontinued,
                    price?.PricePerSqm, price?.PricePerSheet(size), price?.EffectiveFrom));
            }
        }

        return rows
            .OrderBy(r => r.Supplier).ThenBy(r => r.ProductLine).ThenBy(r => r.Colour).ThenBy(r => r.SheetSize)
            .ToList();
    }

    public async Task<IReadOnlyList<PricePeriodRow>> GetPriceHistoryAsync(
        int? colourId, int? priceBandId, int sheetSizeId, CancellationToken ct = default) =>
        await _db.MaterialPrices.AsNoTracking()
            .Where(p => p.SheetSizeId == sheetSizeId && p.ColourId == colourId && p.PriceBandId == priceBandId)
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new PricePeriodRow(p.PricePerSqm, p.EffectiveFrom, p.EffectiveTo, p.CapturedByUserId, p.CapturedAtUtc))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RateCardRow>> GetRateCardAsync(DateOnly asAt, CancellationToken ct = default)
    {
        var items = await _db.RateItems.AsNoTracking().Include(r => r.DerivedFromRateItem).ToListAsync(ct);
        var suppliers = await _db.Suppliers.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name, ct);
        var inForce = await _db.RatePrices.AsNoTracking()
            .Where(p => p.EffectiveFrom <= asAt && (p.EffectiveTo == null || p.EffectiveTo >= asAt))
            .ToListAsync(ct);

        var rows = new List<RateCardRow>();
        foreach (var item in items)
        {
            var retired = item.Status == CatalogueStatus.Discontinued;
            var derivedFrom = item.DerivedFromRateItem?.Name;
            var prices = inForce.Where(p => p.RateItemId == item.Id).ToList();

            if (prices.Count == 0)
            {
                rows.Add(new RateCardRow(item.Id, item.Name, item.Category.ToString(), item.Unit.ToString(),
                    null, null, null, null, derivedFrom, item.DerivedFromRateItemMultiplier, retired));
                continue;
            }

            foreach (var p in prices)
                rows.Add(new RateCardRow(item.Id, item.Name, item.Category.ToString(), item.Unit.ToString(),
                    p.SupplierId, p.SupplierId is int s ? suppliers.GetValueOrDefault(s) : null,
                    p.Amount, p.EffectiveFrom, derivedFrom, item.DerivedFromRateItemMultiplier, retired));
        }

        return rows.OrderBy(r => r.Category).ThenBy(r => r.Name).ToList();
    }

    // ------------------------------------------------------------ writes

    public async Task<CatalogueResult> SetMaterialPriceAsync(
        int? colourId, int? priceBandId, int sheetSizeId, decimal pricePerSqm, DateOnly from, CancellationToken ct = default)
    {
        if (colourId is int id)
        {
            var colour = await _db.Colours.AsNoTracking().Include(c => c.ProductLine)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
            if (colour is null)
                return CatalogueResult.Fail("That colour is not in the catalogue.");
            if (colour.Status == CatalogueStatus.Discontinued || colour.ProductLine?.Status == CatalogueStatus.Discontinued)
                return CatalogueResult.Fail($"{colour.Name} has been retired and cannot be given a new price.");
        }

        return await AttemptAsync(() => _prices.SetMaterialPriceAsync(
            colourId, priceBandId, sheetSizeId, pricePerSqm, from, _currentUser.UserId, ct));
    }

    public Task<CatalogueResult> SetRateAsync(
        int rateItemId, int? supplierId, decimal amount, DateOnly from, CancellationToken ct = default) =>
        AttemptAsync(() => _prices.SetRateAsync(rateItemId, supplierId, amount, from, ct));

    public async Task<CatalogueResult> RetireColourAsync(int colourId, CancellationToken ct = default)
    {
        var colour = await _db.Colours.FindAsync(new object[] { colourId }, ct);
        if (colour is null)
            return CatalogueResult.Fail("That colour is not in the catalogue.");

        // US-24: retire, never delete. Existing quotes keep their copied price and
        // still show the colour; it can no longer be chosen on a new quote.
        colour.Status = CatalogueStatus.Discontinued;
        await _db.SaveChangesAsync(ct);
        return CatalogueResult.Ok();
    }

    public async Task<CatalogueResult> RetireProductLineAsync(int productLineId, CancellationToken ct = default)
    {
        var line = await _db.ProductLines.FindAsync(new object[] { productLineId }, ct);
        if (line is null)
            return CatalogueResult.Fail("That product line is not in the catalogue.");

        line.Status = CatalogueStatus.Discontinued;
        await _db.SaveChangesAsync(ct);
        return CatalogueResult.Ok();
    }

    /// <summary>
    /// IPriceHistory signals a broken price rule by throwing. The MD gets the rule's
    /// own message rather than an error page; the database triggers are the last line.
    /// </summary>
    private static async Task<CatalogueResult> AttemptAsync(Func<Task> change)
    {
        try
        {
            await change();
            return CatalogueResult.Ok();
        }
        catch (ArgumentException e)
        {
            var message = e.ParamName is null ? e.Message : e.Message.Replace($" (Parameter '{e.ParamName}')", "");
            return CatalogueResult.Fail(message);
        }
        catch (InvalidOperationException e)
        {
            return CatalogueResult.Fail(e.Message);
        }
        catch (DbUpdateException)
        {
            return CatalogueResult.Fail(
                "The database refused this price because it would overlap another period or not be greater than zero.");
        }
    }
}