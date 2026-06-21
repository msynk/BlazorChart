
namespace BlazorChart;

/// <summary>
/// Draws line/box/point/label annotations over a cartesian chart, mirroring the popular
/// chartjs-plugin-annotation.
/// </summary>
public sealed class BlazorChartAnnotationPlugin : IBlazorChartPlugin
{
    public string Id => "annotation";
    public List<BlazorChartAnnotation> Annotations { get; } = new();

    public BlazorChartAnnotationPlugin() { }
    public BlazorChartAnnotationPlugin(params BlazorChartAnnotation[] annotations) => Annotations.AddRange(annotations);

    public void BeforeDatasetsDraw(BlazorChartPluginContext ctx)
    {
        foreach (var a in Annotations.Where(a => a.DrawBehindDatasets))
            Draw(ctx, a, behind: true);
    }

    public void AfterDatasetsDraw(BlazorChartPluginContext ctx)
    {
        foreach (var a in Annotations.Where(a => !a.DrawBehindDatasets))
            Draw(ctx, a, behind: false);
    }

    private void Draw(BlazorChartPluginContext ctx, BlazorChartAnnotation a, bool behind)
    {
        if (!ctx.IsCartesian || ctx.Plot is not { } plot) return;
        Action<BlazorChartSvgNode> add = behind ? ctx.AddBehind : ctx.AddFront;

        double X(double v, bool isIndex) => isIndex ? ctx.XForIndex((int)v) : ctx.XForValue(v);

        switch (a.Kind)
        {
            case BlazorChartAnnotationKind.Line:
                if (a.Orientation == BlazorChartLineOrientation.Horizontal)
                {
                    double y = ctx.YForValue(a.Value, a.AxisId);
                    add(new BlazorChartSvgLine { X1 = plot.Left, Y1 = y, X2 = plot.Right, Y2 = y, Stroke = a.Color, StrokeWidth = a.LineWidth, Dash = BlazorChartSvg.Dash(a.Dash) });
                    if (!string.IsNullOrEmpty(a.Label))
                        AddLabel(add, a, plot.Right - 4, y, "end");
                }
                else
                {
                    double x = X(a.Value, a.XIsIndex);
                    add(new BlazorChartSvgLine { X1 = x, Y1 = plot.Top, X2 = x, Y2 = plot.Bottom, Stroke = a.Color, StrokeWidth = a.LineWidth, Dash = BlazorChartSvg.Dash(a.Dash) });
                    if (!string.IsNullOrEmpty(a.Label))
                        AddLabel(add, a, x, plot.Top + 10, "middle");
                }
                break;

            case BlazorChartAnnotationKind.Box:
            {
                double x1 = a.XMin is { } xm ? X(xm, a.XIsIndex) : plot.Left;
                double x2 = a.XMax is { } xM ? X(xM, a.XIsIndex) : plot.Right;
                double y1 = a.YMax is { } yM ? ctx.YForValue(yM, a.AxisId) : plot.Top;
                double y2 = a.YMin is { } ym ? ctx.YForValue(ym, a.AxisId) : plot.Bottom;

                // Clamp to the plot area so a box whose bounds fall outside the visible
                // axis range (e.g. YMin below the axis minimum) doesn't spill past the chart.
                x1 = Math.Clamp(x1, plot.Left, plot.Right);
                x2 = Math.Clamp(x2, plot.Left, plot.Right);
                y1 = Math.Clamp(y1, plot.Top, plot.Bottom);
                y2 = Math.Clamp(y2, plot.Top, plot.Bottom);

                add(new BlazorChartSvgRect
                {
                    X = Math.Min(x1, x2), Y = Math.Min(y1, y2),
                    Width = Math.Abs(x2 - x1), Height = Math.Abs(y2 - y1),
                    Fill = a.FillColor ?? BlazorChartColorUtil.WithAlpha(a.Color, 0.15),
                    Stroke = a.Color, StrokeWidth = a.LineWidth
                });
                if (!string.IsNullOrEmpty(a.Label))
                    AddLabel(add, a, (x1 + x2) / 2, (y1 + y2) / 2, "middle");
                break;
            }

            case BlazorChartAnnotationKind.Point:
            {
                double x = a.XMin is { } xv ? X(xv, a.XIsIndex) : plot.CenterX;
                double y = ctx.YForValue(a.Value, a.AxisId);
                add(new BlazorChartSvgCircle { Cx = x, Cy = y, R = a.LineWidth * 2 + 2, Fill = a.FillColor ?? a.Color, Stroke = a.Color, StrokeWidth = a.LineWidth });
                break;
            }

            case BlazorChartAnnotationKind.Label:
            {
                double x = a.XMin is { } xv ? X(xv, a.XIsIndex) : plot.CenterX;
                double y = ctx.YForValue(a.Value, a.AxisId);
                AddLabel(add, a, x, y, "middle");
                break;
            }
        }
    }

    private static void AddLabel(Action<BlazorChartSvgNode> add, BlazorChartAnnotation a, double x, double y, string anchor)
    {
        double w = (a.Label!.Length * 7) + 10;
        add(new BlazorChartSvgRect { X = anchor == "end" ? x - w : anchor == "middle" ? x - w / 2 : x, Y = y - 9, Width = w, Height = 18, Rx = 4, Fill = a.LabelBackground });
        add(new BlazorChartSvgText { X = anchor == "end" ? x - w / 2 : x, Y = y, Text = a.Label!, Fill = a.LabelColor, FontSize = 11, Anchor = "middle", Baseline = "central", FontWeight = "bold" });
    }
}
