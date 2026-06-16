namespace BlazorChart.Models;

/// <summary>
/// Contextual information passed to scriptable dataset options, mirroring Chart.js
/// <c>ScriptableContext</c>. A scriptable option is a <c>Func&lt;ScriptableContext, T&gt;</c> that
/// is evaluated per data element, letting colors/radius/styles vary by value, index or state.
/// </summary>
public sealed class ScriptableContext
{
    /// <summary>Index of the dataset this element belongs to.</summary>
    public int DatasetIndex { get; init; }

    /// <summary>Index of the data point within the dataset.</summary>
    public int DataIndex { get; init; }

    /// <summary>The parsed primary value (y for cartesian, value for arcs/radar).</summary>
    public double? Value { get; init; }

    /// <summary>The parsed x value for point-based datasets (scatter/bubble), else null.</summary>
    public double? ValueX { get; init; }

    /// <summary>The bubble radius value for bubble datasets, else null.</summary>
    public double? ValueR { get; init; }

    /// <summary>The category label for this index, if any.</summary>
    public string? Label { get; init; }

    /// <summary>The dataset label.</summary>
    public string? DatasetLabel { get; init; }

    /// <summary>True when this element is currently active (hovered/focused).</summary>
    public bool Active { get; init; }

    /// <summary>The effective chart type for this dataset.</summary>
    public ChartType Type { get; init; }
}

/// <summary>
/// A value that is either a constant or a function of <see cref="ScriptableContext"/>.
/// Provides implicit conversions so existing constant assignments keep working while also
/// accepting a scriptable function, mirroring Chart.js scriptable options.
/// </summary>
/// <typeparam name="T">The resolved value type.</typeparam>
public readonly struct Scriptable<T>
{
    private readonly T? _constant;
    private readonly Func<ScriptableContext, T?>? _fn;

    public Scriptable(T? constant) { _constant = constant; _fn = null; }
    public Scriptable(Func<ScriptableContext, T?> fn) { _fn = fn; _constant = default; }

    public bool HasValue => _fn is not null || _constant is not null;

    /// <summary>Resolves the value for the given context (function takes precedence).</summary>
    public T? Resolve(ScriptableContext ctx) => _fn is not null ? _fn(ctx) : _constant;

    public static implicit operator Scriptable<T>(T value) => new(value);
    public static implicit operator Scriptable<T>(Func<ScriptableContext, T?> fn) => new(fn);
}
