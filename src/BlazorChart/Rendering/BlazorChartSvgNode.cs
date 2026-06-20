namespace BlazorChart;

/// <summary>Base class for renderable SVG primitives produced by the renderer.</summary>
public abstract class BlazorChartSvgNode
{
    public string? Title { get; set; }
    public double Opacity { get; set; } = 1;
    public string? CssClass { get; set; }
}
