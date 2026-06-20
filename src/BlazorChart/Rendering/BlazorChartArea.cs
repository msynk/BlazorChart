namespace BlazorChart.Rendering;

/// <summary>A rectangular plotting area in SVG pixel coordinates.</summary>
public struct BlazorChartArea
{
    public double Left, Top, Right, Bottom;
    public BlazorChartArea(double left, double top, double right, double bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }
    public readonly double Width => Right - Left;
    public readonly double Height => Bottom - Top;
    public readonly double CenterX => (Left + Right) / 2;
    public readonly double CenterY => (Top + Bottom) / 2;
}
