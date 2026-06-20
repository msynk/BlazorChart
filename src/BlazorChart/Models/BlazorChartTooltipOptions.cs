namespace BlazorChart;

/// <summary>Tooltip plugin options.</summary>
public sealed class BlazorChartTooltipOptions
{
    public bool Enabled { get; set; } = true;
    public BlazorChartInteractionMode Mode { get; set; } = BlazorChartInteractionMode.Nearest;
    public bool Intersect { get; set; } = true;
    /// <summary>Where the tooltip is anchored when multiple items are active.</summary>
    public BlazorChartTooltipPositioner Position { get; set; } = BlazorChartTooltipPositioner.Average;
    public string BackgroundColor { get; set; } = "rgba(0,0,0,0.8)";
    public string TitleColor { get; set; } = "#fff";
    public string BodyColor { get; set; } = "#fff";
    public string FooterColor { get; set; } = "#fff";
    public BlazorChartFont TitleFont { get; set; } = new() { Weight = "bold" };
    public BlazorChartFont BodyFont { get; set; } = new();
    public BlazorChartFont FooterFont { get; set; } = new() { Weight = "bold" };
    public double Padding { get; set; } = 6;
    public double CornerRadius { get; set; } = 6;
    public bool DisplayColors { get; set; } = true;
    /// <summary>Render the color swatch using the dataset point style instead of a square.</summary>
    public bool UsePointStyle { get; set; }
    /// <summary>Border color of the tooltip box.</summary>
    public string? BorderColor { get; set; }
    /// <summary>Border width of the tooltip box.</summary>
    public double BorderWidth { get; set; }
    /// <summary>Text alignment of the title (left/center/right).</summary>
    public BlazorChartAlign TitleAlign { get; set; } = BlazorChartAlign.Start;
    /// <summary>Text alignment of the body (left/center/right).</summary>
    public BlazorChartAlign BodyAlign { get; set; } = BlazorChartAlign.Start;
    /// <summary>Rich text/styling callbacks.</summary>
    public BlazorChartTooltipCallbacks Callbacks { get; set; } = new();
    /// <summary>Optional label formatter: (datasetLabel, value) => text. Shorthand for <c>Callbacks.Label</c>.</summary>
    public Func<string, double, string>? LabelFormatter { get; set; }
}
