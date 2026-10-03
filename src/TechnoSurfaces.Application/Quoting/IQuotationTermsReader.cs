using TechnoSurfaces.Domain;

namespace TechnoSurfaces.Application.Quoting;

/// <summary>One line of standing wording, as printed on the quotation.</summary>
public sealed record StandingTerm(TermSection Section, string Text);

/// <summary>The warranty for the brand quoted, as printed on the quotation.</summary>
public sealed record BrandWarranty(string Brand, string MaterialWarranty, string WorkmanshipWarranty);

/// <summary>
/// The standing content of a customer quotation: the terms, lead times,
/// exclusions, disclaimers and payment terms applied to every quotation (US-12),
/// and the warranty that follows the brand quoted (US-13).
///
/// Declared here so the quotation can be composed without depending on
/// persistence; implemented in the Infrastructure layer.
/// </summary>
public interface IQuotationTermsReader
{
    /// <summary>Every active line, by section and then in template order.</summary>
    Task<IReadOnlyList<StandingTerm>> GetStandingTermsAsync(CancellationToken ct = default);

    /// <summary>
    /// The warranty for the brand of a product line, or null when the product line
    /// has no brand or its brand's warranty has not been confirmed by the client.
    /// The caller prints no warranty in that case; it never substitutes another
    /// brand's wording.
    /// </summary>
    Task<BrandWarranty?> GetWarrantyForProductLineAsync(int productLineId, CancellationToken ct = default);
}
