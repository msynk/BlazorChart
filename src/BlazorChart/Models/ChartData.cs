namespace BlazorChart.Models;

/// <summary>A single (x, y) or (x, y, r) data point used by scatter and bubble charts.</summary>
public readonly record struct DataPoint(double X, double Y, double? R = null);

/// <summary>Per-corner radius for a bar, mirroring Chart.js <c>borderRadius</c> object form.</summary>
public readonly record struct BorderRadiusCorners(
    double TopLeft, double TopRight, double BottomRight, double BottomLeft)
{
    public static implicit operator BorderRadiusCorners(double all) => new(all, all, all, all);
    /// <summary>Rounds only the two "top" (away-from-baseline) corners.</summary>
    public static BorderRadiusCorners Top(double r) => new(r, r, 0, 0);
}

/// <summary>The chart data, mirroring Chart.js <c>data</c>: labels + datasets.</summary>
public sealed class ChartData
{
    public List<string> Labels { get; set; } = new();
    public List<ChartDataset> Datasets { get; set; } = new();
}

/// <summary>
/// A dataset, mirroring Chart.js dataset configuration. A dataset can carry either a
/// simple list of values (<see cref="Data"/>) for value based charts, or a list of
/// <see cref="DataPoint"/> (<see cref="Points"/>) for scatter/bubble charts.
/// </summary>
public sealed class ChartDataset
{
    /// <summary>Dataset label shown in legends and tooltips.</summary>
    public string? Label { get; set; }

    /// <summary>Per-index values (line, bar, radar, pie, doughnut, polarArea).</summary>
    public List<double?> Data { get; set; } = new();

    /// <summary>Floating-bar ranges [low, high] per index. When set, bars span low→high.</summary>
    public List<(double Low, double High)?>? RangeData { get; set; }

    /// <summary>Point data for scatter/bubble charts. When set, takes precedence over <see cref="Data"/>.</summary>
    public List<DataPoint>? Points { get; set; }

    /// <summary>Optional per-dataset type override for mixed charts.</summary>
    public ChartType? Type { get; set; }

    // ---- Colors (single value applies to the whole dataset; list applies per data index) ----
    public string? BackgroundColor { get; set; }
    public List<string>? BackgroundColors { get; set; }
    /// <summary>A repeating pattern fill, used by bars and area fills (overrides solid color).</summary>
    public FillPattern? BackgroundPattern { get; set; }
    public string? BorderColor { get; set; }
    public List<string>? BorderColors { get; set; }
    public double BorderWidth { get; set; } = 1;

    public string? HoverBackgroundColor { get; set; }
    public string? HoverBorderColor { get; set; }
    public double? HoverBorderWidth { get; set; }

    // ---- Scriptable options (evaluated per element; take precedence over the constants above) ----
    /// <summary>Scriptable background color: <c>ctx => color</c>.</summary>
    public Func<ScriptableContext, string?>? BackgroundColorFn { get; set; }
    /// <summary>Scriptable border color: <c>ctx => color</c>.</summary>
    public Func<ScriptableContext, string?>? BorderColorFn { get; set; }
    /// <summary>Scriptable border width: <c>ctx => width</c>.</summary>
    public Func<ScriptableContext, double?>? BorderWidthFn { get; set; }
    /// <summary>Scriptable point radius: <c>ctx => radius</c>.</summary>
    public Func<ScriptableContext, double?>? PointRadiusFn { get; set; }
    /// <summary>Scriptable point background color.</summary>
    public Func<ScriptableContext, string?>? PointBackgroundColorFn { get; set; }
    /// <summary>Scriptable point border color.</summary>
    public Func<ScriptableContext, string?>? PointBorderColorFn { get; set; }
    /// <summary>Scriptable point style.</summary>
    public Func<ScriptableContext, PointStyle?>? PointStyleFn { get; set; }

    // ---- Line / radar element options ----
    public FillMode Fill { get; set; } = FillMode.None;
    public string? FillColor { get; set; }
    /// <summary>Optional gradient (linear or radial) used for the area fill (takes precedence over <see cref="FillColor"/>).</summary>
    public GradientBase? FillGradient { get; set; }
    /// <summary>Target dataset index when <see cref="Fill"/> is <see cref="FillMode.Dataset"/>.</summary>
    public int? FillTargetIndex { get; set; }
    /// <summary>Bezier curve tension (0 = straight lines).</summary>
    public double Tension { get; set; }
    /// <summary>Cubic interpolation mode. <see cref="CubicInterpolationMode.Monotone"/> avoids overshoot.</summary>
    public CubicInterpolationMode CubicInterpolationMode { get; set; } = CubicInterpolationMode.Default;
    public SteppedLine Stepped { get; set; } = SteppedLine.False;
    public List<double>? BorderDash { get; set; }
    public string BorderJoinStyle { get; set; } = "round";
    public string BorderCapStyle { get; set; } = "round";
    public bool ShowLine { get; set; } = true;
    public bool SpanGaps { get; set; }
    /// <summary>Optional per-segment styling callbacks (color/width/dash per line segment).</summary>
    public LineSegmentStyle? Segment { get; set; }
    /// <summary>When <see cref="Fill"/> is <see cref="FillMode.Value"/>, the absolute axis value to fill to.</summary>
    public double? FillValue { get; set; }

    // ---- Point element options ----
    public double PointRadius { get; set; } = 3;
    public double PointHoverRadius { get; set; } = 4;
    public double PointBorderWidth { get; set; } = 1;
    public string? PointBackgroundColor { get; set; }
    public string? PointBorderColor { get; set; }
    public PointStyle PointStyle { get; set; } = PointStyle.Circle;
    /// <summary>Pixel radius around a point that still counts as a hit for hover/tooltip.</summary>
    public double HitRadius { get; set; } = 1;
    /// <summary>Point fill color when hovered (falls back to <see cref="PointBackgroundColor"/>).</summary>
    public string? PointHoverBackgroundColor { get; set; }
    /// <summary>Point border color when hovered.</summary>
    public string? PointHoverBorderColor { get; set; }
    /// <summary>Point border width when hovered.</summary>
    public double? PointHoverBorderWidth { get; set; }

    // ---- Bar element options ----
    public double? BarThickness { get; set; }
    public double? MaxBarThickness { get; set; }
    public double BarPercentage { get; set; } = 0.9;
    public double CategoryPercentage { get; set; } = 0.8;
    public double BorderRadius { get; set; }
    /// <summary>Optional per-corner bar radius. When set, overrides <see cref="BorderRadius"/>.</summary>
    public BorderRadiusCorners? BorderRadiusCorners { get; set; }
    /// <summary>Pixels to grow each bar by to avoid anti-aliasing gaps between stacked bars.</summary>
    public double? InflateAmount { get; set; }
    /// <summary>Which bar edge omits its border. Default skips the baseline edge.</summary>
    public BorderSkipped BorderSkipped { get; set; } = BorderSkipped.Start;

    // ---- Arc (pie/doughnut/polar) element options ----
    public double Offset { get; set; }
    public double SpacingArc { get; set; }

    // ---- Axis assignment / stacking / ordering ----
    public string XAxisID { get; set; } = "x";
    public string YAxisID { get; set; } = "y";
    public string RAxisID { get; set; } = "r";
    public string? Stack { get; set; }
    public int Order { get; set; }
    public bool Hidden { get; set; }

    /// <summary>Number of logical data items in this dataset.</summary>
    public int Count => Points?.Count ?? RangeData?.Count ?? Data.Count;
}
