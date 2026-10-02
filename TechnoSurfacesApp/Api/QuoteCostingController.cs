using Microsoft.AspNetCore.Mvc;
using TechnoSurfaces.Application.Costing;
using TechnoSurfaces.Application.Quoting;

namespace TechnoSurfacesApp.Api;

/// <summary>
/// The costing sheet of a quote: its priced lines, the markup and transport on the
/// open version, and the totals. Called by the costing sheet screen.
///
/// Every error is a ProblemDetails body with the matching status code, never 200
/// with a failure flag. A price that cannot be resolved is 422 with the reason, and
/// no line is created (NFR-01, US-03). Write calls need the antiforgery token in a
/// RequestVerificationToken header.
///
/// The rule that an estimator edits only their own draft belongs to the
/// authorisation handler and is applied here once that handler is on develop.
/// </summary>
[ApiController]
[Route("api/quotes/{quoteId:int}")]
[Produces("application/json")]
public sealed class QuoteCostingController : ControllerBase
{
    private readonly ICostingSheetService _costing;

    public QuoteCostingController(ICostingSheetService costing) => _costing = costing;

    /// <summary>GET /api/quotes/{quoteId}/costing: the current version, its lines and totals.</summary>
    [HttpGet("costing")]
    [ProducesResponseType<CostingSheetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Costing(int quoteId, CancellationToken ct)
    {
        var result = await _costing.GetAsync(quoteId, ct);
        return result.Outcome == CostingOutcome.Ok
            ? Ok(CostingSheetDto.From(result.Quote!, result.Totals!))
            : Failure(result, quoteId);
    }

    /// <summary>PUT /api/quotes/{quoteId}/costing: markup (US-07) and the transport amount.</summary>
    [HttpPut("costing")]
    [ProducesResponseType<QuoteTotals>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeCosting(int quoteId, ChangeCostingRequest request, CancellationToken ct)
    {
        var result = await _costing.ChangeCostingAsync(quoteId,
            new ChangeCosting(request.MarkupPercent, request.TransportAmount), ct);
        return result.Outcome == CostingOutcome.Ok ? Ok(result.Totals) : Failure(result, quoteId);
    }

    /// <summary>GET /api/quotes/{quoteId}/totals: a full recalculation (US-09).</summary>
    [HttpGet("totals")]
    [ProducesResponseType<QuoteTotals>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Totals(int quoteId, CancellationToken ct)
    {
        var result = await _costing.GetAsync(quoteId, ct);
        return result.Outcome == CostingOutcome.Ok ? Ok(result.Totals) : Failure(result, quoteId);
    }

    /// <summary>GET /api/quotes/{quoteId}/lines/{lineId}</summary>
    [HttpGet("lines/{lineId:int}", Name = nameof(GetLine))]
    [ProducesResponseType<LineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLine(int quoteId, int lineId, CancellationToken ct)
    {
        var result = await _costing.GetLineAsync(quoteId, lineId, ct);
        return result.Outcome == CostingOutcome.Ok
            ? Ok(new LineResponse(CostingLineDto.From(result.Line!), result.Totals!))
            : Failure(result, quoteId);
    }

    /// <summary>POST /api/quotes/{quoteId}/lines (US-01, US-03, US-04).</summary>
    [HttpPost("lines")]
    [ProducesResponseType<LineResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddLine(int quoteId, AddLineRequest request, CancellationToken ct)
    {
        var result = request.Type == AddLineRequest.Material
            ? await _costing.AddMaterialLineAsync(quoteId,
                new AddMaterialLine(request.ColourId!.Value, request.SheetSizeId!.Value, request.Quantity, request.SupplierDiscountPercent), ct)
            : await _costing.AddRateLineAsync(quoteId,
                new AddRateLine(request.RateItemId!.Value, request.Quantity, request.UnitPrice), ct);

        if (result.Outcome != CostingOutcome.Ok)
            return Failure(result, quoteId);

        return CreatedAtRoute(nameof(GetLine), new { quoteId, lineId = result.Line!.Id },
            new LineResponse(CostingLineDto.From(result.Line), result.Totals!));
    }

    /// <summary>PUT /api/quotes/{quoteId}/lines/{lineId}: quantity, rate or discount on this quote only (US-04, US-06, US-08).</summary>
    [HttpPut("lines/{lineId:int}")]
    [ProducesResponseType<LineResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeLine(int quoteId, int lineId, ChangeLineRequest request, CancellationToken ct)
    {
        var result = await _costing.ChangeLineAsync(quoteId, lineId, new ChangeLine(
            request.Quantity, request.UnitPrice, request.ClearPriceOverride,
            request.SupplierDiscountPercent, request.RestoreDerivedQuantity), ct);

        return result.Outcome == CostingOutcome.Ok
            ? Ok(new LineResponse(CostingLineDto.From(result.Line!), result.Totals!))
            : Failure(result, quoteId);
    }

    /// <summary>DELETE /api/quotes/{quoteId}/lines/{lineId}</summary>
    [HttpDelete("lines/{lineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveLine(int quoteId, int lineId, CancellationToken ct)
    {
        var result = await _costing.RemoveLineAsync(quoteId, lineId, ct);
        return result.Outcome == CostingOutcome.Ok ? NoContent() : Failure(result, quoteId);
    }

    private IActionResult Failure(CostingResult result, int quoteId) => result.Outcome switch
    {
        CostingOutcome.QuoteNotFound => Problem(
            statusCode: StatusCodes.Status404NotFound, title: "Quote not found",
            detail: $"There is no quote {quoteId}."),

        CostingOutcome.LineNotFound => Problem(
            statusCode: StatusCodes.Status404NotFound, title: "Line not found",
            detail: $"Quote {quoteId} has no such line on its current version."),

        CostingOutcome.PriceNotResolved => Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity, title: "The price could not be resolved",
            detail: result.Problem),

        CostingOutcome.NotSelectable => Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity, title: "This item cannot be quoted",
            detail: result.Problem),

        CostingOutcome.NotApplicable => Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity, title: "This change does not apply to the line",
            detail: result.Problem),

        CostingOutcome.VersionSealed => Problem(
            statusCode: StatusCodes.Status409Conflict, title: "This version can no longer be changed",
            detail: result.Problem),

        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}
