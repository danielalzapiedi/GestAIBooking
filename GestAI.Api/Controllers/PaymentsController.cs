using GestAI.Application.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/properties/{propertyId:int}/bookings/{bookingId:int}/[controller]")]
[Authorize]
public sealed class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int propertyId, int bookingId, CancellationToken ct)
        => Ok(await mediator.Send(new GetPaymentsQuery(propertyId, bookingId), ct));

    [HttpPost]
    public async Task<IActionResult> Create(int propertyId, int bookingId, [FromBody] CreatePaymentCommand command, CancellationToken ct)
        => Ok(await mediator.Send(command with { PropertyId = propertyId, BookingId = bookingId }, ct));

    [HttpDelete("{paymentId:int}")]
    public async Task<IActionResult> Delete(int propertyId, int bookingId, int paymentId, CancellationToken ct)
        => Ok(await mediator.Send(new DeletePaymentCommand(propertyId, paymentId), ct));
}
