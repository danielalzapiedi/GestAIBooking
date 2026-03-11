using GestAI.Application.BlockedDates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class BlockedDatesController(IMediator mediator) : ControllerBase
{
    [HttpGet("range")]
    public async Task<IActionResult> ByRange(int propertyId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await mediator.Send(new GetBlockedDatesByRangeQuery(propertyId, from, to), ct));

    [HttpPost]
    public async Task<IActionResult> Create(int propertyId, [FromBody] CreateBlockedDateCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    [HttpDelete("{blockedDateId:int}")]
    public async Task<IActionResult> Delete(int propertyId, int blockedDateId, CancellationToken ct)
        => Ok(await mediator.Send(new DeleteBlockedDateCommand(propertyId, blockedDateId), ct));
}
