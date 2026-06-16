namespace BlazorChart.Models;

/// <summary>Font definition mirroring Chart.js Font options.</summary>
public sealed class ChartFont
{
    public string Family { get; set; } = "Helvetica, Arial, sans-serif";
    public double Size { get; set; } = 12;
    public string Style { get; set; } = "normal";
    public string Weight { get; set; } = "normal";
    public double LineHeight { get; set; } = 1.2;

    public ChartFont Clone() => new()
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

/// <summary>Padding values mirroring Chart.js padding (number or per-side).</summary>
public readonly record struct Padding(double Top, double Right, double Bottom, double Left)
{
    public static Padding All(double v) => new(v, v, v, v);
    public static Padding Symmetric(double vertical, double horizontal) => new(vertical, horizontal, vertical, horizontal);
    public double Vertical => Top + Bottom;
    public double Horizontal => Left + Right;

    public static implicit operator Padding(double v) => All(v);
}
