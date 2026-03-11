using GestAI.Application.Properties;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    public sealed record SetDefaultPropertyBody(int PropertyId);

    [HttpPost("default-property")]
    public async Task<IActionResult> SetDefaultProperty([FromBody] SetDefaultPropertyBody body, CancellationToken ct)
        => Ok(await mediator.Send(new SetDefaultPropertyCommand(body.PropertyId), ct));
}
