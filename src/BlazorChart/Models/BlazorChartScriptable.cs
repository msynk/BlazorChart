namespace BlazorChart;

/// <summary>
/// A value that is either a constant or a function of <see cref="BlazorChartScriptableContext"/>.
/// Provides implicit conversions so existing constant assignments keep working while also
/// accepting a scriptable function, mirroring Chart.js scriptable options.
/// </summary>
/// <typeparam name="T">The resolved value type.</typeparam>
public readonly struct BlazorChartScriptable<T>
{
    private readonly T? _constant;
    private readonly Func<BlazorChartScriptableContext, T?>? _fn;

    public BlazorChartScriptable(T? constant) { _constant = constant; _fn = null; }
    public BlazorChartScriptable(Func<BlazorChartScriptableContext, T?> fn) { _fn = fn; _constant = default; }

    public bool HasValue => _fn is not null || _constant is not null;

    /// <summary>Resolves the value for the given context (function takes precedence).</summary>
    public T? Resolve(BlazorChartScriptableContext ctx) => _fn is not null ? _fn(ctx) : _constant;

    public static implicit operator BlazorChartScriptable<T>(T value) => new(value);
    public static implicit operator BlazorChartScriptable<T>(Func<BlazorChartScriptableContext, T?> fn) => new(fn);
}
