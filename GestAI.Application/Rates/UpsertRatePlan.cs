using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Rates;

public sealed record SeasonalRateInputDto(string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record DateRangeRateInputDto(string Name, DateOnly DateFrom, DateOnly DateTo, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record UpsertRatePlanCommand(int PropertyId, int? RatePlanId, int UnitId, string Name, decimal BaseNightlyRate, bool WeekendAdjustmentEnabled, RateAdjustmentType WeekendAdjustmentType, decimal WeekendAdjustmentValue, bool IsActive, List<SeasonalRateInputDto>? SeasonalRates, List<DateRangeRateInputDto>? DateRangeRates) : IRequest<AppResult<int>>;

public sealed class UpsertRatePlanCommandValidator : AbstractValidator<UpsertRatePlanCommand>
{
    public UpsertRatePlanCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BaseNightlyRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WeekendAdjustmentValue).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.SeasonalRates).ChildRules(rate =>
        {
            rate.RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            rate.RuleFor(x => x.StartMonth).InclusiveBetween(1, 12);
            rate.RuleFor(x => x.EndMonth).InclusiveBetween(1, 12);
            rate.RuleFor(x => x.StartDay).InclusiveBetween(1, 31);
            rate.RuleFor(x => x.EndDay).InclusiveBetween(1, 31);
            rate.RuleFor(x => x.AdjustmentValue).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.DateRangeRates).ChildRules(rate =>
        {
            rate.RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            rate.RuleFor(x => x.DateTo).GreaterThan(x => x.DateFrom);
            rate.RuleFor(x => x.AdjustmentValue).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class UpsertRatePlanCommandHandler : IRequestHandler<UpsertRatePlanCommand, AppResult<int>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public UpsertRatePlanCommandHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<int>> Handle(UpsertRatePlanCommand request, CancellationToken ct)
    {
        var unit = await _db.Units.AsNoTracking()
            .Include(x => x.Property)
            .FirstOrDefaultAsync(x => x.Id == request.UnitId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (unit is null) return AppResult<int>.Fail("forbidden", "Unidad inválida.");
        if (!unit.IsActive) return AppResult<int>.Fail("unit_inactive", "No podés asignar tarifas a una unidad inactiva.");
        if (!unit.Property.IsActive && request.IsActive) return AppResult<int>.Fail("property_inactive", "No podés activar tarifas en un hospedaje inactivo.");

        var duplicateName = await _db.RatePlans.AsNoTracking()
            .AnyAsync(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId && x.Id != (request.RatePlanId ?? 0) && x.Name == request.Name.Trim(), ct);
        if (duplicateName) return AppResult<int>.Fail("duplicate_name", "Ya existe una tarifa con ese nombre para la unidad.");

        var seasonalRates = request.SeasonalRates ?? [];
        var dateRangeRates = request.DateRangeRates ?? [];
        foreach (var season in seasonalRates)
        {
            if (!IsValidMonthDay(season.StartMonth, season.StartDay) || !IsValidMonthDay(season.EndMonth, season.EndDay))
                return AppResult<int>.Fail("season_invalid", $"La temporada '{season.Name}' tiene un día o mes inválido.");
        }
        for (var i = 0; i < dateRangeRates.Count; i++)
        {
            for (var j = i + 1; j < dateRangeRates.Count; j++)
            {
                var a = dateRangeRates[i];
                var b = dateRangeRates[j];
                if (a.IsActive && b.IsActive && a.DateFrom < b.DateTo && b.DateFrom < a.DateTo)
                    return AppResult<int>.Fail("date_overlap", "Hay rangos específicos superpuestos dentro de la tarifa.");
            }
        }

        RatePlan entity;
        if (request.RatePlanId is null)
        {
            entity = new RatePlan { PropertyId = request.PropertyId, UnitId = request.UnitId };
            _db.RatePlans.Add(entity);
        }
        else
        {
            entity = await _db.RatePlans.Include(x => x.SeasonalRates).Include(x => x.DateRangeRates)
                .FirstOrDefaultAsync(x => x.Id == request.RatePlanId.Value && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Tarifa no encontrada.");
            _db.SeasonalRates.RemoveRange(entity.SeasonalRates);
            _db.DateRangeRates.RemoveRange(entity.DateRangeRates);
        }
        entity.UnitId = request.UnitId;
        entity.Name = request.Name.Trim();
        entity.BaseNightlyRate = request.BaseNightlyRate;
        entity.WeekendAdjustmentEnabled = request.WeekendAdjustmentEnabled;
        entity.WeekendAdjustmentType = request.WeekendAdjustmentType;
        entity.WeekendAdjustmentValue = request.WeekendAdjustmentValue;
        entity.IsActive = request.IsActive;
        entity.SeasonalRates = seasonalRates.Select(x => new SeasonalRate { Name = x.Name.Trim(), StartMonth = x.StartMonth, StartDay = x.StartDay, EndMonth = x.EndMonth, EndDay = x.EndDay, AdjustmentType = x.AdjustmentType, AdjustmentValue = x.AdjustmentValue, IsActive = x.IsActive }).ToList();
        entity.DateRangeRates = dateRangeRates.Select(x => new DateRangeRate { Name = x.Name.Trim(), DateFrom = x.DateFrom, DateTo = x.DateTo, AdjustmentType = x.AdjustmentType, AdjustmentValue = x.AdjustmentValue, IsActive = x.IsActive }).ToList();
        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(entity.Id);
    }

    private static bool IsValidMonthDay(int month, int day)
    {
        if (month < 1 || month > 12)
            return false;

        if (day < 1)
            return false;

        var daysInMonth = DateTime.DaysInMonth(2000, month); // año cualquiera

        return day <= daysInMonth;
    }
}
