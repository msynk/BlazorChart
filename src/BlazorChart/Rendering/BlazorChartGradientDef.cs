using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>A registered gradient definition referenced by id.</summary>
public sealed record BlazorChartGradientDef(string Id, BlazorChartGradientBase Gradient);
