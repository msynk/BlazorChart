namespace BlazorChart;

public sealed class BlazorChartSvgRect : BlazorChartSvgNode
{
    public double X, Y, Width, Height;
    public string Fill = "none";
    public string? Stroke;
    public double StrokeWidth = 0;
    public double Rx;
}
