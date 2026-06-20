namespace BlazorChart;

/// <summary>Interaction options, mirroring Chart.js <c>options.interaction</c>.</summary>
public sealed class BlazorChartInteractionOptions
{
    public BlazorChartInteractionMode Mode { get; set; } = BlazorChartInteractionMode.Nearest;
    public bool Intersect { get; set; } = true;
}
