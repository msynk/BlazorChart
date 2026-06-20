namespace BlazorChart;

/// <summary>Context passed to per-segment styling callbacks for a line.</summary>
public readonly record struct BlazorChartSegmentContext(
    int StartIndex,
    int EndIndex,
    double StartValue,
    double EndValue);
