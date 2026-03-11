using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Payments;

public sealed record GetPaymentsQuery(int PropertyId, int BookingId) : IRequest<AppResult<List<PaymentDto>>>;

public sealed class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, AppResult<List<PaymentDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetPaymentsQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var exists = await _db.Bookings.AsNoTracking()
            .AnyAsync(b => b.Id == request.BookingId && b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (!exists) return AppResult<List<PaymentDto>>.Fail("not_found", "Reserva no encontrada.");

        var data = await _db.Payments.AsNoTracking()
            .Where(p => p.BookingId == request.BookingId && p.PropertyId == request.PropertyId)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Select(p => new PaymentDto(p.Id, p.BookingId, p.Amount, p.Method, p.Date, p.Status, p.Notes))
            .ToListAsync(ct);

        return AppResult<List<PaymentDto>>.Ok(data);
    }
}
