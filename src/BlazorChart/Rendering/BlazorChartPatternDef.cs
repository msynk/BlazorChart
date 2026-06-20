using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>A registered pattern definition referenced by id.</summary>
public sealed record BlazorChartPatternDef(string Id, BlazorChartFillPattern Pattern);
