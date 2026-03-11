using GestAI.Application.Properties;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PropertiesController(IMediator mediator) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await mediator.Send(new GetMyPropertiesQuery(), ct));

    [HttpGet("{propertyId:int}")]
    public async Task<IActionResult> Detail(int propertyId, CancellationToken ct) => Ok(await mediator.Send(new GetPropertyDetailQuery(propertyId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertPropertyCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = null }, ct));

    [HttpPut("{propertyId:int}")]
    public async Task<IActionResult> Upsert(int propertyId, [FromBody] UpsertPropertyCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));
}
