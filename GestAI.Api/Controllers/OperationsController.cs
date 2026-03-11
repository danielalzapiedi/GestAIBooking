using GestAI.Application.Operations;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/operations")]
[Authorize]
public sealed class OperationsController(IMediator mediator) : ControllerBase
{
    [HttpGet("tasks")]
    public async Task<IActionResult> List(int propertyId, [FromQuery] OperationalTaskStatus? status, [FromQuery] OperationalTaskType? type, CancellationToken ct)
        => Ok(await mediator.Send(new GetOperationalTasksQuery(propertyId, status, type), ct));

    [HttpPost("tasks/{taskId:int}/complete")]
    public async Task<IActionResult> Complete(int propertyId, int taskId, CancellationToken ct)
        => Ok(await mediator.Send(new CompleteOperationalTaskCommand(propertyId, taskId), ct));
}
