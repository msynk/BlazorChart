using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>Builds SVG marker shapes for the Chart.js point styles.</summary>
public static class BlazorChartPointShapes
{
    public static BlazorChartSvgNode? Build(BlazorChartPointStyle style, double x, double y, double r,
        string fill, string stroke, double strokeWidth)
    {
        switch (style)
        {
            case BlazorChartPointStyle.None:
                return null;
            case BlazorChartPointStyle.Rect:
                return new BlazorChartSvgRect { X = x - r, Y = y - r, Width = r * 2, Height = r * 2, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
            case BlazorChartPointStyle.RectRounded:
                return new BlazorChartSvgRect { X = x - r, Y = y - r, Width = r * 2, Height = r * 2, Rx = r * 0.4, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
            case BlazorChartPointStyle.Triangle:
                return new BlazorChartSvgPolygon
                {
                    Points = { (x, y - r), (x - r, y + r), (x + r, y + r) },
                    Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth
                };
            case BlazorChartPointStyle.RectRot:
                return new BlazorChartSvgPolygon
                {
                    Points = { (x, y - r), (x + r, y), (x, y + r), (x - r, y) },
                    Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth
                };
            case BlazorChartPointStyle.Cross:
                return new BlazorChartSvgPath { D = $"M {BlazorChartSvg.N(x)} {BlazorChartSvg.N(y - r)} L {BlazorChartSvg.N(x)} {BlazorChartSvg.N(y + r)} M {BlazorChartSvg.N(x - r)} {BlazorChartSvg.N(y)} L {BlazorChartSvg.N(x + r)} {BlazorChartSvg.N(y)}", Stroke = stroke, StrokeWidth = Math.Max(1, strokeWidth) };
            case BlazorChartPointStyle.CrossRot:
            {
                double o = r * 0.707;
                return new BlazorChartSvgPath { D = $"M {BlazorChartSvg.N(x - o)} {BlazorChartSvg.N(y - o)} L {BlazorChartSvg.N(x + o)} {BlazorChartSvg.N(y + o)} M {BlazorChartSvg.N(x - o)} {BlazorChartSvg.N(y + o)} L {BlazorChartSvg.N(x + o)} {BlazorChartSvg.N(y - o)}", Stroke = stroke, StrokeWidth = Math.Max(1, strokeWidth) };
            }
            case BlazorChartPointStyle.Dash:
            case BlazorChartPointStyle.Line:
                return new BlazorChartSvgPath { D = $"M {BlazorChartSvg.N(x - r)} {BlazorChartSvg.N(y)} L {BlazorChartSvg.N(x + r)} {BlazorChartSvg.N(y)}", Stroke = stroke, StrokeWidth = Math.Max(2, strokeWidth) };
            case BlazorChartPointStyle.Star:
                return BuildStar(x, y, r, fill, stroke, strokeWidth);
            default:
                return new BlazorChartSvgCircle { Cx = x, Cy = y, R = r, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
        }
    }

    private static BlazorChartSvgPolygon BuildStar(double cx, double cy, double r, string fill, string stroke, double sw)
    {
        var poly = new BlazorChartSvgPolygon { Fill = fill, Stroke = stroke, StrokeWidth = sw };
        for (int i = 0; i < 10; i++)
        {
            double rad = i % 2 == 0 ? r : r / 2;
            double ang = Math.PI / 5 * i - Math.PI / 2;
            poly.Points.Add((cx + Math.Cos(ang) * rad, cy + Math.Sin(ang) * rad));
        }
        return poly;
    }
}
