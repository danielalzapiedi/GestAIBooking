using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

public sealed record GetPropertyDetailQuery(int PropertyId) : IRequest<AppResult<PropertyDetailDto>>;

public sealed class GetPropertyDetailQueryHandler : IRequestHandler<GetPropertyDetailQuery, AppResult<PropertyDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    public GetPropertyDetailQueryHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<PropertyDetailDto>> Handle(GetPropertyDetailQuery request, CancellationToken ct)
    {
        var dto = await _db.Properties.AsNoTracking()
            .Where(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(x => new PropertyDetailDto(x.Id, x.Name, x.CommercialName, x.Type, x.IsActive, x.Phone, x.Email, x.City, x.Province, x.Country, x.Address, x.DefaultCheckInTime, x.DefaultCheckOutTime, x.Currency, x.DepositPolicy, x.DefaultDepositPercentage, x.CancellationPolicy, x.TermsAndConditions, x.CheckInInstructions, x.PropertyRules, x.CommercialContactName, x.CommercialContactPhone, x.CommercialContactEmail, x.PublicSlug, x.PublicDescription))
            .FirstOrDefaultAsync(ct);
        return dto is null ? AppResult<PropertyDetailDto>.Fail("not_found", "Hospedaje no encontrado.") : AppResult<PropertyDetailDto>.Ok(dto);
    }
}
