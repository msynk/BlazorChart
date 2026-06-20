namespace BlazorChart.Models;

/// <summary>
/// A linear gradient fill. By default it runs vertically (top→bottom of the chart area),
/// which is the common case for area fills.
/// </summary>
public sealed class BlazorChartLinearGradient : BlazorChartGradientBase
{
    /// <summary>When true the gradient runs vertically, otherwise horizontally.</summary>
    public bool Vertical { get; set; } = true;

    public BlazorChartLinearGradient() { }

    public BlazorChartLinearGradient(bool vertical, params BlazorChartGradientStop[] stops)
    {
        Vertical = vertical;
        Stops.AddRange(stops);
    }

    /// <summary>Convenience: a two-stop top→bottom gradient between two colors.</summary>
    public static BlazorChartLinearGradient Vertical2(string top, string bottom) =>
        new(true, new BlazorChartGradientStop(0, top), new BlazorChartGradientStop(1, bottom));

    /// <summary>Convenience: a two-stop left→right gradient between two colors.</summary>
    public static BlazorChartLinearGradient Horizontal2(string left, string right) =>
        new(false, new BlazorChartGradientStop(0, left), new BlazorChartGradientStop(1, right));
}
