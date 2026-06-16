namespace BlazorChart.Rendering;

/// <summary>Mutable interaction state shared with the component (hover + legend toggles).</summary>
public sealed class RenderState
{
    /// <summary>Datasets hidden via the legend (by dataset index).</summary>
    public HashSet<int> HiddenDatasets { get; } = new();

    /// <summary>Data indices hidden via the legend (pie/doughnut/polarArea).</summary>
    public HashSet<int> HiddenIndices { get; } = new();

    /// <summary>The currently hovered element, if any.</summary>
    public (int Dataset, int Index)? Active { get; set; }

    /// <summary>Zoom/pan range overrides per axis id (min, max in data coordinates).</summary>
    public Dictionary<string, (double Min, double Max)> AxisRanges { get; } = new();

    public bool IsDatasetHidden(int i) => HiddenDatasets.Contains(i);
    public bool IsIndexHidden(int i) => HiddenIndices.Contains(i);
}

/// <summary>A rectangular plotting area in SVG pixel coordinates.</summary>
public struct Area
{
    public double Left, Top, Right, Bottom;
    public Area(double left, double top, double right, double bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }
    public readonly double Width => Right - Left;
    public readonly double Height => Bottom - Top;
    public readonly double CenterX => (Left + Right) / 2;
    public readonly double CenterY => (Top + Bottom) / 2;
}
