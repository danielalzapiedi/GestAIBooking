using GestAI.Application.Quotes;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class QuotesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int propertyId, [FromQuery] int? unitId, [FromQuery] DateOnly checkInDate, [FromQuery] DateOnly checkOutDate, [FromQuery] int adults = 2, [FromQuery] int children = 0, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetQuoteQuery(propertyId, unitId, checkInDate, checkOutDate, adults, children), ct));

    [HttpPost("save")]
    public async Task<IActionResult> Save(int propertyId, [FromBody] SaveQuoteCommand command, CancellationToken ct = default)
        => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    [HttpGet("saved")]
    public async Task<IActionResult> Saved(int propertyId, [FromQuery] string? search, [FromQuery] SavedQuoteStatus? status, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetSavedQuotesQuery(propertyId, search, status), ct));

    [HttpGet("saved/{savedQuoteId:int}")]
    public async Task<IActionResult> SavedDetail(int propertyId, int savedQuoteId, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetSavedQuoteDetailQuery(propertyId, savedQuoteId), ct));

    [HttpPost("saved/{savedQuoteId:int}/convert")]
    public async Task<IActionResult> Convert(int propertyId, int savedQuoteId, [FromBody] ConvertSavedQuoteToBookingCommand command, CancellationToken ct = default)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, SavedQuoteId = savedQuoteId }, ct));

    [HttpGet("simulate")]
    public async Task<IActionResult> Simulate(int propertyId, [FromQuery] int unitId, [FromQuery] DateOnly checkInDate, [FromQuery] DateOnly checkOutDate, [FromQuery] int adults = 2, [FromQuery] int children = 0, CancellationToken ct = default)
        => Ok(await mediator.Send(new PricingSimulationQuery(propertyId, unitId, checkInDate, checkOutDate, adults, children), ct));
}
