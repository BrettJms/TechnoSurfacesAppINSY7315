using TechnoSurfaces.Domain;

namespace TechnoSurfaces.Application.Pricing;

/// <summary>
/// Identifies the material being priced. The natural key that works for every
/// supplier is (Supplier, ProductLine, Colour, SheetWidth, Thickness); thickness is
/// carried by the product line and the sheet size carries the dimensions, so the
/// key reduces to a colour and a sheet size.
/// </summary>
/// <param name="ColourId">The colour chosen by the estimator.</param>
/// <param name="SheetSizeId">The sheet size chosen by the estimator.</param>
public readonly record struct PriceKey(int ColourId, int SheetSizeId);

/// <summary>
/// Resolves the sheet price for a material, as at a date.
///
/// This is the Strategy pattern from the design document. Three of the five
/// suppliers price by colour band and two price each colour individually; neither
/// is the general case, so there is one strategy per pricing scheme rather than a
/// branch inside a single resolver. <see cref="PriceResolverFactory"/> selects the
/// strategy from the supplier.
/// </summary>
public interface IPriceResolutionStrategy
{
    /// <summary>The scheme this strategy handles.</summary>
    PricingStructure Handles { get; }

    /// <summary>
    /// Resolves the price in force on <paramref name="asAt"/>. Returns a failure
    /// rather than zero when no price applies.
    /// </summary>
    Task<PriceResolution> ResolveAsync(PriceKey key, DateOnly asAt, CancellationToken ct = default);
}
