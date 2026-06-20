namespace BlazorChart;

/// <summary>Font definition mirroring Chart.js Font options.</summary>
public sealed class BlazorChartFont
{
    public string Family { get; set; } = "Helvetica, Arial, sans-serif";
    public double Size { get; set; } = 12;
    public string Style { get; set; } = "normal";
    public string Weight { get; set; } = "normal";
    public double LineHeight { get; set; } = 1.2;

    public BlazorChartFont Clone() => new()
    {
        Family = Family,
        Size = Size,
        Style = Style,
        Weight = Weight,
        LineHeight = LineHeight
    };

    /// <summary>The total line height in pixels.</summary>
    public double LineHeightPx => Size * LineHeight;
}
