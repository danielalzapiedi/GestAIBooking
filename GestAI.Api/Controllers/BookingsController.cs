using GestAI.Application.Bookings;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/[controller]")]
[Authorize]
public sealed class BookingsController(IMediator mediator) : ControllerBase
{
    [HttpGet("range")]
    public async Task<IActionResult> ByRange(int propertyId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await mediator.Send(new GetBookingsByRangeQuery(propertyId, from, to), ct));

    [HttpGet]
    public async Task<IActionResult> List(int propertyId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await mediator.Send(new GetBookingsListQuery(propertyId, from, to), ct));

    [HttpGet("{bookingId:int}")]
    public async Task<IActionResult> Detail(int propertyId, int bookingId, CancellationToken ct)
        => Ok(await mediator.Send(new GetBookingDetailQuery(propertyId, bookingId), ct));

    [HttpPost]
    public async Task<IActionResult> Upsert(int propertyId, [FromBody] UpsertBookingCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId }, ct));

    [HttpPost("{bookingId:int}/duplicate")]
    public async Task<IActionResult> Duplicate(int propertyId, int bookingId, [FromBody] DuplicateBookingCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, SourceBookingId = bookingId }, ct));

    public sealed record ChangeStatusBody(BookingStatus Status, string? Reason);

    [HttpPost("{bookingId:int}/status")]
    public async Task<IActionResult> ChangeStatus(int propertyId, int bookingId, [FromBody] ChangeStatusBody body, CancellationToken ct)
        => Ok(await mediator.Send(new ChangeBookingStatusCommand(propertyId, bookingId, body.Status, body.Reason), ct));

    [HttpPost("{bookingId:int}/check-in")]
    public async Task<IActionResult> CheckIn(int propertyId, int bookingId, [FromBody] CheckInOutCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, BookingId = bookingId, IsCheckIn = true }, ct));

    [HttpPost("{bookingId:int}/check-out")]
    public async Task<IActionResult> CheckOut(int propertyId, int bookingId, [FromBody] CheckInOutCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, BookingId = bookingId, IsCheckIn = false }, ct));

    [HttpDelete("{bookingId:int}")]
    public async Task<IActionResult> Cancel(int propertyId, int bookingId, [FromQuery] string? reason, CancellationToken ct)
        => Ok(await mediator.Send(new CancelBookingCommand(propertyId, bookingId, reason), ct));
}
