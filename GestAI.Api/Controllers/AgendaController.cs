using GestAI.Application.Agenda;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class AgendaController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily")]
    public async Task<IActionResult> Daily(int propertyId, [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await mediator.Send(new GetDailyAgendaQuery(propertyId, date), ct));
}
