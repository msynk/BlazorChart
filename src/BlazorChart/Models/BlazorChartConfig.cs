namespace BlazorChart.Models;

/// <summary>
/// The top-level chart configuration, mirroring the object you pass to <c>new Chart(ctx, config)</c>
/// in Chart.js: a <see cref="BlazorChartType"/>, <see cref="BlazorChartData"/> and <see cref="BlazorChartOptions"/>.
/// </summary>
public sealed class BlazorChartConfig
{
    public BlazorChartType Type { get; set; } = BlazorChartType.Line;
    public BlazorChartData Data { get; set; } = new();
    public BlazorChartOptions Options { get; set; } = new();

    public BlazorChartConfig() { }

    public BlazorChartConfig(BlazorChartType type, BlazorChartData data, BlazorChartOptions? options = null)
    {
        Type = type;
        Data = data;
        Options = options ?? new BlazorChartOptions();
    }
}
