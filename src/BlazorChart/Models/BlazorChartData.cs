namespace BlazorChart.Models;

/// <summary>The chart data, mirroring Chart.js <c>data</c>: labels + datasets.</summary>
public sealed class BlazorChartData
{
    public List<string> Labels { get; set; } = new();
    public List<BlazorChartDataset> Datasets { get; set; } = new();
}
