namespace TechnoSurfaces.Application.Catalogue;

/// <summary>The outcome of a catalogue change, with a message the MD can act on.</summary>
public sealed record CatalogueResult(bool Succeeded, string? Error = null)
{
    public static CatalogueResult Ok() => new(true);
    public static CatalogueResult Fail(string error) => new(false, error);
}

/// <summary>One colour in one sheet size, with the price in force on the date asked for.</summary>
public sealed record CatalogueRow(
    int ColourId, int? PriceBandId, int SheetSizeId,
    string Supplier, string ProductLine, int ThicknessMm,
    string Colour, string SupplierCode, string? Band, string SheetSize,
    bool IsRetired, decimal? PricePerSqm, decimal? PricePerSheet, DateOnly? PriceFrom);

/// <summary>One period in a price's history. Prices are superseded, never overwritten.</summary>
public sealed record PricePeriodRow(
    decimal PricePerSqm, DateOnly From, DateOnly? To, string? CapturedByUserId, DateTime CapturedAtUtc);

/// <summary>One rate on the rate card. Derived rates show their rule instead of a number.</summary>
public sealed record RateCardRow(
    int RateItemId, string Name, string Category, string Unit,
    int? SupplierId, string? Supplier, decimal? Amount, DateOnly? From,
    string? DerivedFrom, decimal? Multiplier, bool IsRetired)
{
    public bool IsDerived => DerivedFrom is not null;

    /// <summary>No price captured yet - the client has not supplied the figure.</summary>
    public bool AwaitingClientFigure => !IsDerived && Amount is null;
}

/// <summary>
/// Catalogue and rate-card maintenance for the Managing Director (US-23, US-24, NFR-10).
/// Price rules live in IPriceHistory; this service adds the screens' reads, retirement,
/// and messages the MD can act on. Every change is recorded by the audit interceptor.
/// </summary>
public interface ICatalogueService
{
    Task<IReadOnlyList<CatalogueRow>> GetCatalogueAsync(DateOnly asAt, CancellationToken ct = default);

    Task<IReadOnlyList<PricePeriodRow>> GetPriceHistoryAsync(
        int? colourId, int? priceBandId, int sheetSizeId, CancellationToken ct = default);

    Task<IReadOnlyList<RateCardRow>> GetRateCardAsync(DateOnly asAt, CancellationToken ct = default);

    Task<CatalogueResult> SetMaterialPriceAsync(
        int? colourId, int? priceBandId, int sheetSizeId, decimal pricePerSqm, DateOnly from, CancellationToken ct = default);

    Task<CatalogueResult> SetRateAsync(
        int rateItemId, int? supplierId, decimal amount, DateOnly from, CancellationToken ct = default);

    Task<CatalogueResult> RetireColourAsync(int colourId, CancellationToken ct = default);

    Task<CatalogueResult> RetireProductLineAsync(int productLineId, CancellationToken ct = default);
}