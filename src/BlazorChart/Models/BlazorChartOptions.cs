namespace BlazorChart.Models;

/// <summary>
/// Top-level chart options, mirroring Chart.js <c>options</c>.
/// </summary>
public sealed class BlazorChartOptions
{
    public bool Responsive { get; set; } = true;
    public bool MaintainAspectRatio { get; set; } = true;
    /// <summary>Canvas aspect ratio (width/height). When null, defaults to 2 for cartesian charts and 1 for circular/radar charts.</summary>
    public double? AspectRatio { get; set; }

    public BlazorChartIndexAxis IndexAxis { get; set; } = BlazorChartIndexAxis.X;

    public BlazorChartLayoutOptions Layout { get; set; } = new();
    public BlazorChartInteractionOptions Interaction { get; set; } = new();
    public BlazorChartAnimationOptions Animation { get; set; } = new();
    public BlazorChartElementOptions Elements { get; set; } = new();
    public BlazorChartPluginOptions Plugins { get; set; } = new();
    public BlazorChartZoomOptions Zoom { get; set; } = new();

    /// <summary>Named scales, keyed by id (e.g. "x", "y", "r", "y2").</summary>
    public Dictionary<string, BlazorChartScaleOptions> Scales { get; set; } = new();

    // ---- Doughnut / pie / polar specific ----
    /// <summary>Inner radius as a percentage string for doughnut charts (0-100).</summary>
    public double CutoutPercentage { get; set; } = 50;
    /// <summary>Sweep of the chart in degrees (default 360).</summary>
    public double CircumferenceDegrees { get; set; } = 360;
    /// <summary>Starting angle in degrees (Chart.js default -90 = top).</summary>
    public double RotationDegrees { get; set; } = -90;

    /// <summary>Gets the scale with the given id, creating a default if missing.</summary>
    public BlazorChartScaleOptions GetOrAddScale(string id, BlazorChartScaleType type)
    {
        if (!Scales.TryGetValue(id, out var s))
        {
            s = new BlazorChartScaleOptions { Id = id, Type = type };
            Scales[id] = s;
        }
        return s;
    }
}
