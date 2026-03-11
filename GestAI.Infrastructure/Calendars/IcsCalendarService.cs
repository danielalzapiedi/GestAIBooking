using GestAI.Application.Abstractions;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace GestAI.Infrastructure.Calendars;

public sealed class IcsCalendarService(HttpClient httpClient) : IIcsCalendarService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<IReadOnlyList<IcsCalendarEvent>> LoadAsync(string url, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(ct);
        return Parse(content);
    }

    public string BuildUnitCalendar(string propertyName, string unitName, string exportUrl, IReadOnlyCollection<IcsCalendarEvent> events)
    {
        static string Escape(string? value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//GestAI Booking//Calendar Sync//ES");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine($"X-WR-CALNAME:{Escape(propertyName)} - {Escape(unitName)}");
        sb.AppendLine($"X-WR-RELCALID:{Escape(exportUrl)}");
        foreach (var item in events.OrderBy(x => x.StartDate))
        {
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Escape(item.Uid)}");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}");
            sb.AppendLine($"DTSTART;VALUE=DATE:{item.StartDate:yyyyMMdd}");
            sb.AppendLine($"DTEND;VALUE=DATE:{item.EndDate:yyyyMMdd}");
            sb.AppendLine($"SUMMARY:{Escape(item.Summary ?? "Ocupado")}");
            if (item.IsCancelled) sb.AppendLine("STATUS:CANCELLED");
            sb.AppendLine("END:VEVENT");
        }
        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    private static IReadOnlyList<IcsCalendarEvent> Parse(string content)
    {
        var normalized = content.Replace("\r\n ", string.Empty).Replace("\n ", string.Empty).Replace("\r\n\t", string.Empty).Replace("\n\t", string.Empty);
        var matches = Regex.Matches(normalized, "BEGIN:VEVENT(?<body>.*?)END:VEVENT", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var list = new List<IcsCalendarEvent>();
        foreach (Match match in matches)
        {
            var body = match.Groups["body"].Value;
            var uid = ReadProperty(body, "UID") ?? Guid.NewGuid().ToString("N");
            var summary = ReadProperty(body, "SUMMARY");
            var status = ReadProperty(body, "STATUS");
            var startRaw = ReadPropertyWithParameters(body, "DTSTART");
            var endRaw = ReadPropertyWithParameters(body, "DTEND");
            if (string.IsNullOrWhiteSpace(startRaw) || string.IsNullOrWhiteSpace(endRaw))
                continue;
            if (!TryParseDate(startRaw, out var start) || !TryParseDate(endRaw, out var end))
                continue;
            if (end <= start)
                end = start.AddDays(1);
            list.Add(new IcsCalendarEvent(uid.Trim(), start, end, summary?.Trim(), string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase), body.Trim()));
        }
        return list;
    }

    private static string? ReadProperty(string body, string property)
    {
        var match = Regex.Match(body, $"(?:^|\\n){property}:(?<value>.*?)(?:\\n|$)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["value"].Value.Trim() : null;
    }

    private static string? ReadPropertyWithParameters(string body, string property)
    {
        var match = Regex.Match(body, $"(?:^|\\n){property}(;[^:]+)?:(?<value>.*?)(?:\\n|$)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["value"].Value.Trim() : null;
    }

    private static bool TryParseDate(string value, out DateOnly date)
    {
        value = value.Trim();
        if (DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;
        if (DateTime.TryParseExact(value, new[] { "yyyyMMdd'T'HHmmss'Z'", "yyyyMMdd'T'HHmmss", "yyyyMMdd'T'HHmm", "yyyyMMdd'T'HHmmssK" }, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }
        date = default;
        return false;
    }
}
