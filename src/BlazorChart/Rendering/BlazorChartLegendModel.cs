
namespace BlazorChart;

public sealed class BlazorChartLegendModel
{
    public List<BlazorChartLegendItemModel> Items { get; set; } = new();
    public BlazorChartPosition Position { get; set; } = BlazorChartPosition.Top;
    public BlazorChartAlign Align { get; set; } = BlazorChartAlign.Center;
    public BlazorChartLegendLabelOptions Labels { get; set; } = new();
    public string? Title { get; set; }
    public bool OnClickToggle { get; set; } = true;
}
