namespace BlazorChart.Models;

/// <summary>Container for all plugin options, mirroring Chart.js <c>options.plugins</c>.</summary>
public sealed class BlazorChartPluginOptions
{
    public BlazorChartLegendOptions Legend { get; set; } = new();
    public BlazorChartTitleOptions Title { get; set; } = new();
    public BlazorChartTitleOptions Subtitle { get; set; } = new() { Font = new BlazorChartFont { Size = 13 } };
    public BlazorChartTooltipOptions Tooltip { get; set; } = new();
    public BlazorChartDataLabelOptions DataLabels { get; set; } = new();
    public BlazorChartDecimationOptions Decimation { get; set; } = new();

    /// <summary>User-registered drawing plugins (annotations, custom overlays, ...).</summary>
    public List<Rendering.Plugins.IBlazorChartPlugin> Custom { get; set; } = new();
}
