namespace GestAI.Application.Common;

public static class DateRange
{
    // Rango [start, end) solapa con [start2, end2) ?
    public static bool Overlaps(DateOnly start, DateOnly endExclusive, DateOnly start2, DateOnly end2Exclusive)
        => start < end2Exclusive && start2 < endExclusive;
}
