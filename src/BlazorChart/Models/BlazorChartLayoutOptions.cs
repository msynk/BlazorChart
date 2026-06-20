namespace BlazorChart.Models;

/// <summary>Layout options (chart padding), mirroring Chart.js <c>options.layout</c>.</summary>
public sealed class BlazorChartLayoutOptions
{
    public BlazorChartPadding Padding { get; set; } = 4;
}
