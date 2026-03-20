using GestAI.Application.Templates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class TemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, CancellationToken ct) => Ok(await mediator.Send(new GetTemplatesQuery(propertyId), ct));
    [HttpPost]
    public async Task<IActionResult> Upsert(int propertyId, [FromBody] UpsertTemplateCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    public sealed record ToggleStatusBody(bool IsActive);

    [HttpPost("{templateId:int}/status")]
    public async Task<IActionResult> ToggleStatus(int propertyId, int templateId, [FromBody] ToggleStatusBody body, CancellationToken ct)
        => Ok(await mediator.Send(new ToggleTemplateStatusCommand(propertyId, templateId, body.IsActive), ct));
}
