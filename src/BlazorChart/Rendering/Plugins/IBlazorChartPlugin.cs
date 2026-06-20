using BlazorChart.Models;

namespace BlazorChart.Rendering.Plugins;

/// <summary>
/// A drawing plugin, mirroring the Chart.js plugin concept. Plugins receive a
/// <see cref="BlazorChartPluginContext"/> with the computed scene, plotting area and scale conversions,
/// and can inject extra SVG primitives behind or in front of the datasets.
/// </summary>
public interface IBlazorChartPlugin
{
    /// <summary>Unique plugin id.</summary>
    string Id { get; }

    /// <summary>Called after scales/grid are computed but before datasets are drawn.</summary>
    void BeforeDatasetsDraw(BlazorChartPluginContext ctx) { }

    /// <summary>Called after datasets are drawn.</summary>
    void AfterDatasetsDraw(BlazorChartPluginContext ctx) { }
}
