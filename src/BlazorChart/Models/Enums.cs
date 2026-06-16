namespace BlazorChart.Models;

/// <summary>The built-in chart types, mirroring Chart.js.</summary>
public enum ChartType
{
    Line,
    Bar,
    Radar,
    Pie,
    Doughnut,
    PolarArea,
    Bubble,
    Scatter
}

/// <summary>Scale (axis) types, mirroring Chart.js cartesian and radial scales.</summary>
public enum ScaleType
{
    Linear,
    Logarithmic,
    Category,
    Time,
    RadialLinear
}

/// <summary>Generic position used by axes, legend and title.</summary>
public enum Position
{
    Top,
    Left,
    Bottom,
    Right,
    Center,
    Chart
}

/// <summary>Alignment used by legend/title labels.</summary>
public enum Align
{
    Start,
    Center,
    End
}

/// <summary>The axis along which the data index is laid out ('x' = vertical bars, 'y' = horizontal bars).</summary>
public enum IndexAxis
{
    X,
    Y
}

/// <summary>How <see cref="Models.LineFill"/> fills relative to a baseline.</summary>
public enum FillMode
{
    None,
    Origin,
    Start,
    End,
    Stack,
    /// <summary>Fill to another dataset's line (see <see cref="ChartDataset.FillTargetIndex"/>).</summary>
    Dataset,
    /// <summary>Fill to an absolute axis value (see <see cref="ChartDataset.FillValue"/>).</summary>
    Value
}

/// <summary>Point marker styles, mirroring Chart.js pointStyle.</summary>
public enum PointStyle
{
    Circle,
    Cross,
    CrossRot,
    Dash,
    Line,
    Rect,
    RectRounded,
    RectRot,
    Star,
    Triangle,
    None
}

/// <summary>Interaction mode used to determine which items are active on hover.</summary>
public enum InteractionMode
{
    Nearest,
    Index,
    Dataset,
    Point,
    X,
    Y
}

/// <summary>Stepped line rendering options.</summary>
public enum SteppedLine
{
    False,
    Before,
    After,
    Middle
}

/// <summary>Cubic interpolation mode for line smoothing, mirroring Chart.js.</summary>
public enum CubicInterpolationMode
{
    /// <summary>Cardinal spline using the dataset tension.</summary>
    Default,
    /// <summary>Monotone cubic interpolation that never overshoots the data (good for monotonic series).</summary>
    Monotone
}

/// <summary>Time axis units, mirroring Chart.js time scale units.</summary>
public enum TimeUnit
{
    Auto,
    Millisecond,
    Second,
    Minute,
    Hour,
    Day,
    Week,
    Month,
    Quarter,
    Year
}

/// <summary>Zoom/pan axis mode.</summary>
public enum ZoomMode
{
    X,
    Y,
    XY
}

/// <summary>Which edge of a bar omits its border.</summary>
public enum BorderSkipped
{
    None,
    Start,
    End,
    Top,
    Bottom,
    Left,
    Right
}

/// <summary>Where the tooltip is anchored relative to the active items.</summary>
public enum TooltipPositioner
{
    /// <summary>Anchor at the nearest (hovered) item.</summary>
    Nearest,
    /// <summary>Anchor at the average position of all active items.</summary>
    Average
}
