using GestAI.Application.Rates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class RatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, CancellationToken ct) => Ok(await mediator.Send(new GetRatesQuery(propertyId), ct));

    [HttpPost]
    public async Task<IActionResult> Create(int propertyId, [FromBody] UpsertRatePlanCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId, RatePlanId = null }, ct));

    [HttpPut("{ratePlanId:int}")]
    public async Task<IActionResult> Update(int propertyId, int ratePlanId, [FromBody] UpsertRatePlanCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId, RatePlanId = ratePlanId }, ct));
}
