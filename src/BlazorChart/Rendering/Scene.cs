using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>A registered gradient definition referenced by id.</summary>
public sealed record GradientDef(string Id, GradientBase Gradient);

/// <summary>A registered pattern definition referenced by id.</summary>
public sealed record PatternDef(string Id, FillPattern Pattern);

/// <summary>Tooltip payload attached to an interactive data element.</summary>
public sealed class TooltipInfo
{
    public string? Title { get; set; }
    public List<TooltipItem> Items { get; set; } = new();
    /// <summary>Lines rendered before the body (from the BeforeBody callback).</summary>
    public List<string> BeforeBody { get; set; } = new();
    /// <summary>Lines rendered after the body (from the AfterBody callback).</summary>
    public List<string> AfterBody { get; set; } = new();
    /// <summary>Footer lines (from the Footer callback).</summary>
    public List<string> Footer { get; set; } = new();
    /// <summary>Anchor position (in chart pixel coordinates) the tooltip points at.</summary>
    public double AnchorX { get; set; }
    public double AnchorY { get; set; }
}

public sealed class TooltipItem
{
    public string Color { get; set; } = "#000";
    public string Text { get; set; } = "";
    /// <summary>Optional point style for the tooltip swatch (when tooltip usePointStyle is enabled).</summary>
    public Models.PointStyle? PointStyle { get; set; }
}

/// <summary>An interactive data-driven element (bar, point, arc, ...).</summary>
public sealed class DataElement
{
    public required SvgNode Shape { get; init; }
    public int DatasetIndex { get; init; }
    public int DataIndex { get; init; }
    public TooltipInfo Tooltip { get; init; } = new();
    /// <summary>Hit-test centroid in chart pixel coordinates.</summary>
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    /// <summary>Optional shape used when this element is the active/hovered one.</summary>
    public SvgNode? HoverShape { get; init; }
    /// <summary>Optional secondary shape drawn with the element (e.g. a bar's skipped-edge border)
    /// so it animates together with the fill.</summary>
    public SvgNode? BorderShape { get; init; }
    /// <summary>Per-element entry animation CSS class (e.g. grow from baseline for bars, pop for points).</summary>
    public string? EnterAnim { get; init; }
    /// <summary>Transform origin (in view-box/pixel coordinates) for the entry animation — the bar baseline
    /// or the point center.</summary>
    public double AnimOriginX { get; init; }
    public double AnimOriginY { get; init; }
    /// <summary>Primary numeric value of this element (for tooltip templates).</summary>
    public double Value { get; init; }
    /// <summary>Series (dataset) label, for tooltip templates.</summary>
    public string? SeriesLabel { get; init; }
}

/// <summary>A single item exposed to a custom tooltip template.</summary>
public sealed class TooltipPoint
{
    public int DatasetIndex { get; init; }
    public int DataIndex { get; init; }
    public string? Label { get; init; }
    public double Value { get; init; }
    public string Color { get; init; } = "#000";
    public string FormattedValue { get; init; } = "";
}

/// <summary>Context passed to a custom tooltip template (<c>TooltipTemplate</c>).</summary>
public sealed class TooltipContext
{
    public string? Title { get; init; }
    public IReadOnlyList<TooltipPoint> Points { get; init; } = Array.Empty<TooltipPoint>();
}

/// <summary>A legend entry.</summary>
public sealed class LegendItemModel
{
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#000";
    public string? StrokeColor { get; set; }
    public bool Hidden { get; set; }
    public bool UsePointStyle { get; set; }
    public PointStyle PointStyle { get; set; }
    /// <summary>Dataset index, or for pie/doughnut/polar the data index.</summary>
    public int Index { get; set; }
    public bool IsDataIndex { get; set; }
}

public sealed class LegendModel
{
    public List<LegendItemModel> Items { get; set; } = new();
    public Position Position { get; set; } = Position.Top;
    public Align Align { get; set; } = Align.Center;
    public LegendLabelOptions Labels { get; set; } = new();
    public string? Title { get; set; }
    public bool OnClickToggle { get; set; } = true;
}

public sealed class TitleModel
{
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#333";
    public Position Position { get; set; } = Position.Top;
    public Align Align { get; set; } = Align.Center;
    public ChartFont Font { get; set; } = new();
}

/// <summary>The full computed scene the component renders.</summary>
public sealed class ChartScene
{
    public double Width { get; set; }
    public double Height { get; set; }

    public List<SvgNode> Background { get; } = new();
    /// <summary>Series paths (lines + area fills) drawn above the grid and below the data points.
    /// Rendered in an animated group so every line — solid, dashed or per-segment — animates uniformly.</summary>
    public List<SvgNode> Series { get; } = new();
    public List<DataElement> Elements { get; } = new();
    public List<SvgNode> Foreground { get; } = new();

    /// <summary>Gradient definitions referenced via url(#id).</summary>
    public List<GradientDef> Defs { get; } = new();

    /// <summary>Pattern definitions referenced via url(#id).</summary>
    public List<PatternDef> Patterns { get; } = new();

    public LegendModel? Legend { get; set; }
    public TitleModel? Title { get; set; }
    public TitleModel? Subtitle { get; set; }

    /// <summary>The cartesian plotting area (null for circular/radar charts).</summary>
    public Area? PlotArea { get; set; }
    /// <summary>True for pie/doughnut/polar/radar charts.</summary>
    public bool IsRadialOrCircular { get; set; }
    /// <summary>True when the cartesian chart contains bar datasets.</summary>
    public bool HasBars { get; set; }
    /// <summary>True when bars are horizontal (indexAxis = y).</summary>
    public bool HorizontalBars { get; set; }

    /// <summary>Effective value range per axis id after data/zoom resolution.</summary>
    public Dictionary<string, (double Min, double Max)> AxisRanges { get; } = new();
    /// <summary>Axis ids that support zoom/pan (linear/time/logarithmic).</summary>
    public HashSet<string> ZoomableAxes { get; } = new();
}
