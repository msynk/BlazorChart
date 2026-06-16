namespace BlazorChart.Models;

/// <summary>Built-in hatch/pattern styles for fills.</summary>
public enum PatternStyle
{
    DiagonalUp,
    DiagonalDown,
    Horizontal,
    Vertical,
    Grid,
    Cross,
    Dots
}

/// <summary>A repeating SVG pattern fill (hatching, dots, grid, ...).</summary>
public sealed class FillPattern
{
    public PatternStyle Style { get; set; } = PatternStyle.DiagonalUp;
    public string Color { get; set; } = "#36a2eb";
    /// <summary>Background color behind the pattern (null = transparent).</summary>
    public string? Background { get; set; }
    /// <summary>Tile size in pixels.</summary>
    public double Size { get; set; } = 8;
    /// <summary>Stroke/dot thickness.</summary>
    public double LineWidth { get; set; } = 2;

    public FillPattern() { }

    public FillPattern(PatternStyle style, string color, string? background = null)
    {
        Style = style;
        Color = color;
        Background = background;
    }
}
