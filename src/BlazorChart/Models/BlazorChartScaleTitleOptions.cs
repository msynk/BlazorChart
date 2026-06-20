namespace BlazorChart.Models;

/// <summary>Axis title configuration.</summary>
public sealed class BlazorChartScaleTitleOptions
{
    public bool Display { get; set; }
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#666";
    public BlazorChartFont Font { get; set; } = new() { Weight = "bold" };
    public BlazorChartPadding Padding { get; set; } = new(4, 0, 4, 0);
}
