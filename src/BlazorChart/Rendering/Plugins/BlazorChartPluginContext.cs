
namespace BlazorChart;

/// <summary>Context passed to plugins, exposing the scene, area and scale conversions.</summary>
public sealed class BlazorChartPluginContext
{
    public required BlazorChartScene Scene { get; init; }
    public required BlazorChartConfig Config { get; init; }
    public BlazorChartArea? Plot { get; init; }
    public bool IsCartesian { get; init; }

    // ---- Circular / radial geometry (pie/doughnut/polar/radar) ----
    /// <summary>Center of a circular/radial chart, if applicable.</summary>
    public double CenterX { get; init; }
    public double CenterY { get; init; }
    /// <summary>Inner radius (doughnut cutout) in pixels.</summary>
    public double InnerRadius { get; init; }
    /// <summary>Outer radius in pixels.</summary>
    public double OuterRadius { get; init; }

    internal BlazorChartAxisScale? IndexScale { get; init; }
    internal Dictionary<string, BlazorChartAxisScale>? ValueScales { get; init; }
    internal bool IndexIsCategory { get; init; }

    /// <summary>Pixel position along the index axis for a category index.</summary>
    public double XForIndex(int index, bool centered = true)
        => IndexScale?.PixelForIndex(index, centered) ?? 0;

    /// <summary>Pixel position along the index axis for a raw value (linear/time axes).</summary>
    public double XForValue(double value) => IndexScale?.PixelFor(value) ?? 0;

    /// <summary>Pixel position along a value axis (default "y").</summary>
    public double YForValue(double value, string axisId = "y")
    {
        if (ValueScales is null) return 0;
        if (ValueScales.TryGetValue(axisId, out var s)) return s.PixelFor(value);
        return ValueScales.Values.FirstOrDefault()?.PixelFor(value) ?? 0;
    }

    public void AddBehind(BlazorChartSvgNode node) => Scene.Background.Add(node);
    public void AddFront(BlazorChartSvgNode node) => Scene.Foreground.Add(node);
}
