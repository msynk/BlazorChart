namespace BlazorChart.Models;

/// <summary>Axis border line configuration, mirroring Chart.js <c>scale.border</c>.</summary>
public sealed class AxisBorderOptions
{
    public bool Display { get; set; } = true;
    public string Color { get; set; } = "rgba(0,0,0,0.25)";
    public double Width { get; set; } = 1;
    public List<double>? Dash { get; set; }
}

/// <summary>Grid line configuration for a scale.</summary>
public sealed class GridOptions
{
    public bool Display { get; set; } = true;
    public string Color { get; set; } = "rgba(0,0,0,0.1)";
    public double LineWidth { get; set; } = 1;
    public bool DrawOnChartArea { get; set; } = true;
    public bool DrawTicks { get; set; } = true;
    public double TickLength { get; set; } = 6;
    public string TickColor { get; set; } = "rgba(0,0,0,0.1)";
    public List<double>? BorderDash { get; set; }
    /// <summary>Color of the zero line; when null the normal grid color is used.</summary>
    public string? ZeroLineColor { get; set; }
    /// <summary>Radial scales: draw grid as concentric circles instead of polygons.</summary>
    public bool Circular { get; set; }
}

/// <summary>Point label configuration for radial (radar/polar) scales.</summary>
public sealed class PointLabelOptions
{
    public bool Display { get; set; } = true;
    public string Color { get; set; } = "#666";
    public ChartFont Font { get; set; } = new() { Size = 11 };
    /// <summary>Extra padding (px) between the outer grid and the labels.</summary>
    public double Padding { get; set; } = 5;
    /// <summary>Optional per-label formatter: (label, index) => text.</summary>
    public Func<string, int, string>? Callback { get; set; }
}

/// <summary>Tick configuration for a scale.</summary>
public sealed class TickOptions
{
    public bool Display { get; set; } = true;
    public string Color { get; set; } = "#666";
    public ChartFont Font { get; set; } = new();
    public double Padding { get; set; } = 3;
    public double? StepSize { get; set; }
    public int? Count { get; set; }
    public int? MaxTicksLimit { get; set; } = 11;
    public int? Precision { get; set; }
    public double Rotation { get; set; }
    /// <summary>Maximum auto-rotation (degrees) for category tick labels that don't fit. Chart.js default 50.</summary>
    public double MaxRotation { get; set; } = 50;
    /// <summary>Minimum auto-rotation (degrees) for tick labels.</summary>
    public double MinRotation { get; set; }
    /// <summary>Tick label alignment relative to the tick (start/center/end).</summary>
    public Align Align { get; set; } = Align.Center;
    /// <summary>Render value-axis tick labels inside the chart area.</summary>
    public bool Mirror { get; set; }
    public bool AutoSkip { get; set; } = true;
    /// <summary>Optional formatting callback applied to each numeric/category tick value.</summary>
    public Func<double, int, string>? Callback { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
}

/// <summary>Axis title configuration.</summary>
public sealed class ScaleTitleOptions
{
    public bool Display { get; set; }
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#666";
    public ChartFont Font { get; set; } = new() { Weight = "bold" };
    public Padding Padding { get; set; } = new(4, 0, 4, 0);
}

/// <summary>
/// A scale/axis definition, mirroring Chart.js cartesian and radial scale options.
/// </summary>
public sealed class ScaleOptions
{
    public string Id { get; set; } = "";
    public ScaleType Type { get; set; } = ScaleType.Linear;
    public bool Display { get; set; } = true;
    public Position? Position { get; set; }

    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? SuggestedMin { get; set; }
    public double? SuggestedMax { get; set; }
    /// <summary>Extra padding added to the data range, as a fraction of the range (e.g. 0.1 = 10%).</summary>
    public double Grace { get; set; }
    public bool BeginAtZero { get; set; }
    public bool Reverse { get; set; }
    public bool Stacked { get; set; }
    /// <summary>When stacked, normalize each category to 100% (percentage stack).</summary>
    public bool Stacked100 { get; set; }

    /// <summary>Padding (fraction of a step) applied at the ends of a category axis.</summary>
    public bool Offset { get; set; }

    public GridOptions Grid { get; set; } = new();
    public TickOptions Ticks { get; set; } = new();
    public ScaleTitleOptions Title { get; set; } = new();
    /// <summary>The axis border line (drawn at the axis edge).</summary>
    public AxisBorderOptions Border { get; set; } = new();

    /// <summary>Angle lines configuration (radial scales only).</summary>
    public bool AngleLines { get; set; } = true;
    public string AngleLineColor { get; set; } = "rgba(0,0,0,0.1)";
    public double AngleLineWidth { get; set; } = 1;
    public List<double>? AngleLineDash { get; set; }
    /// <summary>Start angle in degrees for radial scales.</summary>
    public double StartAngle { get; set; }
    /// <summary>Point (category) labels around a radial scale.</summary>
    public PointLabelOptions PointLabels { get; set; } = new();
    /// <summary>Show a filled backdrop behind radial tick labels.</summary>
    public bool ShowLabelBackdrop { get; set; } = true;
    /// <summary>Backdrop color for radial tick labels.</summary>
    public string BackdropColor { get; set; } = "rgba(255,255,255,0.75)";

    // ---- Time scale ----
    /// <summary>The unit for a time axis. Auto picks a sensible unit from the data range.</summary>
    public TimeUnit TimeUnit { get; set; } = TimeUnit.Auto;
    /// <summary>Optional custom date formatter for time-axis tick labels.</summary>
    public Func<DateTime, string>? TimeFormat { get; set; }
}
