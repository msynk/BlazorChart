namespace BlazorChart.Models;

/// <summary>Per-corner radius for a bar, mirroring Chart.js <c>borderRadius</c> object form.</summary>
public readonly record struct BlazorChartBorderRadiusCorners(
    double TopLeft, double TopRight, double BottomRight, double BottomLeft)
{
    public static implicit operator BlazorChartBorderRadiusCorners(double all) => new(all, all, all, all);
    /// <summary>Rounds only the two "top" (away-from-baseline) corners.</summary>
    public static BlazorChartBorderRadiusCorners Top(double r) => new(r, r, 0, 0);
}
