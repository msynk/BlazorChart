using BlazorChart.Models;

namespace BlazorChart.Rendering;

public sealed class BlazorChartTitleModel
{
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#333";
    public BlazorChartPosition Position { get; set; } = BlazorChartPosition.Top;
    public BlazorChartAlign Align { get; set; } = BlazorChartAlign.Center;
    public BlazorChartFont Font { get; set; } = new();
}
