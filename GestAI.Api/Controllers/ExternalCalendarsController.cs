using GestAI.Application.ExternalCalendars;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/external-calendars")]
[Authorize]
public sealed class ExternalCalendarsController(IMediator mediator) : ControllerBase
{
    [HttpGet("connections")]
    public async Task<IActionResult> Connections(int propertyId, [FromQuery] int? unitId, CancellationToken ct)
        => Ok(await mediator.Send(new GetExternalCalendarConnectionsQuery(propertyId, unitId), ct));

    [HttpPost("connections")]
    public async Task<IActionResult> Upsert(int propertyId, [FromBody] UpsertExternalCalendarConnectionCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    [HttpDelete("connections/{connectionId:int}")]
    public async Task<IActionResult> Delete(int propertyId, int connectionId, CancellationToken ct)
        => Ok(await mediator.Send(new DeleteExternalCalendarConnectionCommand(propertyId, connectionId), ct));

    [HttpPost("connections/{connectionId:int}/sync")]
    public async Task<IActionResult> Sync(int propertyId, int connectionId, CancellationToken ct)
        => Ok(await mediator.Send(new SyncExternalCalendarConnectionCommand(propertyId, connectionId), ct));

    [HttpGet("events")]
    public async Task<IActionResult> Events(int propertyId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int? unitId, CancellationToken ct)
        => Ok(await mediator.Send(new GetExternalCalendarEventsByRangeQuery(propertyId, from, to, unitId), ct));

    [HttpGet("logs")]
    public async Task<IActionResult> Logs(int propertyId, [FromQuery] int? connectionId, CancellationToken ct)
        => Ok(await mediator.Send(new GetExternalSyncLogsQuery(propertyId, connectionId), ct));
}
