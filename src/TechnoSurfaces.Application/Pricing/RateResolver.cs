using System.Globalization;

namespace TechnoSurfaces.Application.Pricing;

/// <summary>
/// Resolves the amount charged for a rate item, as at a date.
///
/// Two rules apply, in order. A rate may vary by supplier, because adhesive is R130
/// from two suppliers, R250 from a third and R299 from Max on Top when used on
/// another supplier's material; a supplier-specific amount therefore wins over the
/// general one.
///
/// A rate may also be derived from another rate rather than maintained separately.
/// Overtime is fabrication multiplied by 1.5 and installation mirrors fabrication.
/// Holding those as relationships is the architectural answer to the spreadsheet's
/// core weakness, where a rate card copied into twelve sheets drifted apart and a
/// price change had to be made twelve times.
/// </summary>
public interface IRateResolver
{
    Task<PriceResolution> ResolveAsync(int rateItemId, int? supplierId, DateOnly asAt, CancellationToken ct = default);
}

public sealed class RateResolver : IRateResolver
{
    /// <summary>
    /// Guards against a rate card that has been configured to derive from itself,
    /// directly or in a ring. Without this, a cycle would recurse until the stack
    /// gave out.
    /// </summary>
    private const int MaxDerivationDepth = 5;

    private readonly ICatalogueReader _catalogue;

    public RateResolver(ICatalogueReader catalogue) => _catalogue = catalogue;

    public Task<PriceResolution> ResolveAsync(int rateItemId, int? supplierId, DateOnly asAt, CancellationToken ct = default) =>
        ResolveAsync(rateItemId, supplierId, asAt, depth: 0, ct);

    private async Task<PriceResolution> ResolveAsync(int rateItemId, int? supplierId, DateOnly asAt, int depth, CancellationToken ct)
    {
        if (depth > MaxDerivationDepth)
            return PriceResolution.Failure(
                $"Rate item {rateItemId} derives from itself. Correct the rate card before quoting.");

        var item = await _catalogue.GetRateItemAsync(rateItemId, ct);
        if (item is null)
            return PriceResolution.Failure($"Rate item {rateItemId} is not on the rate card.");

        // A rate of its own takes precedence over a derived one.
        var own = await _catalogue.FindRatePriceAsync(rateItemId, supplierId, asAt, ct);
        if (own is not null)
        {
            var scope = own.SupplierId is null ? "all suppliers" : $"supplier {own.SupplierId}";
            return PriceResolution.Success(
                own.Amount,
                $"Rate card: {item.Name}, {scope}, effective {own.EffectiveFrom:yyyy-MM-dd}");
        }

        if (item.DerivedFromRateItemId is null)
            return PriceResolution.Failure(
                $"No rate is in force on {asAt:yyyy-MM-dd} for {item.Name}. " +
                "The Managing Director sets this on the rate card screen.");

        var multiplier = item.DerivedFromRateItemMultiplier ?? 1m;
        var parent = await ResolveAsync(item.DerivedFromRateItemId.Value, supplierId, asAt, depth + 1, ct);

        if (!parent.Resolved)
            return PriceResolution.Failure(
                $"{item.Name} is derived from {item.DerivedFromRateItem?.Name ?? "another rate"}, " +
                $"which could not be resolved: {parent.FailureReason}");

        var amount = decimal.Round(parent.UnitPrice * multiplier, 2, MidpointRounding.AwayFromZero);
        var parentName = item.DerivedFromRateItem?.Name ?? "the base rate";

        if (multiplier == 1m)
            return PriceResolution.Success(amount, $"Rate card: {item.Name} mirrors {parentName}. {parent.Origin}");

        // Formatted with the invariant culture on purpose. The origin is persisted
        // onto the costing line, so it must not vary with the locale of whichever
        // machine resolved the price: en-ZA would write "1,5" and the App Service
        // default would write "1.5" for the same rate. Trailing zeros are stripped
        // because a decimal loaded from the database keeps its scale.
        var factor = multiplier.ToString("0.##", CultureInfo.InvariantCulture);

        return PriceResolution.Success(
            amount,
            $"Rate card: {item.Name} is {parentName} x {factor}. {parent.Origin}");
    }
}
