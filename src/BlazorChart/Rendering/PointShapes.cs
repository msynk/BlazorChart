using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>Builds SVG marker shapes for the Chart.js point styles.</summary>
public static class PointShapes
{
    public static SvgNode? Build(PointStyle style, double x, double y, double r,
        string fill, string stroke, double strokeWidth)
    {
        switch (style)
        {
            case PointStyle.None:
                return null;
            case PointStyle.Rect:
                return new SvgRect { X = x - r, Y = y - r, Width = r * 2, Height = r * 2, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
            case PointStyle.RectRounded:
                return new SvgRect { X = x - r, Y = y - r, Width = r * 2, Height = r * 2, Rx = r * 0.4, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
            case PointStyle.Triangle:
                return new SvgPolygon
                {
                    Points = { (x, y - r), (x - r, y + r), (x + r, y + r) },
                    Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth
                };
            case PointStyle.RectRot:
                return new SvgPolygon
                {
                    Points = { (x, y - r), (x + r, y), (x, y + r), (x - r, y) },
                    Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth
                };
            case PointStyle.Cross:
                return new SvgPath { D = $"M {Svg.N(x)} {Svg.N(y - r)} L {Svg.N(x)} {Svg.N(y + r)} M {Svg.N(x - r)} {Svg.N(y)} L {Svg.N(x + r)} {Svg.N(y)}", Stroke = stroke, StrokeWidth = Math.Max(1, strokeWidth) };
            case PointStyle.CrossRot:
            {
                double o = r * 0.707;
                return new SvgPath { D = $"M {Svg.N(x - o)} {Svg.N(y - o)} L {Svg.N(x + o)} {Svg.N(y + o)} M {Svg.N(x - o)} {Svg.N(y + o)} L {Svg.N(x + o)} {Svg.N(y - o)}", Stroke = stroke, StrokeWidth = Math.Max(1, strokeWidth) };
            }
            case PointStyle.Dash:
            case PointStyle.Line:
                return new SvgPath { D = $"M {Svg.N(x - r)} {Svg.N(y)} L {Svg.N(x + r)} {Svg.N(y)}", Stroke = stroke, StrokeWidth = Math.Max(2, strokeWidth) };
            case PointStyle.Star:
                return BuildStar(x, y, r, fill, stroke, strokeWidth);
            default:
                return new SvgCircle { Cx = x, Cy = y, R = r, Fill = fill, Stroke = stroke, StrokeWidth = strokeWidth };
        }
    }

    private static SvgPolygon BuildStar(double cx, double cy, double r, string fill, string stroke, double sw)
    {
        var poly = new SvgPolygon { Fill = fill, Stroke = stroke, StrokeWidth = sw };
        for (int i = 0; i < 10; i++)
        {
            double rad = i % 2 == 0 ? r : r / 2;
            double ang = Math.PI / 5 * i - Math.PI / 2;
            poly.Points.Add((cx + Math.Cos(ang) * rad, cy + Math.Sin(ang) * rad));
        }
        return poly;
    }
}
