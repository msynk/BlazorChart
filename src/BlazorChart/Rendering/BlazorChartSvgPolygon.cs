namespace BlazorChart.Rendering;

public sealed class BlazorChartSvgPolygon : BlazorChartSvgNode
{
    public List<(double X, double Y)> Points = new();
    public string Fill = "none";
    public string? Stroke;
    public double StrokeWidth = 0;
    public bool Closed = true;
}
