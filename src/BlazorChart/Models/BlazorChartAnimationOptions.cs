namespace BlazorChart;

/// <summary>Animation options. For the SVG renderer these map to CSS transitions.</summary>
public sealed class BlazorChartAnimationOptions
{
    public bool Animate { get; set; } = true;
    public int Duration { get; set; } = 600;
    public string Easing { get; set; } = "ease-out";
    /// <summary>Per-element entry delay (ms). When &gt; 0, elements animate in sequence (staggered).</summary>
    public double DelayBetween { get; set; }
}
