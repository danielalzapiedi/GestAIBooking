using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Common.Pricing;

public sealed record PricingLineDto(string Label, decimal Amount, string Kind);
public sealed record NightBreakdownDto(DateOnly Date, decimal BaseRate, List<PricingLineDto> Adjustments, decimal FinalRate);
public sealed record AppliedPromotionDto(int PromotionId, string Name, decimal Amount, bool IsCumulative, int Priority);
public sealed record PricingResult(
    decimal BaseAmount,
    decimal PromotionsAmount,
    decimal FinalAmount,
    decimal SuggestedDepositAmount,
    decimal SuggestedNightlyRate,
    string AppliedPromotionNames,
    List<string> Rules,
    List<PricingLineDto> Lines,
    List<NightBreakdownDto> NightBreakdown,
    List<AppliedPromotionDto> AppliedPromotions);

public static class CommercialPricing
{
    public static async Task<PricingResult> CalculateAsync(IAppDbContext db, int propertyId, int unitId, DateOnly checkIn, DateOnly checkOut, int adults, int children, CancellationToken ct)
    {
        var unit = await db.Units.AsNoTracking().FirstAsync(x => x.Id == unitId && x.PropertyId == propertyId && x.IsActive, ct);
        var property = await db.Properties.AsNoTracking().FirstAsync(x => x.Id == propertyId && x.IsActive, ct);
        var nights = Math.Max(1, checkOut.DayNumber - checkIn.DayNumber);
        var occupancy = adults + children;
        var rules = new List<string>();
        var lines = new List<PricingLineDto>();
        var nightBreakdown = new List<NightBreakdownDto>();

        if (occupancy > unit.TotalCapacity || adults > unit.CapacityAdults || children > unit.CapacityChildren)
        {
            rules.Add($"La ocupación solicitada supera la capacidad de la unidad {unit.Name}.");
        }

        var ratePlan = await db.RatePlans.AsNoTracking()
            .Include(x => x.SeasonalRates)
            .Include(x => x.DateRangeRates)
            .Where(x => x.PropertyId == propertyId && x.UnitId == unitId && x.IsActive)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        decimal baseAmount = 0m;
        for (var date = checkIn; date < checkOut; date = date.AddDays(1))
        {
            var nightlyBase = ratePlan?.BaseNightlyRate ?? unit.BaseRate;
            var nightly = nightlyBase;
            var nightlyLines = new List<PricingLineDto>();

            if (ratePlan?.WeekendAdjustmentEnabled == true && (date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday))
            {
                var adjusted = Apply(ratePlan.WeekendAdjustmentType, nightly, ratePlan.WeekendAdjustmentValue);
                nightlyLines.Add(new PricingLineDto("Ajuste fin de semana", Math.Round(adjusted - nightly, 2), "weekend"));
                nightly = adjusted;
            }

            if (ratePlan is not null)
            {
                foreach (var s in ratePlan.SeasonalRates.Where(x => x.IsActive && InSeason(x, date)).OrderBy(x => x.Name))
                {
                    var adjusted = Apply(s.AdjustmentType, nightly, s.AdjustmentValue);
                    nightlyLines.Add(new PricingLineDto($"Temporada: {s.Name}", Math.Round(adjusted - nightly, 2), "season"));
                    nightly = adjusted;
                }

                foreach (var r in ratePlan.DateRangeRates.Where(x => x.IsActive && x.DateFrom <= date && date < x.DateTo).OrderBy(x => x.DateFrom))
                {
                    var adjusted = Apply(r.AdjustmentType, nightly, r.AdjustmentValue);
                    nightlyLines.Add(new PricingLineDto($"Rango: {r.Name}", Math.Round(adjusted - nightly, 2), "range"));
                    nightly = adjusted;
                }
            }

            baseAmount += nightly;
            nightBreakdown.Add(new NightBreakdownDto(date, Math.Round(nightlyBase, 2), nightlyLines, Math.Round(nightly, 2)));
        }

        lines.Add(new PricingLineDto("Tarifa base calculada", Math.Round(baseAmount, 2), "base"));

        var promos = await db.Promotions.AsNoTracking()
            .Where(x => x.PropertyId == propertyId
                && x.IsActive
                && !x.IsDeleted
                && (x.UnitId == null || x.UnitId == unitId)
                && x.DateFrom <= checkIn
                && checkOut <= x.DateTo)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.IsCumulative)
            .ToListAsync(ct);

        var validationRules = ValidatePromotionsAndRules(promos, checkIn, checkOut, DateOnly.FromDateTime(DateTime.UtcNow.Date));
        rules.AddRange(validationRules);

        decimal promotionsAmount = 0m;
        var appliedNames = new List<string>();
        var appliedPromotions = new List<AppliedPromotionDto>();
        foreach (var promo in promos)
        {
            if (!IsPromotionApplicable(promo, checkIn, checkOut, DateOnly.FromDateTime(DateTime.UtcNow.Date)))
                continue;

            var target = promo.Scope == PromotionScope.PerNight
                ? Math.Round(baseAmount / nights, 2) * nights
                : baseAmount;
            var delta = promo.ValueType == DiscountValueType.Percentage
                ? Math.Round(target * promo.Value / 100m, 2)
                : Math.Round(promo.Value, 2);

            if (delta <= 0)
                continue;

            promotionsAmount += delta;
            appliedNames.Add(promo.Name);
            appliedPromotions.Add(new AppliedPromotionDto(promo.Id, promo.Name, delta, promo.IsCumulative, promo.Priority));
            lines.Add(new PricingLineDto($"Promoción: {promo.Name}", -delta, "promotion"));

            if (!promo.IsCumulative)
                break;
        }

        var finalAmount = Math.Max(0m, baseAmount - promotionsAmount);
        var suggestedDeposit = Math.Round(finalAmount * property.DefaultDepositPercentage / 100m, 2);
        lines.Add(new PricingLineDto("Total final", Math.Round(finalAmount, 2), "total"));
        lines.Add(new PricingLineDto("Seña sugerida", suggestedDeposit, "deposit"));

        return new PricingResult(
            Math.Round(baseAmount, 2),
            Math.Round(promotionsAmount, 2),
            Math.Round(finalAmount, 2),
            suggestedDeposit,
            nights == 0 ? 0 : Math.Round(finalAmount / nights, 2),
            string.Join(", ", appliedNames),
            rules.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            lines,
            nightBreakdown,
            appliedPromotions);
    }

    public static List<string> ValidatePromotionsAndRules(IEnumerable<Promotion> promos, DateOnly checkIn, DateOnly checkOut, DateOnly? today = null)
    {
        var currentDate = today ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var leadDays = checkIn.DayNumber - currentDate.DayNumber;
        var errors = new List<string>();
        foreach (var p in promos.Where(x => x.IsActive && !x.IsDeleted))
        {
            if (p.MinNights.HasValue && nights < p.MinNights.Value) errors.Add($"La promo/regla '{p.Name}' exige estadía mínima de {p.MinNights} noches.");
            if (p.MaxNights.HasValue && nights > p.MaxNights.Value) errors.Add($"La promo/regla '{p.Name}' permite hasta {p.MaxNights} noches.");
            if (p.BookingWindowDaysMin.HasValue && leadDays < p.BookingWindowDaysMin.Value) errors.Add($"La promo/regla '{p.Name}' requiere reservar con al menos {p.BookingWindowDaysMin} días de anticipación.");
            if (p.BookingWindowDaysMax.HasValue && leadDays > p.BookingWindowDaysMax.Value) errors.Add($"La promo/regla '{p.Name}' solo aplica para reservas con hasta {p.BookingWindowDaysMax} días de anticipación.");
            if (!Allowed(p.AllowedCheckInDays, checkIn.DayOfWeek)) errors.Add($"La promo/regla '{p.Name}' no permite check-in el {checkIn:dddd}.");
            if (!Allowed(p.AllowedCheckOutDays, checkOut.AddDays(-1).DayOfWeek)) errors.Add($"La promo/regla '{p.Name}' no permite check-out el {checkOut.AddDays(-1):dddd}.");
        }
        return errors;
    }

    public static bool IsPromotionApplicable(Promotion promo, DateOnly checkIn, DateOnly checkOut, DateOnly? today = null)
        => !ValidatePromotionsAndRules([promo], checkIn, checkOut, today).Any();

    private static decimal Apply(RateAdjustmentType type, decimal current, decimal value)
        => type == RateAdjustmentType.Percentage ? current + (current * value / 100m) : current + value;

    private static bool Allowed(string? csv, DayOfWeek day)
        => string.IsNullOrWhiteSpace(csv)
            || csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(day.ToString(), StringComparer.OrdinalIgnoreCase);

    private static bool InSeason(SeasonalRate rate, DateOnly date)
    {
        var start = new DateOnly(date.Year, rate.StartMonth, rate.StartDay);
        var end = new DateOnly(date.Year, rate.EndMonth, rate.EndDay);
        if (end >= start) return start <= date && date <= end;
        return date >= start || date <= end;
    }
}
