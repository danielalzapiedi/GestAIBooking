using GestAI.Application.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int propertyId, CancellationToken ct) => Ok(await mediator.Send(new GetDashboardSummaryQuery(propertyId), ct));
}
