using System.Globalization;

namespace BlazorChart.Rendering;

/// <summary>Base class for renderable SVG primitives produced by the renderer.</summary>
public abstract class SvgNode
{
    public string? Title { get; set; }
    public double Opacity { get; set; } = 1;
    public string? CssClass { get; set; }
}

public sealed class SvgLine : SvgNode
{
    public double X1, Y1, X2, Y2;
    public string Stroke = "#000";
    public double StrokeWidth = 1;
    public string? Dash;
}

public sealed class SvgRect : SvgNode
{
    public double X, Y, Width, Height;
    public string Fill = "none";
    public string? Stroke;
    public double StrokeWidth = 0;
    public double Rx;
}

public sealed class SvgCircle : SvgNode
{
    public double Cx, Cy, R;
    public string Fill = "#000";
    public string? Stroke;
    public double StrokeWidth = 0;
}

public sealed class SvgPath : SvgNode
{
    public string D = "";
    public string Fill = "none";
    public string? Stroke;
    public double StrokeWidth = 0;
    public string? Dash;
    public string LineCap = "butt";
    public string LineJoin = "miter";
    /// <summary>When true the path animates a "draw-on" effect via stroke-dashoffset.</summary>
    public bool AnimateDraw;
    /// <summary>When true the path fades in (used where draw-on is unavailable, e.g. dashed/segmented lines).</summary>
    public bool AnimateFade;
}

public sealed class SvgPolygon : SvgNode
{
    public List<(double X, double Y)> Points = new();
    public string Fill = "none";
    public string? Stroke;
    public double StrokeWidth = 0;
    public bool Closed = true;
}

public sealed class SvgText : SvgNode
{
    public double X, Y;
    public string Text = "";
    public string Fill = "#000";
    public string FontFamily = "sans-serif";
    public double FontSize = 12;
    public string FontWeight = "normal";
    public string FontStyle = "normal";
    /// <summary>start | middle | end</summary>
    public string Anchor = "start";
    /// <summary>auto | middle | hanging | central</summary>
    public string Baseline = "auto";
    public double Rotation;
}

/// <summary>Helpers for formatting numbers in an invariant, SVG-friendly way.</summary>
public static class Svg
{
    public static string N(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
        return Math.Round(v, 3).ToString(CultureInfo.InvariantCulture);
    }

    public static string Dash(IEnumerable<double>? dash) =>
        dash is null ? "" : string.Join(",", dash.Select(N));
}
