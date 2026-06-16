namespace BlazorChart.Models;

/// <summary>Context passed to per-segment styling callbacks for a line.</summary>
public readonly record struct SegmentContext(
    int StartIndex,
    int EndIndex,
    double StartValue,
    double EndValue);

/// <summary>
/// Per-segment line styling, mirroring Chart.js dataset <c>segment</c>. Each callback receives the
/// segment endpoints and returns an override (or null to use the dataset default).
/// </summary>
public sealed class LineSegmentStyle
{
    public Func<SegmentContext, string?>? BorderColor { get; set; }
    public Func<SegmentContext, double?>? BorderWidth { get; set; }
    public Func<SegmentContext, IReadOnlyList<double>?>? BorderDash { get; set; }
}
