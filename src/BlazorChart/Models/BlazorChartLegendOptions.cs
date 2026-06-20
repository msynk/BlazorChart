namespace BlazorChart;

/// <summary>Legend plugin options.</summary>
public sealed class BlazorChartLegendOptions
{
    public bool Display { get; set; } = true;
    public BlazorChartPosition Position { get; set; } = BlazorChartPosition.Top;
    public BlazorChartAlign Align { get; set; } = BlazorChartAlign.Center;
    public bool Reverse { get; set; }
    /// <summary>Allow clicking a legend item to toggle dataset/data visibility.</summary>
    public bool OnClickToggle { get; set; } = true;
    public BlazorChartLegendLabelOptions Labels { get; set; } = new();
    public string? Title { get; set; }
}
