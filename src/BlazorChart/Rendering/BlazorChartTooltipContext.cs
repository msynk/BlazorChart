namespace BlazorChart.Rendering;

/// <summary>Context passed to a custom tooltip template (<c>TooltipTemplate</c>).</summary>
public sealed class BlazorChartTooltipContext
{
    public string? Title { get; init; }
    public IReadOnlyList<BlazorChartTooltipPoint> Points { get; init; } = Array.Empty<BlazorChartTooltipPoint>();
}
