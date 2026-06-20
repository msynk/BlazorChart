namespace BlazorChart;

/// <summary>
/// Tooltip text/styling callbacks, mirroring Chart.js <c>tooltip.callbacks</c>. Any callback
/// returning null falls back to the default behavior. Multi-line results may use '\n'.
/// </summary>
public sealed class BlazorChartTooltipCallbacks
{
    /// <summary>Builds the tooltip title from the active items.</summary>
    public Func<IReadOnlyList<BlazorChartTooltipItemContext>, string?>? Title { get; set; }
    /// <summary>Lines rendered before the body items.</summary>
    public Func<IReadOnlyList<BlazorChartTooltipItemContext>, string?>? BeforeBody { get; set; }
    /// <summary>Builds the body line for a single item (replaces the default "label: value").</summary>
    public Func<BlazorChartTooltipItemContext, string?>? Label { get; set; }
    /// <summary>Overrides the color swatch shown next to a body line.</summary>
    public Func<BlazorChartTooltipItemContext, string?>? LabelColor { get; set; }
    /// <summary>Lines rendered after the body items.</summary>
    public Func<IReadOnlyList<BlazorChartTooltipItemContext>, string?>? AfterBody { get; set; }
    /// <summary>Footer lines rendered below the body.</summary>
    public Func<IReadOnlyList<BlazorChartTooltipItemContext>, string?>? Footer { get; set; }
}
