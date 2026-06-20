using System.Globalization;
using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>
/// Generates nice tick boundaries and labels for a time scale. Axis values are stored as
/// OLE Automation dates (<see cref="DateTime.ToOADate"/>), i.e. days since 1899-12-30.
/// </summary>
public static class BlazorChartTimeAxis
{
    public static BlazorChartTimeUnit ChooseUnit(DateTime min, DateTime max)
    {
        var span = max - min;
        if (span <= TimeSpan.FromSeconds(2)) return BlazorChartTimeUnit.Millisecond;
        if (span <= TimeSpan.FromMinutes(2)) return BlazorChartTimeUnit.Second;
        if (span <= TimeSpan.FromHours(2)) return BlazorChartTimeUnit.Minute;
        if (span <= TimeSpan.FromDays(2)) return BlazorChartTimeUnit.Hour;
        if (span <= TimeSpan.FromDays(14)) return BlazorChartTimeUnit.Day;
        if (span <= TimeSpan.FromDays(60)) return BlazorChartTimeUnit.Week;
        if (span <= TimeSpan.FromDays(365 * 2)) return BlazorChartTimeUnit.Month;
        return BlazorChartTimeUnit.Year;
    }

    public static string DefaultFormat(DateTime d, BlazorChartTimeUnit unit) => unit switch
    {
        BlazorChartTimeUnit.Millisecond => d.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Second => d.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Minute => d.ToString("HH:mm", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Hour => d.ToString("HH:mm", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Day => d.ToString("MMM d", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Week => d.ToString("MMM d", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Month => d.ToString("MMM yyyy", CultureInfo.InvariantCulture),
        BlazorChartTimeUnit.Quarter => $"Q{(d.Month - 1) / 3 + 1} {d.Year}",
        _ => d.Year.ToString(CultureInfo.InvariantCulture)
    };

    private static DateTime Floor(DateTime d, BlazorChartTimeUnit unit) => unit switch
    {
        BlazorChartTimeUnit.Millisecond => d,
        BlazorChartTimeUnit.Second => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second),
        BlazorChartTimeUnit.Minute => new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0),
        BlazorChartTimeUnit.Hour => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0),
        BlazorChartTimeUnit.Day or BlazorChartTimeUnit.Week => d.Date,
        BlazorChartTimeUnit.Month => new DateTime(d.Year, d.Month, 1),
        BlazorChartTimeUnit.Quarter => new DateTime(d.Year, (d.Month - 1) / 3 * 3 + 1, 1),
        _ => new DateTime(d.Year, 1, 1)
    };

    private static DateTime Next(DateTime d, BlazorChartTimeUnit unit, int step) => unit switch
    {
        BlazorChartTimeUnit.Millisecond => d.AddMilliseconds(step),
        BlazorChartTimeUnit.Second => d.AddSeconds(step),
        BlazorChartTimeUnit.Minute => d.AddMinutes(step),
        BlazorChartTimeUnit.Hour => d.AddHours(step),
        BlazorChartTimeUnit.Day => d.AddDays(step),
        BlazorChartTimeUnit.Week => d.AddDays(7 * step),
        BlazorChartTimeUnit.Month => d.AddMonths(step),
        BlazorChartTimeUnit.Quarter => d.AddMonths(3 * step),
        _ => d.AddYears(step)
    };

    /// <summary>Generates (oaDateValue, label) ticks between min and max.</summary>
    public static List<(double Value, string Label)> Ticks(double minOa, double maxOa, BlazorChartTimeUnit unit,
        Func<DateTime, string>? format, int maxTicks = 11)
    {
        var min = DateTime.FromOADate(minOa);
        var max = DateTime.FromOADate(maxOa);
        if (unit == BlazorChartTimeUnit.Auto) unit = ChooseUnit(min, max);

        // Choose a step so we don't exceed maxTicks.
        int step = 1;
        var probe = Floor(min, unit);
        int count = 0;
        for (var t = probe; t <= max; t = Next(t, unit, 1)) { count++; if (count > 5000) break; }
        if (count > maxTicks) step = (int)Math.Ceiling((double)count / maxTicks);

        var ticks = new List<(double, string)>();
        var cur = Floor(min, unit);
        while (cur <= max)
        {
            if (cur >= min)
                ticks.Add((cur.ToOADate(), (format ?? (d => DefaultFormat(d, unit)))(cur)));
            cur = Next(cur, unit, step);
            if (ticks.Count > maxTicks * 3) break;
        }
        if (ticks.Count == 0)
            ticks.Add((minOa, (format ?? (d => DefaultFormat(d, unit)))(min)));
        return ticks;
    }
}
