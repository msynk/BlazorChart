using BlazorChart.Models;

namespace BlazorChart.Rendering.Plugins;

public enum AnnotationKind { Line, Box, Point, Label }
public enum LineOrientation { Horizontal, Vertical }

/// <summary>A single annotation (line, box, point or label).</summary>
public sealed class Annotation
{
    public AnnotationKind Kind { get; set; } = AnnotationKind.Line;

    // Line
    public LineOrientation Orientation { get; set; } = LineOrientation.Horizontal;
    /// <summary>Value on the relevant axis (y value for horizontal lines, x value/index for vertical).</summary>
    public double Value { get; set; }
    public string AxisId { get; set; } = "y";

    // Box / point bounds (value coordinates). Null = chart edge.
    public double? XMin { get; set; }
    public double? XMax { get; set; }
    public double? YMin { get; set; }
    public double? YMax { get; set; }
    /// <summary>For vertical line / box X bounds, interpret as a category index rather than a value.</summary>
    public bool XIsIndex { get; set; }

    public string Color { get; set; } = "#ff6384";
    public string? FillColor { get; set; }
    public double LineWidth { get; set; } = 2;
    public List<double>? Dash { get; set; }

    public string? Label { get; set; }
    public string LabelColor { get; set; } = "#fff";
    public string LabelBackground { get; set; } = "#ff6384";
    public bool DrawBehindDatasets { get; set; }
}

/// <summary>
/// Draws line/box/point/label annotations over a cartesian chart, mirroring the popular
/// chartjs-plugin-annotation.
/// </summary>
public sealed class AnnotationPlugin : IChartPlugin
{
    public string Id => "annotation";
    public List<Annotation> Annotations { get; } = new();

    public AnnotationPlugin() { }
    public AnnotationPlugin(params Annotation[] annotations) => Annotations.AddRange(annotations);

    public void BeforeDatasetsDraw(PluginContext ctx)
    {
        foreach (var a in Annotations.Where(a => a.DrawBehindDatasets))
            Draw(ctx, a, behind: true);
    }

    public void AfterDatasetsDraw(PluginContext ctx)
    {
        foreach (var a in Annotations.Where(a => !a.DrawBehindDatasets))
            Draw(ctx, a, behind: false);
    }

    private void Draw(PluginContext ctx, Annotation a, bool behind)
    {
        if (!ctx.IsCartesian || ctx.Plot is not { } plot) return;
        Action<SvgNode> add = behind ? ctx.AddBehind : ctx.AddFront;

        double X(double v, bool isIndex) => isIndex ? ctx.XForIndex((int)v) : ctx.XForValue(v);

        switch (a.Kind)
        {
            case AnnotationKind.Line:
                if (a.Orientation == LineOrientation.Horizontal)
                {
                    double y = ctx.YForValue(a.Value, a.AxisId);
                    add(new SvgLine { X1 = plot.Left, Y1 = y, X2 = plot.Right, Y2 = y, Stroke = a.Color, StrokeWidth = a.LineWidth, Dash = Svg.Dash(a.Dash) });
                    if (!string.IsNullOrEmpty(a.Label))
                        AddLabel(add, a, plot.Right - 4, y, "end");
                }
                else
                {
                    double x = X(a.Value, a.XIsIndex);
                    add(new SvgLine { X1 = x, Y1 = plot.Top, X2 = x, Y2 = plot.Bottom, Stroke = a.Color, StrokeWidth = a.LineWidth, Dash = Svg.Dash(a.Dash) });
                    if (!string.IsNullOrEmpty(a.Label))
                        AddLabel(add, a, x, plot.Top + 10, "middle");
                }
                break;

            case AnnotationKind.Box:
            {
                double x1 = a.XMin is { } xm ? X(xm, a.XIsIndex) : plot.Left;
                double x2 = a.XMax is { } xM ? X(xM, a.XIsIndex) : plot.Right;
                double y1 = a.YMax is { } yM ? ctx.YForValue(yM, a.AxisId) : plot.Top;
                double y2 = a.YMin is { } ym ? ctx.YForValue(ym, a.AxisId) : plot.Bottom;
                add(new SvgRect
                {
                    X = Math.Min(x1, x2), Y = Math.Min(y1, y2),
                    Width = Math.Abs(x2 - x1), Height = Math.Abs(y2 - y1),
                    Fill = a.FillColor ?? ColorUtil.WithAlpha(a.Color, 0.15),
                    Stroke = a.Color, StrokeWidth = a.LineWidth
                });
                if (!string.IsNullOrEmpty(a.Label))
                    AddLabel(add, a, (x1 + x2) / 2, (y1 + y2) / 2, "middle");
                break;
            }

            case AnnotationKind.Point:
            {
                double x = a.XMin is { } xv ? X(xv, a.XIsIndex) : plot.CenterX;
                double y = ctx.YForValue(a.Value, a.AxisId);
                add(new SvgCircle { Cx = x, Cy = y, R = a.LineWidth * 2 + 2, Fill = a.FillColor ?? a.Color, Stroke = a.Color, StrokeWidth = a.LineWidth });
                break;
            }

            case AnnotationKind.Label:
            {
                double x = a.XMin is { } xv ? X(xv, a.XIsIndex) : plot.CenterX;
                double y = ctx.YForValue(a.Value, a.AxisId);
                AddLabel(add, a, x, y, "middle");
                break;
            }
        }
    }

    private static void AddLabel(Action<SvgNode> add, Annotation a, double x, double y, string anchor)
    {
        double w = (a.Label!.Length * 7) + 10;
        add(new SvgRect { X = anchor == "end" ? x - w : anchor == "middle" ? x - w / 2 : x, Y = y - 9, Width = w, Height = 18, Rx = 4, Fill = a.LabelBackground });
        add(new SvgText { X = anchor == "end" ? x - w / 2 : x, Y = y, Text = a.Label!, Fill = a.LabelColor, FontSize = 11, Anchor = "middle", Baseline = "central", FontWeight = "bold" });
    }
}
