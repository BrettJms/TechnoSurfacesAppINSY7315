using TechnoSurfaces.Domain;

namespace TechnoSurfaces.Application.Pricing;

/// <summary>
/// Selects the pricing strategy for the supplier behind a colour and delegates to
/// it. This is the context in the Strategy pattern: the caller asks for a price and
/// does not know or care which scheme the supplier uses.
///
/// Adding a sixth supplier on a scheme we have not seen means adding a strategy and
/// registering it, not editing this class.
/// </summary>
public interface IPriceResolver
{
    Task<PriceResolution> ResolveAsync(PriceKey key, DateOnly asAt, CancellationToken ct = default);
}

public sealed class PriceResolver : IPriceResolver
{
    private readonly ICatalogueReader _catalogue;
    private readonly IReadOnlyDictionary<PricingStructure, IPriceResolutionStrategy> _strategies;

    public PriceResolver(ICatalogueReader catalogue, IEnumerable<IPriceResolutionStrategy> strategies)
    {
        _catalogue = catalogue;
        _strategies = strategies.ToDictionary(s => s.Handles);
    }

    public async Task<PriceResolution> ResolveAsync(PriceKey key, DateOnly asAt, CancellationToken ct = default)
    {
        var supplier = await _catalogue.GetSupplierForColourAsync(key.ColourId, ct);
        if (supplier is null)
            return PriceResolution.Failure($"Colour {key.ColourId} is not linked to a supplier.");

        if (!_strategies.TryGetValue(supplier.PricingStructure, out var strategy))
            return PriceResolution.Failure(
                $"No pricing strategy is registered for {supplier.Name}, which prices by {supplier.PricingStructure}.");

        return await strategy.ResolveAsync(key, asAt, ct);
    }
}
