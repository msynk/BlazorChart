namespace BlazorChart.Models;

/// <summary>Base type for gradient fills (linear or radial).</summary>
public abstract class BlazorChartGradientBase
{
    public List<BlazorChartGradientStop> Stops { get; set; } = new();
}
