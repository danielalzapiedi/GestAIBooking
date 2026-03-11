using GestAI.Application.Units;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class UnitsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, CancellationToken ct) => Ok(await mediator.Send(new GetUnitsQuery(propertyId), ct));

    [HttpGet("{unitId:int}")]
    public async Task<IActionResult> Get(int propertyId, int unitId, CancellationToken ct) => Ok(await mediator.Send(new GetUnitQuery(propertyId, unitId), ct));

    [HttpPost]
    public async Task<IActionResult> Create(int propertyId, [FromBody] UpsertUnitCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId, UnitId = null }, ct));

    [HttpPut("{unitId:int}")]
    public async Task<IActionResult> Update(int propertyId, int unitId, [FromBody] UpsertUnitCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId, UnitId = unitId }, ct));

    public sealed record SetStatusBody(UnitOperationalStatus Status);
    [HttpPost("{unitId:int}/housekeeping-status")]
    public async Task<IActionResult> SetStatus(int propertyId, int unitId, [FromBody] SetStatusBody body, CancellationToken ct) => Ok(await mediator.Send(new SetUnitOperationalStatusCommand(propertyId, unitId, body.Status), ct));
}
