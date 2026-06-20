namespace BlazorChart.Models;

/// <summary>
/// Per-segment line styling, mirroring Chart.js dataset <c>segment</c>. Each callback receives the
/// segment endpoints and returns an override (or null to use the dataset default).
/// </summary>
public sealed class BlazorChartLineSegmentStyle
{
    public Func<BlazorChartSegmentContext, string?>? BorderColor { get; set; }
    public Func<BlazorChartSegmentContext, double?>? BorderWidth { get; set; }
    public Func<BlazorChartSegmentContext, IReadOnlyList<double>?>? BorderDash { get; set; }
}
