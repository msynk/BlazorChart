using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>
/// Computes a <see cref="BlazorChartScene"/> (pure SVG primitives + interaction metadata) from a
/// <see cref="BlazorChartConfig"/>. This is the heart of the native Blazor renderer — no JavaScript or
/// canvas is involved.
/// </summary>
public sealed partial class BlazorChartRenderer
{
    private readonly BlazorChartConfig _config;
    private readonly BlazorChartData _data;
    private readonly BlazorChartOptions _options;
    private readonly BlazorChartRenderState _state;
    private readonly double _w;
    private readonly double _h;

    public BlazorChartRenderer(BlazorChartConfig config, BlazorChartRenderState state, double width, double height)
    {
        _config = config;
        _data = config.Data;
        _options = config.Options;
        _state = state;
        _w = width;
        _h = height;
    }

    public BlazorChartScene Render()
    {
        var scene = new BlazorChartScene { Width = _w, Height = _h };
        scene.Title = BuildTitle(_options.Plugins.Title);
        scene.Subtitle = BuildTitle(_options.Plugins.Subtitle);
        EnsureScales();

        switch (_config.Type)
        {
            case BlazorChartType.Pie:
            case BlazorChartType.Doughnut:
            case BlazorChartType.PolarArea:
                scene.IsRadialOrCircular = true;
                RenderCircular(scene);
                break;
            case BlazorChartType.Radar:
                scene.IsRadialOrCircular = true;
                RenderRadar(scene);
                break;
            default:
                RenderCartesian(scene);
                break;
        }

        BuildLegend(scene);
        return scene;
    }

    // ---- shared helpers ----

    private int _gradSeq;

    /// <summary>Registers a gradient on the scene and returns a <c>url(#id)</c> fill reference.</summary>
    private string RegisterGradient(BlazorChartScene scene, BlazorChartGradientBase grad)
    {
        string id = $"bcgrad{_gradSeq++}";
        scene.Defs.Add(new BlazorChartGradientDef(id, grad));
        return $"url(#{id})";
    }

    /// <summary>Registers a pattern on the scene and returns a <c>url(#id)</c> fill reference.</summary>
    private string RegisterPattern(BlazorChartScene scene, BlazorChartFillPattern pattern)
    {
        string id = $"bcpat{_gradSeq++}";
        scene.Patterns.Add(new BlazorChartPatternDef(id, pattern));
        return $"url(#{id})";
    }

    private static double EstimateTextWidth(string? text, double fontSize)
        => BlazorChartTextMeasure.Width(text, fontSize);

    private BlazorChartArea ContentArea()
    {
        var p = _options.Layout.Padding;
        return new BlazorChartArea(p.Left, p.Top, _w - p.Right, _h - p.Bottom);
    }

    /// <summary>Builds a scriptable-options context for a data element.</summary>
    private BlazorChartScriptableContext Ctx(BlazorChartDataset ds, int dsIndex, int dataIndex, double? value = null)
    {
        bool active = _state.Active == (dsIndex, dataIndex);
        double? v = value;
        double? vx = null, vr = null;
        if (v is null)
        {
            if (ds.Points is { } pts && dataIndex < pts.Count)
            {
                v = pts[dataIndex].Y; vx = pts[dataIndex].X; vr = pts[dataIndex].R;
            }
            else if (dataIndex < ds.Data.Count)
            {
                v = ds.Data[dataIndex];
            }
        }
        return new BlazorChartScriptableContext
        {
            DatasetIndex = dsIndex,
            DataIndex = dataIndex,
            Value = v,
            ValueX = vx,
            ValueR = vr,
            Label = dataIndex < _data.Labels.Count ? _data.Labels[dataIndex] : null,
            DatasetLabel = ds.Label,
            Active = active,
            Type = EffectiveType(ds)
        };
    }

    /// <summary>Resolves the effective color for a data element, honoring dataset palettes.</summary>
    private string ResolveBackground(BlazorChartDataset ds, int dsIndex, int dataIndex, bool perIndexPalette, double? value = null)
    {
        if (ds.BackgroundColorFn is { } fn && fn(Ctx(ds, dsIndex, dataIndex, value)) is { } c) return c;
        if (ds.BackgroundColors is { Count: > 0 } list)
            return list[dataIndex % list.Count];
        if (!string.IsNullOrEmpty(ds.BackgroundColor))
            return ds.BackgroundColor!;
        return perIndexPalette ? BlazorChartColorUtil.Palette(dataIndex) : BlazorChartColorUtil.Palette(dsIndex);
    }

    private string ResolveBorder(BlazorChartDataset ds, int dsIndex, int dataIndex, bool perIndexPalette, double? value = null)
    {
        if (ds.BorderColorFn is { } fn && fn(Ctx(ds, dsIndex, dataIndex, value)) is { } c) return c;
        if (ds.BorderColors is { Count: > 0 } list)
            return list[dataIndex % list.Count];
        if (!string.IsNullOrEmpty(ds.BorderColor))
            return ds.BorderColor!;
        return perIndexPalette ? BlazorChartColorUtil.Palette(dataIndex) : BlazorChartColorUtil.Palette(dsIndex);
    }

    private string FormatTooltipValue(BlazorChartDataset ds, double value)
    {
        var t = _options.Plugins.Tooltip;
        if (t.LabelFormatter is { } f) return f(ds.Label ?? "", value);
        string label = string.IsNullOrEmpty(ds.Label) ? "" : ds.Label + ": ";
        return label + value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Builds a tooltip item context for callbacks.</summary>
    private BlazorChartTooltipItemContext BuildTooltipItem(BlazorChartDataset ds, int dsIndex, int dataIndex, double value, string color)
    {
        double? vx = ds.Points is { } pts && dataIndex < pts.Count ? pts[dataIndex].X : null;
        return new BlazorChartTooltipItemContext
        {
            DatasetIndex = dsIndex,
            DataIndex = dataIndex,
            DatasetLabel = ds.Label,
            Label = dataIndex < _data.Labels.Count ? _data.Labels[dataIndex] : null,
            Value = value,
            ValueX = vx,
            Color = color,
            FormattedValue = value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    /// <summary>Builds the body text for one tooltip item, honoring the Label callback / formatter.</summary>
    private string BuildItemText(BlazorChartDataset ds, int dsIndex, int dataIndex, double value, string color)
    {
        var t = _options.Plugins.Tooltip;
        if (t.Callbacks.Label is { } cb && cb(BuildTooltipItem(ds, dsIndex, dataIndex, value, color)) is { } txt)
            return txt;
        return FormatTooltipValue(ds, value);
    }

    private void EnsureScales()
    {
        if (_config.Type is BlazorChartType.Pie or BlazorChartType.Doughnut)
            return;

        if (_config.Type is BlazorChartType.PolarArea or BlazorChartType.Radar)
        {
            _options.GetOrAddScale("r", BlazorChartScaleType.RadialLinear);
            return;
        }

        // Cartesian: ensure x and y exist with sensible defaults.
        var x = _options.GetOrAddScale("x", _config.Type is BlazorChartType.Scatter or BlazorChartType.Bubble
            ? BlazorChartScaleType.Linear : BlazorChartScaleType.Category);
        x.Position ??= BlazorChartPosition.Bottom;

        // Additional x axes referenced by datasets (default linear, bottom).
        foreach (var id in _data.Datasets.Select(d => d.XAxisID).Distinct())
        {
            if (id == "x" || string.IsNullOrEmpty(id)) continue;
            var x2 = _options.GetOrAddScale(id, BlazorChartScaleType.Linear);
            x2.Position ??= BlazorChartPosition.Bottom;
        }

        // Gather y axis ids referenced by datasets.
        var yIds = _data.Datasets.Select(d => d.YAxisID).Distinct().ToList();
        foreach (var id in yIds)
        {
            var y = _options.GetOrAddScale(id, BlazorChartScaleType.Linear);
            y.Position ??= BlazorChartPosition.Left;
        }
        if (!_options.Scales.ContainsKey("y"))
        {
            var y = _options.GetOrAddScale("y", BlazorChartScaleType.Linear);
            y.Position ??= BlazorChartPosition.Left;
        }
    }

    private void BuildLegend(BlazorChartScene scene)
    {
        var lo = _options.Plugins.Legend;
        if (!lo.Display) return;

        var legend = new BlazorChartLegendModel
        {
            Position = lo.Position,
            Align = lo.Align,
            Labels = lo.Labels,
            Title = lo.Title,
            OnClickToggle = lo.OnClickToggle
        };

        if (_config.Type is BlazorChartType.Pie or BlazorChartType.Doughnut or BlazorChartType.PolarArea)
        {
            // One legend entry per data index (label).
            var ds = _data.Datasets.FirstOrDefault();
            int n = _data.Labels.Count;
            for (int i = 0; i < n; i++)
            {
                legend.Items.Add(new BlazorChartLegendItemModel
                {
                    Text = _data.Labels[i],
                    Color = ds is null ? BlazorChartColorUtil.Palette(i) : ResolveBackground(ds, 0, i, true),
                    Hidden = _state.IsIndexHidden(i),
                    Index = i,
                    IsDataIndex = true,
                    UsePointStyle = lo.Labels.UsePointStyle,
                    PointStyle = lo.Labels.PointStyle
                });
            }
        }
        else
        {
            for (int i = 0; i < _data.Datasets.Count; i++)
            {
                var ds = _data.Datasets[i];
                legend.Items.Add(new BlazorChartLegendItemModel
                {
                    Text = ds.Label ?? $"Dataset {i + 1}",
                    Color = ResolveBackground(ds, i, 0, false),
                    StrokeColor = ResolveBorder(ds, i, 0, false),
                    Hidden = _state.IsDatasetHidden(i) || ds.Hidden,
                    Index = i,
                    UsePointStyle = lo.Labels.UsePointStyle,
                    PointStyle = ds.PointStyle
                });
            }
        }

        if (lo.Reverse) legend.Items.Reverse();
        scene.Legend = legend;
    }

    private BlazorChartTitleModel? BuildTitle(BlazorChartTitleOptions o)
    {
        if (!o.Display || string.IsNullOrEmpty(o.Text)) return null;
        return new BlazorChartTitleModel
        {
            Text = o.Text,
            Color = o.Color,
            Position = o.Position,
            Align = o.Align,
            Font = o.Font
        };
    }
}
