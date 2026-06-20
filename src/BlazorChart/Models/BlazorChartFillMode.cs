namespace BlazorChart.Models;

/// <summary>How <see cref="Models.LineFill"/> fills relative to a baseline.</summary>
public enum BlazorChartFillMode
{
    None,
    Origin,
    Start,
    End,
    Stack,
    /// <summary>Fill to another dataset's line (see <see cref="BlazorChartDataset.FillTargetIndex"/>).</summary>
    Dataset,
    /// <summary>Fill to an absolute axis value (see <see cref="BlazorChartDataset.FillValue"/>).</summary>
    Value
}
