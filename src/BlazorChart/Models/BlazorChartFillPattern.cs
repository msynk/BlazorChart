namespace BlazorChart.Models;

/// <summary>A repeating SVG pattern fill (hatching, dots, grid, ...).</summary>
public sealed class BlazorChartFillPattern
{
    public BlazorChartPatternStyle Style { get; set; } = BlazorChartPatternStyle.DiagonalUp;
    public string Color { get; set; } = "#36a2eb";
    /// <summary>Background color behind the pattern (null = transparent).</summary>
    public string? Background { get; set; }
    /// <summary>Tile size in pixels.</summary>
    public double Size { get; set; } = 8;
    /// <summary>Stroke/dot thickness.</summary>
    public double LineWidth { get; set; } = 2;

    public BlazorChartFillPattern() { }

    public BlazorChartFillPattern(BlazorChartPatternStyle style, string color, string? background = null)
    {
        Style = style;
        Color = color;
        Background = background;
    }
}
