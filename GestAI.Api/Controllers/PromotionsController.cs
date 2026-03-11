using GestAI.Application.Promotions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class PromotionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, [FromQuery] bool includeDeleted = false, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetPromotionsQuery(propertyId, includeDeleted), ct));

    [HttpPost]
    public async Task<IActionResult> Create(int propertyId, [FromBody] UpsertPromotionCommand command, CancellationToken ct = default)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, PromotionId = null }, ct));

    [HttpPut("{promotionId:int}")]
    public async Task<IActionResult> Update(int propertyId, int promotionId, [FromBody] UpsertPromotionCommand command, CancellationToken ct = default)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, PromotionId = promotionId }, ct));

    [HttpPut("{promotionId:int}/status")]
    public async Task<IActionResult> Toggle(int propertyId, int promotionId, [FromBody] TogglePromotionStatusCommand command, CancellationToken ct = default)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, PromotionId = promotionId }, ct));

    [HttpDelete("{promotionId:int}")]
    public async Task<IActionResult> Delete(int propertyId, int promotionId, CancellationToken ct = default)
        => Ok(await mediator.Send(new DeletePromotionCommand(propertyId, promotionId), ct));
}
