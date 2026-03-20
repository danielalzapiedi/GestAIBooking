using GestAI.Application.Saas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SaasController(IMediator mediator) : ControllerBase
{
    [HttpGet("me/access")]
    public async Task<IActionResult> GetAccess(CancellationToken ct) => Ok(await mediator.Send(new GetCurrentUserAccessQuery(), ct));

    [HttpGet("account/summary")]
    public async Task<IActionResult> GetAccountSummary(CancellationToken ct) => Ok(await mediator.Send(new GetAccountSummaryQuery(), ct));

    [HttpGet("account/plans")]
    public async Task<IActionResult> GetAccountPlans(CancellationToken ct) => Ok(await mediator.Send(new GetAccountPlanOptionsQuery(), ct));

    [HttpPut("account")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountCommand command, CancellationToken ct) => Ok(await mediator.Send(command, ct));

    [HttpGet("account/audit")]
    public async Task<IActionResult> GetAudit([FromQuery] int take = 30, [FromQuery] string? entityName = null, [FromQuery] string? userName = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetAccountAuditQuery(take, entityName, userName, from, to), ct));

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct) => Ok(await mediator.Send(new GetAccountUsersQuery(), ct));

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UpsertAccountUserCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { UserId = null }, ct));

    [HttpPut("users/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpsertAccountUserCommand command, CancellationToken ct) => Ok(await mediator.Send(command with { UserId = userId }, ct));

    public sealed record ToggleBody(bool IsActive);
    [HttpPost("users/{userId}/status")]
    public async Task<IActionResult> ToggleUser(string userId, [FromBody] ToggleBody body, CancellationToken ct) => Ok(await mediator.Send(new ToggleAccountUserStatusCommand(userId, body.IsActive), ct));
}
