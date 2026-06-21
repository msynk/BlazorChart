
namespace BlazorChart;

/// <summary>The full computed scene the component renders.</summary>
public sealed class BlazorChartScene
{
    public double Width { get; set; }
    public double Height { get; set; }

    public List<BlazorChartSvgNode> Background { get; } = new();
    /// <summary>Series paths (lines + area fills) drawn above the grid and below the data points.
    /// Rendered in an animated group so every line — solid, dashed or per-segment — animates uniformly.</summary>
    public List<BlazorChartSvgNode> Series { get; } = new();
    public List<BlazorChartDataElement> Elements { get; } = new();
    public List<BlazorChartSvgNode> Foreground { get; } = new();

    /// <summary>Gradient definitions referenced via url(#id).</summary>
    public List<BlazorChartGradientDef> Defs { get; } = new();

    /// <summary>Pattern definitions referenced via url(#id).</summary>
    public List<BlazorChartPatternDef> Patterns { get; } = new();

    public BlazorChartLegendModel? Legend { get; set; }
    public BlazorChartTitleModel? Title { get; set; }
    public BlazorChartTitleModel? Subtitle { get; set; }

    /// <summary>The cartesian plotting area (null for circular/radar charts).</summary>
    public BlazorChartArea? PlotArea { get; set; }
    /// <summary>True for pie/doughnut/polar/radar charts.</summary>
    public bool IsRadialOrCircular { get; set; }
    /// <summary>True when the cartesian chart contains bar datasets.</summary>
    public bool HasBars { get; set; }
    /// <summary>True when bars are horizontal (indexAxis = y).</summary>
    public bool HorizontalBars { get; set; }
    /// <summary>True when the line series should animate with a progressive draw-on (stroke reveal,
    /// left to right) rather than the default group rise. Set by the renderer for line/area charts
    /// when <see cref="BlazorChartAnimationOptions.Progressive"/> is enabled.</summary>
    public bool ProgressiveDraw { get; set; }
    /// <summary>The value-axis baseline pixel the bars grow from (y for vertical bars, x for
    /// horizontal bars). Used as the transform-origin for the bar entry animation so they scale
    /// out of the axis line rather than the edge of the SVG.</summary>
    public double BarBaseline { get; set; }

    /// <summary>Effective value range per axis id after data/zoom resolution.</summary>
    public Dictionary<string, (double Min, double Max)> AxisRanges { get; } = new();
    /// <summary>Axis ids that support zoom/pan (linear/time/logarithmic).</summary>
    public HashSet<string> ZoomableAxes { get; } = new();
}
