namespace BlazorChart.Models;

/// <summary>Legend label configuration.</summary>
public sealed class LegendLabelOptions
{
    public string Color { get; set; } = "#666";
    public ChartFont Font { get; set; } = new();
    public double BoxWidth { get; set; } = 40;
    public double BoxHeight { get; set; } = 12;
    public double Padding { get; set; } = 10;
    public bool UsePointStyle { get; set; }
    public PointStyle PointStyle { get; set; } = PointStyle.Circle;
}

/// <summary>Legend plugin options.</summary>
public sealed class LegendOptions
{
    public bool Display { get; set; } = true;
    public Position Position { get; set; } = Position.Top;
    public Align Align { get; set; } = Align.Center;
    public bool Reverse { get; set; }
    /// <summary>Allow clicking a legend item to toggle dataset/data visibility.</summary>
    public bool OnClickToggle { get; set; } = true;
    public LegendLabelOptions Labels { get; set; } = new();
    public string? Title { get; set; }
}

/// <summary>Title / subtitle plugin options.</summary>
public sealed class TitleOptions
{
    public bool Display { get; set; }
    public string Text { get; set; } = "";
    public string Color { get; set; } = "#333";
    public Position Position { get; set; } = Position.Top;
    public Align Align { get; set; } = Align.Center;
    public ChartFont Font { get; set; } = new() { Size = 16, Weight = "bold" };
    public Padding Padding { get; set; } = 10;
}

/// <summary>
/// A single tooltip item exposed to tooltip callbacks, mirroring Chart.js <c>TooltipItem</c>.
/// </summary>
public sealed class TooltipItemContext
{
    public int DatasetIndex { get; init; }
    public int DataIndex { get; init; }
    /// <summary>The dataset label.</summary>
    public string? DatasetLabel { get; init; }
    /// <summary>The category/point label.</summary>
    public string? Label { get; init; }
    /// <summary>The parsed primary (y) value.</summary>
    public double Value { get; init; }
    /// <summary>The parsed x value for point datasets, else null.</summary>
    public double? ValueX { get; init; }
    /// <summary>The dataset color (for the legend swatch).</summary>
    public string Color { get; init; } = "#000";
    /// <summary>The default formatted value string.</summary>
    public string FormattedValue { get; init; } = "";
}

/// <summary>
/// Tooltip text/styling callbacks, mirroring Chart.js <c>tooltip.callbacks</c>. Any callback
/// returning null falls back to the default behavior. Multi-line results may use '\n'.
/// </summary>
public sealed class TooltipCallbacks
{
    /// <summary>Builds the tooltip title from the active items.</summary>
    public Func<IReadOnlyList<TooltipItemContext>, string?>? Title { get; set; }
    /// <summary>Lines rendered before the body items.</summary>
    public Func<IReadOnlyList<TooltipItemContext>, string?>? BeforeBody { get; set; }
    /// <summary>Builds the body line for a single item (replaces the default "label: value").</summary>
    public Func<TooltipItemContext, string?>? Label { get; set; }
    /// <summary>Overrides the color swatch shown next to a body line.</summary>
    public Func<TooltipItemContext, string?>? LabelColor { get; set; }
    /// <summary>Lines rendered after the body items.</summary>
    public Func<IReadOnlyList<TooltipItemContext>, string?>? AfterBody { get; set; }
    /// <summary>Footer lines rendered below the body.</summary>
    public Func<IReadOnlyList<TooltipItemContext>, string?>? Footer { get; set; }
}

/// <summary>Tooltip plugin options.</summary>
public sealed class TooltipOptions
{
    public bool Enabled { get; set; } = true;
    public InteractionMode Mode { get; set; } = InteractionMode.Nearest;
    public bool Intersect { get; set; } = true;
    /// <summary>Where the tooltip is anchored when multiple items are active.</summary>
    public TooltipPositioner Position { get; set; } = TooltipPositioner.Average;
    public string BackgroundColor { get; set; } = "rgba(0,0,0,0.8)";
    public string TitleColor { get; set; } = "#fff";
    public string BodyColor { get; set; } = "#fff";
    public string FooterColor { get; set; } = "#fff";
    public ChartFont TitleFont { get; set; } = new() { Weight = "bold" };
    public ChartFont BodyFont { get; set; } = new();
    public ChartFont FooterFont { get; set; } = new() { Weight = "bold" };
    public double Padding { get; set; } = 6;
    public double CornerRadius { get; set; } = 6;
    public bool DisplayColors { get; set; } = true;
    /// <summary>Render the color swatch using the dataset point style instead of a square.</summary>
    public bool UsePointStyle { get; set; }
    /// <summary>Border color of the tooltip box.</summary>
    public string? BorderColor { get; set; }
    /// <summary>Border width of the tooltip box.</summary>
    public double BorderWidth { get; set; }
    /// <summary>Text alignment of the title (left/center/right).</summary>
    public Align TitleAlign { get; set; } = Align.Start;
    /// <summary>Text alignment of the body (left/center/right).</summary>
    public Align BodyAlign { get; set; } = Align.Start;
    /// <summary>Rich text/styling callbacks.</summary>
    public TooltipCallbacks Callbacks { get; set; } = new();
    /// <summary>Optional label formatter: (datasetLabel, value) => text. Shorthand for <c>Callbacks.Label</c>.</summary>
    public Func<string, double, string>? LabelFormatter { get; set; }
}

/// <summary>Data label plugin options (renders values on the chart).</summary>
public sealed class DataLabelOptions
{
    public bool Display { get; set; }
    public string Color { get; set; } = "#333";
    public ChartFont Font { get; set; } = new();
    /// <summary>Simple value formatter.</summary>
    public Func<double, string>? Formatter { get; set; }
    /// <summary>Rich formatter receiving (value, datasetIndex, dataIndex).</summary>
    public Func<double, int, int, string>? FormatterCtx { get; set; }
    /// <summary>Per-element display predicate (value, datasetIndex, dataIndex) => show.</summary>
    public Func<double, int, int, bool>? DisplayFn { get; set; }
    /// <summary>Anchor of the label relative to the element (start = baseline, center, end = tip).</summary>
    public Align Anchor { get; set; } = Align.Center;
    /// <summary>Optional background color drawn behind the label.</summary>
    public string? BackgroundColor { get; set; }
    /// <summary>Corner radius of the label background.</summary>
    public double BorderRadius { get; set; } = 3;
    /// <summary>Padding inside the label background.</summary>
    public double Padding { get; set; } = 2;
    /// <summary>Rotation of the label text in degrees.</summary>
    public double Rotation { get; set; }
}

/// <summary>Decimation (downsampling) options for large line datasets.</summary>
public sealed class DecimationOptions
{
    public bool Enabled { get; set; }
    /// <summary>Target number of points to keep (LTTB).</summary>
    public int Samples { get; set; } = 200;
    /// <summary>Only decimate when the dataset exceeds this many points.</summary>
    public int Threshold { get; set; } = 500;
}

/// <summary>Container for all plugin options, mirroring Chart.js <c>options.plugins</c>.</summary>
public sealed class PluginOptions
{
    public LegendOptions Legend { get; set; } = new();
    public TitleOptions Title { get; set; } = new();
    public TitleOptions Subtitle { get; set; } = new() { Font = new ChartFont { Size = 13 } };
    public TooltipOptions Tooltip { get; set; } = new();
    public DataLabelOptions DataLabels { get; set; } = new();
    public DecimationOptions Decimation { get; set; } = new();

    /// <summary>User-registered drawing plugins (annotations, custom overlays, ...).</summary>
    public List<Rendering.Plugins.IChartPlugin> Custom { get; set; } = new();
}
