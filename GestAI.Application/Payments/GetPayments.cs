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
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;

    public GetPaymentsQueryHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
    }

    public async Task<AppResult<List<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Payments, ct))
            return AppResult<List<PaymentDto>>.Fail("feature_disabled", "La gestión de pagos está desactivada para este hospedaje.");

        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);
        if (property is null)
            return AppResult<List<PaymentDto>>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Payments, ct))
            return AppResult<List<PaymentDto>>.Fail("forbidden", "No tenés permisos para usar el módulo de pagos.");

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
