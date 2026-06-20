namespace BlazorChart.Models;

/// <summary>Title / subtitle plugin options.</summary>
public sealed class BlazorChartTitleOptions
{
    public bool Display { get; set; }
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#333";
    public BlazorChartPosition Position { get; set; } = BlazorChartPosition.Top;
    public BlazorChartAlign Align { get; set; } = BlazorChartAlign.Center;
    public BlazorChartFont Font { get; set; } = new() { Size = 16, Weight = "bold" };
    public BlazorChartPadding Padding { get; set; } = 10;
}
