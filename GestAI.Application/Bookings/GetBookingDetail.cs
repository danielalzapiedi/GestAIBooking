using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Application.Common.Pricing;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record GetBookingDetailQuery(int PropertyId, int BookingId) : IRequest<AppResult<BookingDetailDto>>;

public sealed class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, AppResult<BookingDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetBookingDetailQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<BookingDetailDto>> Handle(GetBookingDetailQuery request, CancellationToken ct)
    {
        var b = await _db.Bookings
            .AsNoTracking()
            .Include(x => x.Unit)
            .Include(x => x.Guest)
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.PropertyId == request.PropertyId && x.Id == request.BookingId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        if (b is null)
            return AppResult<BookingDetailDto>.Fail("not_found", "Reserva no encontrada.");

        var paid = await _db.Payments.AsNoTracking()
            .Where(p => p.BookingId == b.Id && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        var hasCleaningTaskPending = await _db.OperationalTasks.AsNoTracking()
            .AnyAsync(t => t.PropertyId == b.PropertyId && t.BookingId == b.Id && t.Type == OperationalTaskType.Cleaning && t.Status != OperationalTaskStatus.Completed && t.Status != OperationalTaskStatus.Cancelled, ct);

        var lines = new List<PricingLineDto>
        {
            new("Tarifa base", b.BaseAmount, "base")
        };

        if (b.PromotionsAmount > 0)
            lines.Add(new PricingLineDto("Promociones", -b.PromotionsAmount, "promotion"));

        if (b.ManualPriceOverride)
            lines.Add(new PricingLineDto("Override manual", b.TotalAmount - (b.BaseAmount - b.PromotionsAmount), "override"));

        var nights = Math.Max(0, b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber);
        var dto = new BookingDetailDto(
            b.Id,
            b.PropertyId,
            b.UnitId,
            b.Unit.Name,
            b.GuestId,
            b.Guest.FullName,
            b.Guest.Phone,
            b.Guest.Email,
            b.Guest.DocumentNumber,
            b.BookingCode,
            b.CheckInDate,
            b.CheckOutDate,
            b.Adults,
            b.Children,
            b.Status,
            b.Source,
            b.OperationalStatus,
            b.TotalAmount,
            paid,
            b.TotalAmount - paid,
            nights > 0 ? b.TotalAmount / nights : 0m,
            nights,
            b.BaseAmount,
            b.PromotionsAmount,
            b.ExpectedDepositAmount,
            b.DepositDueDate,
            b.DepositVerified,
            b.DocumentationVerified,
            b.ManualPriceOverride,
            b.CreatedFromQuote,
            b.AppliedPromotionNames,
            b.CancellationPolicyApplied,
            b.CancellationReason,
            b.Tags,
            b.FinalGuestsCount,
            b.ActualCheckInTime,
            b.ActualCheckOutTime,
            b.CheckInNotes,
            b.CheckOutNotes,
            b.Notes,
            b.InternalNotes,
            b.GuestVisibleNotes,
            b.CreatedAt,
            b.UpdatedAt,
            hasCleaningTaskPending,
            b.Events.OrderByDescending(x => x.ChangedAtUtc)
                .Select(x => new BookingEventDto(x.Id, x.EventType, x.Title, x.Detail, x.ChangedByName, x.ChangedAtUtc))
                .ToList(),
            lines);

        return AppResult<BookingDetailDto>.Ok(dto);
    }
}
