using GestAI.Application.Abstractions;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Infrastructure.Ai;

public sealed class MockBookingAssistantService : IBookingAssistantService
{
    public Task<string> GenerateGuestMessageAsync(BookingAssistantRequest request, CancellationToken ct)
    {
        var body = request.TemplateBody
            .Replace("{GuestName}", request.GuestName)
            .Replace("{CheckInDate}", request.CheckInDate.ToString("dd/MM/yyyy"))
            .Replace("{CheckOutDate}", request.CheckOutDate.ToString("dd/MM/yyyy"))
            .Replace("{PropertyName}", request.PropertyName)
            .Replace("{UnitName}", request.UnitName)
            .Replace("{BalanceDue}", request.BalanceDue.ToString("0.00"));
        return Task.FromResult(body);
    }
}

public sealed class MockQuoteSuggestionService : IQuoteSuggestionService
{
    private readonly IAppDbContext _db;
    public MockQuoteSuggestionService(IAppDbContext db) { _db = db; }
    public async Task<QuoteSuggestionResult> SuggestAsync(QuoteSuggestionRequest request, CancellationToken ct)
    {
        var unit = await _db.Units.AsNoTracking().FirstAsync(x => x.Id == request.UnitId!.Value, ct);
        var plan = await _db.RatePlans.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId && x.IsActive)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        decimal total = 0m;
        for (var date = request.CheckInDate; date < request.CheckOutDate; date = date.AddDays(1))
        {
            var nightly = plan?.BaseNightlyRate ?? unit.BaseRate;
            if (plan is not null)
            {
                if (plan.WeekendAdjustmentEnabled && (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday))
                {
                    nightly = plan.WeekendAdjustmentType == RateAdjustmentType.Fixed ? nightly + plan.WeekendAdjustmentValue : nightly + (nightly * plan.WeekendAdjustmentValue / 100m);
                }
                var seasonal = await _db.SeasonalRates.AsNoTracking().Where(x => x.RatePlanId == plan.Id && x.IsActive).ToListAsync(ct);
                foreach (var s in seasonal)
                {
                    var md = date.Month * 100 + date.Day;
                    var start = s.StartMonth * 100 + s.StartDay;
                    var end = s.EndMonth * 100 + s.EndDay;
                    var inRange = start <= end ? md >= start && md <= end : md >= start || md <= end;
                    if (inRange)
                        nightly = s.AdjustmentType == RateAdjustmentType.Fixed ? nightly + s.AdjustmentValue : nightly + (nightly * s.AdjustmentValue / 100m);
                }
                var ranges = await _db.DateRangeRates.AsNoTracking().Where(x => x.RatePlanId == plan.Id && x.IsActive && x.DateFrom <= date && date <= x.DateTo).ToListAsync(ct);
                foreach (var r in ranges)
                    nightly = r.AdjustmentType == RateAdjustmentType.Fixed ? nightly + r.AdjustmentValue : nightly + (nightly * r.AdjustmentValue / 100m);
            }
            total += nightly;
        }
        var nightlyAvg = nights <= 0 ? 0m : Math.Round(total / nights, 2);
        return new QuoteSuggestionResult(nightlyAvg, Math.Round(total, 2), $"Tarifa mock calculada para {nights} noche(s).");
    }
}
