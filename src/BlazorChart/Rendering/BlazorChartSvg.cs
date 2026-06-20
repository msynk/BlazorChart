using System.Globalization;

namespace BlazorChart.Rendering;

/// <summary>Helpers for formatting numbers in an invariant, SVG-friendly way.</summary>
public static class BlazorChartSvg
{
    public static string N(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
        return Math.Round(v, 3).ToString(CultureInfo.InvariantCulture);
    }

    public static string Dash(IEnumerable<double>? dash) =>
        dash is null ? "" : string.Join(",", dash.Select(N));
}
