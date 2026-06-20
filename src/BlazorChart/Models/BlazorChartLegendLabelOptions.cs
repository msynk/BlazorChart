namespace BlazorChart.Models;

/// <summary>Legend label configuration.</summary>
public sealed class BlazorChartLegendLabelOptions
{
    public string Color { get; set; } = "#666";
    public BlazorChartFont Font { get; set; } = new();
    public double BoxWidth { get; set; } = 40;
    public double BoxHeight { get; set; } = 12;
    public double Padding { get; set; } = 10;
    public bool UsePointStyle { get; set; }
    public BlazorChartPointStyle PointStyle { get; set; } = BlazorChartPointStyle.Circle;
}
