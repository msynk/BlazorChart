using BlazorChart.Models;

namespace BlazorChart.Rendering;

/// <summary>
/// Computes a <see cref="ChartScene"/> (pure SVG primitives + interaction metadata) from a
/// <see cref="ChartConfig"/>. This is the heart of the native Blazor renderer — no JavaScript or
/// canvas is involved.
/// </summary>
public sealed partial class ChartRenderer
{
    private readonly ChartConfig _config;
    private readonly ChartData _data;
    private readonly ChartOptions _options;
    private readonly RenderState _state;
    private readonly double _w;
    private readonly double _h;

    public ChartRenderer(ChartConfig config, RenderState state, double width, double height)
    {
        _config = config;
        _data = config.Data;
        _options = config.Options;
        _state = state;
        _w = width;
        _h = height;
    }

    public ChartScene Render()
    {
        var scene = new ChartScene { Width = _w, Height = _h };
        scene.Title = BuildTitle(_options.Plugins.Title);
        scene.Subtitle = BuildTitle(_options.Plugins.Subtitle);
        EnsureScales();

        switch (_config.Type)
        {
            case ChartType.Pie:
            case ChartType.Doughnut:
            case ChartType.PolarArea:
                scene.IsRadialOrCircular = true;
                RenderCircular(scene);
                break;
            case ChartType.Radar:
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
    private string RegisterGradient(ChartScene scene, GradientBase grad)
    {
        string id = $"bcgrad{_gradSeq++}";
        scene.Defs.Add(new GradientDef(id, grad));
        return $"url(#{id})";
    }

    /// <summary>Registers a pattern on the scene and returns a <c>url(#id)</c> fill reference.</summary>
    private string RegisterPattern(ChartScene scene, FillPattern pattern)
    {
        string id = $"bcpat{_gradSeq++}";
        scene.Patterns.Add(new PatternDef(id, pattern));
        return $"url(#{id})";
    }

    private static double EstimateTextWidth(string? text, double fontSize)
        => TextMeasure.Width(text, fontSize);

    private Area ContentArea()
    {
        var p = _options.Layout.Padding;
        return new Area(p.Left, p.Top, _w - p.Right, _h - p.Bottom);
    }

    /// <summary>Builds a scriptable-options context for a data element.</summary>
    private ScriptableContext Ctx(ChartDataset ds, int dsIndex, int dataIndex, double? value = null)
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
        return new ScriptableContext
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
    private string ResolveBackground(ChartDataset ds, int dsIndex, int dataIndex, bool perIndexPalette, double? value = null)
    {
        if (ds.BackgroundColorFn is { } fn && fn(Ctx(ds, dsIndex, dataIndex, value)) is { } c) return c;
        if (ds.BackgroundColors is { Count: > 0 } list)
            return list[dataIndex % list.Count];
        if (!string.IsNullOrEmpty(ds.BackgroundColor))
            return ds.BackgroundColor!;
        return perIndexPalette ? ColorUtil.Palette(dataIndex) : ColorUtil.Palette(dsIndex);
    }

    private string ResolveBorder(ChartDataset ds, int dsIndex, int dataIndex, bool perIndexPalette, double? value = null)
    {
        if (ds.BorderColorFn is { } fn && fn(Ctx(ds, dsIndex, dataIndex, value)) is { } c) return c;
        if (ds.BorderColors is { Count: > 0 } list)
            return list[dataIndex % list.Count];
        if (!string.IsNullOrEmpty(ds.BorderColor))
            return ds.BorderColor!;
        return perIndexPalette ? ColorUtil.Palette(dataIndex) : ColorUtil.Palette(dsIndex);
    }

    private string FormatTooltipValue(ChartDataset ds, double value)
    {
        var t = _options.Plugins.Tooltip;
        if (t.LabelFormatter is { } f) return f(ds.Label ?? "", value);
        string label = string.IsNullOrEmpty(ds.Label) ? "" : ds.Label + ": ";
        return label + value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Builds a tooltip item context for callbacks.</summary>
    private TooltipItemContext BuildTooltipItem(ChartDataset ds, int dsIndex, int dataIndex, double value, string color)
    {
        double? vx = ds.Points is { } pts && dataIndex < pts.Count ? pts[dataIndex].X : null;
        return new TooltipItemContext
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
    private string BuildItemText(ChartDataset ds, int dsIndex, int dataIndex, double value, string color)
    {
        var t = _options.Plugins.Tooltip;
        if (t.Callbacks.Label is { } cb && cb(BuildTooltipItem(ds, dsIndex, dataIndex, value, color)) is { } txt)
            return txt;
        return FormatTooltipValue(ds, value);
    }

    private void EnsureScales()
    {
        if (_config.Type is ChartType.Pie or ChartType.Doughnut)
            return;

        if (_config.Type is ChartType.PolarArea or ChartType.Radar)
        {
            _options.GetOrAddScale("r", ScaleType.RadialLinear);
            return;
        }

        // Cartesian: ensure x and y exist with sensible defaults.
        var x = _options.GetOrAddScale("x", _config.Type is ChartType.Scatter or ChartType.Bubble
            ? ScaleType.Linear : ScaleType.Category);
        x.Position ??= Position.Bottom;

        // Additional x axes referenced by datasets (default linear, bottom).
        foreach (var id in _data.Datasets.Select(d => d.XAxisID).Distinct())
        {
            if (id == "x" || string.IsNullOrEmpty(id)) continue;
            var x2 = _options.GetOrAddScale(id, ScaleType.Linear);
            x2.Position ??= Position.Bottom;
        }

        // Gather y axis ids referenced by datasets.
        var yIds = _data.Datasets.Select(d => d.YAxisID).Distinct().ToList();
        foreach (var id in yIds)
        {
            var y = _options.GetOrAddScale(id, ScaleType.Linear);
            y.Position ??= Position.Left;
        }
        if (!_options.Scales.ContainsKey("y"))
        {
            var y = _options.GetOrAddScale("y", ScaleType.Linear);
            y.Position ??= Position.Left;
        }
    }

    private void BuildLegend(ChartScene scene)
    {
        var lo = _options.Plugins.Legend;
        if (!lo.Display) return;

        var legend = new LegendModel
        {
            Position = lo.Position,
            Align = lo.Align,
            Labels = lo.Labels,
            Title = lo.Title,
            OnClickToggle = lo.OnClickToggle
        };

        if (_config.Type is ChartType.Pie or ChartType.Doughnut or ChartType.PolarArea)
        {
            // One legend entry per data index (label).
            var ds = _data.Datasets.FirstOrDefault();
            int n = _data.Labels.Count;
            for (int i = 0; i < n; i++)
            {
                legend.Items.Add(new LegendItemModel
                {
                    Text = _data.Labels[i],
                    Color = ds is null ? ColorUtil.Palette(i) : ResolveBackground(ds, 0, i, true),
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
                legend.Items.Add(new LegendItemModel
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

    private TitleModel? BuildTitle(TitleOptions o)
    {
        if (!o.Display || string.IsNullOrEmpty(o.Text)) return null;
        return new TitleModel
        {
            Text = o.Text,
            Color = o.Color,
            Position = o.Position,
            Align = o.Align,
            Font = o.Font
        };
    }
}
