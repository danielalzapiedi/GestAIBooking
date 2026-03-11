using GestAI.Application.ExternalCalendars;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/public/unit-calendars")]
[AllowAnonymous]
public sealed class PublicCalendarsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{unitId:int}/ics")]
    public async Task<IActionResult> Export(int unitId, [FromQuery] string token, CancellationToken ct)
    {
        var result = await mediator.Send(new ExportUnitCalendarQuery(unitId, token), ct);
        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return File(Encoding.UTF8.GetBytes(result.Content), "text/calendar; charset=utf-8", result.FileName);
    }
}
