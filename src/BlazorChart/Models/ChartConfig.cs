namespace BlazorChart.Models;

/// <summary>
/// The top-level chart configuration, mirroring the object you pass to <c>new Chart(ctx, config)</c>
/// in Chart.js: a <see cref="ChartType"/>, <see cref="ChartData"/> and <see cref="ChartOptions"/>.
/// </summary>
public sealed class ChartConfig
{
    public ChartType Type { get; set; } = ChartType.Line;
    public ChartData Data { get; set; } = new();
    public ChartOptions Options { get; set; } = new();

    public ChartConfig() { }

    public ChartConfig(ChartType type, ChartData data, ChartOptions? options = null)
    {
        Type = type;
        Data = data;
        Options = options ?? new ChartOptions();
    }
}
