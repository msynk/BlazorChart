namespace BlazorChart.Models;

/// <summary>Layout options (chart padding), mirroring Chart.js <c>options.layout</c>.</summary>
public sealed class LayoutOptions
{
    public Padding Padding { get; set; } = 4;
}

/// <summary>Interaction options, mirroring Chart.js <c>options.interaction</c>.</summary>
public sealed class InteractionOptions
{
    public InteractionMode Mode { get; set; } = InteractionMode.Nearest;
    public bool Intersect { get; set; } = true;
}

/// <summary>Animation options. For the SVG renderer these map to CSS transitions.</summary>
public sealed class AnimationOptions
{
    public bool Animate { get; set; } = true;
    public int Duration { get; set; } = 600;
    public string Easing { get; set; } = "ease-out";
    /// <summary>Per-element entry delay (ms). When &gt; 0, elements animate in sequence (staggered).</summary>
    public double DelayBetween { get; set; }
}

/// <summary>Default element options, mirroring Chart.js <c>options.elements</c>.</summary>
public sealed class ElementOptions
{
    public double PointRadius { get; set; } = 3;
    public double LineTension { get; set; }
    public double LineBorderWidth { get; set; } = 3;
    public double BarBorderWidth { get; set; }
    public double ArcBorderWidth { get; set; } = 2;
    public string ArcBorderColor { get; set; } = "#fff";
}

/// <summary>Zoom &amp; pan options, mirroring chartjs-plugin-zoom. Cartesian charts only.</summary>
public sealed class ZoomOptions
{
    public bool Enabled { get; set; }
    /// <summary>Enable mouse-wheel zoom.</summary>
    public bool Wheel { get; set; } = true;
    /// <summary>Enable click-and-drag panning.</summary>
    public bool Pan { get; set; } = true;
    /// <summary>Enable drag-to-zoom box selection (overrides <see cref="Pan"/> for the drag gesture).</summary>
    public bool DragZoom { get; set; }
    /// <summary>Fill color of the drag-zoom selection box.</summary>
    public string DragBoxColor { get; set; } = "rgba(54,162,235,0.2)";
    /// <summary>Axis/axes affected by zoom and pan.</summary>
    public ZoomMode Mode { get; set; } = ZoomMode.X;
    /// <summary>Wheel zoom sensitivity (fraction per wheel notch).</summary>
    public double Speed { get; set; } = 0.15;
}

/// <summary>
/// Top-level chart options, mirroring Chart.js <c>options</c>.
/// </summary>
public sealed class ChartOptions
{
    public bool Responsive { get; set; } = true;
    public bool MaintainAspectRatio { get; set; } = true;
    /// <summary>Canvas aspect ratio (width/height). When null, defaults to 2 for cartesian charts and 1 for circular/radar charts.</summary>
    public double? AspectRatio { get; set; }

    public IndexAxis IndexAxis { get; set; } = IndexAxis.X;

    public LayoutOptions Layout { get; set; } = new();
    public InteractionOptions Interaction { get; set; } = new();
    public AnimationOptions Animation { get; set; } = new();
    public ElementOptions Elements { get; set; } = new();
    public PluginOptions Plugins { get; set; } = new();
    public ZoomOptions Zoom { get; set; } = new();

    /// <summary>Named scales, keyed by id (e.g. "x", "y", "r", "y2").</summary>
    public Dictionary<string, ScaleOptions> Scales { get; set; } = new();

    // ---- Doughnut / pie / polar specific ----
    /// <summary>Inner radius as a percentage string for doughnut charts (0-100).</summary>
    public double CutoutPercentage { get; set; } = 50;
    /// <summary>Sweep of the chart in degrees (default 360).</summary>
    public double CircumferenceDegrees { get; set; } = 360;
    /// <summary>Starting angle in degrees (Chart.js default -90 = top).</summary>
    public double RotationDegrees { get; set; } = -90;

    /// <summary>Gets the scale with the given id, creating a default if missing.</summary>
    public ScaleOptions GetOrAddScale(string id, ScaleType type)
    {
        if (!Scales.TryGetValue(id, out var s))
        {
            s = new ScaleOptions { Id = id, Type = type };
            Scales[id] = s;
        }
        return s;
    }
}
