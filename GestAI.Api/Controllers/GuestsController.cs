using GestAI.Application.Guests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class GuestsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, [FromQuery] string? search, CancellationToken ct)
        => Ok(await mediator.Send(new GetGuestsQuery(propertyId, search), ct));

    [HttpGet("search")]
    public async Task<IActionResult> Search(int propertyId, [FromQuery] string q, CancellationToken ct)
        => Ok(await mediator.Send(new SearchGuestsQuery(propertyId, q), ct));

    [HttpPost]
    public async Task<IActionResult> Upsert(int propertyId, [FromBody] UpsertGuestCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    [HttpDelete("{guestId:int}")]
    public async Task<IActionResult> Delete(int propertyId, int guestId, CancellationToken ct)
        => Ok(await mediator.Send(new DeleteGuestCommand(propertyId, guestId), ct));
}
