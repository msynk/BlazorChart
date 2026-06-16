namespace BlazorChart.Models;

/// <summary>A single color stop in a gradient.</summary>
public readonly record struct GradientStop(double Offset, string Color);

/// <summary>Base type for gradient fills (linear or radial).</summary>
public abstract class GradientBase
{
    public List<GradientStop> Stops { get; set; } = new();
}

/// <summary>
/// A linear gradient fill. By default it runs vertically (top→bottom of the chart area),
/// which is the common case for area fills.
/// </summary>
public sealed class LinearGradient : GradientBase
{
    /// <summary>When true the gradient runs vertically, otherwise horizontally.</summary>
    public bool Vertical { get; set; } = true;

    public LinearGradient() { }

    public LinearGradient(bool vertical, params GradientStop[] stops)
    {
        Vertical = vertical;
        Stops.AddRange(stops);
    }

    /// <summary>Convenience: a two-stop top→bottom gradient between two colors.</summary>
    public static LinearGradient Vertical2(string top, string bottom) =>
        new(true, new GradientStop(0, top), new GradientStop(1, bottom));

    /// <summary>Convenience: a two-stop left→right gradient between two colors.</summary>
    public static LinearGradient Horizontal2(string left, string right) =>
        new(false, new GradientStop(0, left), new GradientStop(1, right));
}

/// <summary>
/// A radial gradient fill. Center and radius are expressed as fractions (0..1) of the filled
/// shape's bounding box, mirroring SVG <c>objectBoundingBox</c> units.
/// </summary>
public sealed class RadialGradient : GradientBase
{
    /// <summary>Center X as a fraction of the bounding box (0..1).</summary>
    public double CenterX { get; set; } = 0.5;
    /// <summary>Center Y as a fraction of the bounding box (0..1).</summary>
    public double CenterY { get; set; } = 0.5;
    /// <summary>Radius as a fraction of the bounding box (0..1).</summary>
    public double Radius { get; set; } = 0.5;

    public RadialGradient() { }

    public RadialGradient(params GradientStop[] stops) => Stops.AddRange(stops);

    /// <summary>Convenience: a two-stop center→edge radial gradient.</summary>
    public static RadialGradient Center2(string center, string edge) =>
        new(new GradientStop(0, center), new GradientStop(1, edge));
}
