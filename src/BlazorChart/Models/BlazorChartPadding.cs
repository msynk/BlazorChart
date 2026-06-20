namespace BlazorChart.Models;

/// <summary>BlazorChartPadding values mirroring Chart.js padding (number or per-side).</summary>
public readonly record struct BlazorChartPadding(double Top, double Right, double Bottom, double Left)
{
    public static BlazorChartPadding All(double v) => new(v, v, v, v);
    public static BlazorChartPadding Symmetric(double vertical, double horizontal) => new(vertical, horizontal, vertical, horizontal);
    public double Vertical => Top + Bottom;
    public double Horizontal => Left + Right;

    public static implicit operator BlazorChartPadding(double v) => All(v);
}
