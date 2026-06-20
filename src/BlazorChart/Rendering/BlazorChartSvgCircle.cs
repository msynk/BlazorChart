namespace BlazorChart.Rendering;

public sealed class BlazorChartSvgCircle : BlazorChartSvgNode
{
    public double Cx, Cy, R;
    public string Fill = "#000";
    public string? Stroke;
    public double StrokeWidth = 0;
}
