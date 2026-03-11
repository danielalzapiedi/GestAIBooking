using GestAI.Application.Setup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SetupController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
        => Ok(await mediator.Send(new GetSetupStatusQuery(), ct));
}
