namespace GestAI.Infrastructure.Calendars;

public sealed class ExternalCalendarAutoSyncOptions
{
    public const string SectionName = "ExternalCalendarAutoSync";

    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
}

