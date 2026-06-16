using System.Globalization;
using BlazorChart.Models;

namespace BlazorChart.Rendering;

public sealed partial class ChartRenderer
{
    private void RenderCircular(ChartScene scene)
    {
        var area = ContentArea();
        double cx = area.CenterX;
        double cy = area.CenterY;
        double maxR = Math.Min(area.Width, area.Height) / 2 - 4;
        if (maxR <= 0) return;

        if (_config.Type == ChartType.PolarArea)
        {
            RenderPolarArea(scene, cx, cy, maxR);
            return;
        }

        var datasets = _data.Datasets.Where((d, i) => !d.Hidden && !_state.IsDatasetHidden(i)).ToList();
        if (datasets.Count == 0) return;

        double rotation = _options.RotationDegrees * Math.PI / 180;
        double circumference = _options.CircumferenceDegrees * Math.PI / 180;
        double cutout = _config.Type == ChartType.Doughnut ? _options.CutoutPercentage / 100.0 : 0;

        double ringOuter = maxR;
        double ringInner = maxR * cutout;
        double ringThickness = (ringOuter - ringInner) / datasets.Count;

        var ctx = new Plugins.PluginContext
        {
            Scene = scene, Config = _config, IsCartesian = false,
            CenterX = cx, CenterY = cy, InnerRadius = ringInner, OuterRadius = ringOuter
        };
        foreach (var plugin in _options.Plugins.Custom) plugin.BeforeDatasetsDraw(ctx);

        for (int ri = 0; ri < datasets.Count; ri++)
        {
            var ds = datasets[ri];
            int dsIndex = _data.Datasets.IndexOf(ds);
            double outer = ringOuter - ringThickness * ri;
            double inner = outer - ringThickness;

            double total = 0;
            for (int i = 0; i < ds.Data.Count; i++)
                if (!_state.IsIndexHidden(i) && ds.Data[i] is { } v) total += Math.Abs(v);
            if (total <= 0) continue;

            double angle = rotation;
            for (int i = 0; i < ds.Data.Count; i++)
            {
                if (_state.IsIndexHidden(i) || ds.Data[i] is not { } v) continue;
                double slice = Math.Abs(v) / total * circumference;
                double a0 = angle;
                double a1 = angle + slice;
                angle = a1;

                string bg = ResolveBackground(ds, dsIndex, i, true);
                bool active = _state.Active == (dsIndex, i);
                double offset = ds.Offset + (active ? 6 : 0);
                double mid = (a0 + a1) / 2;
                double ox = active || ds.Offset > 0 ? Math.Cos(mid) * offset : 0;
                double oy = active || ds.Offset > 0 ? Math.Sin(mid) * offset : 0;

                var path = new SvgPath
                {
                    D = ArcPath(cx + ox, cy + oy, inner, outer, a0, a1),
                    Fill = active ? ColorUtil.Adjust(bg, 0.08) : bg,
                    Stroke = _options.Elements.ArcBorderColor,
                    StrokeWidth = _options.Elements.ArcBorderWidth
                };

                double pct = total > 0 ? Math.Abs(v) / total * 100 : 0;
                scene.Elements.Add(new DataElement
                {
                    Shape = path,
                    DatasetIndex = dsIndex,
                    DataIndex = i,
                    CenterX = cx + Math.Cos(mid) * (inner + outer) / 2,
                    CenterY = cy + Math.Sin(mid) * (inner + outer) / 2,
                    Value = v,
                    SeriesLabel = i < _data.Labels.Count ? _data.Labels[i] : ds.Label,
                    Tooltip = new TooltipInfo
                    {
                        Title = i < _data.Labels.Count ? _data.Labels[i] : null,
                        AnchorX = cx + Math.Cos(mid) * outer,
                        AnchorY = cy + Math.Sin(mid) * outer,
                        Items = { new TooltipItem { Color = bg, Text = $"{v.ToString("0.##", CultureInfo.InvariantCulture)} ({pct.ToString("0.#", CultureInfo.InvariantCulture)}%)" } }
                    }
                });

                if (_options.Plugins.DataLabels.Display && slice > 0.15)
                {
                    double lr = (inner + outer) / 2;
                    AddDataLabel(scene, v, cx + Math.Cos(mid) * lr, cy + Math.Sin(mid) * lr + 4, dsIndex, i);
                }
            }
        }

        foreach (var plugin in _options.Plugins.Custom) plugin.AfterDatasetsDraw(ctx);
    }

    private void RenderPolarArea(ChartScene scene, double cx, double cy, double maxR)
    {
        var ds = _data.Datasets.FirstOrDefault();
        if (ds is null) return;
        int dsIndex = 0;
        int n = ds.Data.Count;
        if (n == 0) return;

        double maxVal = 0;
        for (int i = 0; i < n; i++)
            if (!_state.IsIndexHidden(i) && ds.Data[i] is { } v) maxVal = Math.Max(maxVal, v);
        if (maxVal <= 0) maxVal = 1;

        var rOpts0 = _options.Scales["r"];
        // Reserve room for perimeter point labels.
        if (rOpts0.PointLabels.Display && _data.Labels.Count > 0)
            maxR -= rOpts0.PointLabels.Font.Size + rOpts0.PointLabels.Padding + 6;
        if (maxR <= 0) return;

        var rScale = new AxisScale(_options.Scales["r"], horizontal: false);
        rScale.SetDataRange(0, maxVal);
        rScale.SetPixelRange(0, maxR);

        // Radial grid circles.
        var rOpts = _options.Scales["r"];
        if (rOpts.Display && rOpts.Grid.Display)
        {
            foreach (var t in rScale.Ticks)
            {
                double rr = t.Pixel;
                if (rr <= 0) continue;
                scene.Background.Add(new SvgCircle { Cx = cx, Cy = cy, R = rr, Fill = "none", Stroke = rOpts.Grid.Color, StrokeWidth = rOpts.Grid.LineWidth });
                if (rOpts.Ticks.Display)
                {
                    if (rOpts.ShowLabelBackdrop)
                    {
                        double w = TextMeasure.Width(t.Label, rOpts.Ticks.Font.Size) + 4;
                        scene.Background.Add(new SvgRect { X = cx + 2, Y = cy - rr - rOpts.Ticks.Font.Size * 0.55, Width = w, Height = rOpts.Ticks.Font.Size + 2, Fill = rOpts.BackdropColor });
                    }
                    scene.Background.Add(new SvgText { X = cx + 4, Y = cy - rr, Text = t.Label, Fill = rOpts.Ticks.Color, FontSize = rOpts.Ticks.Font.Size, FontFamily = rOpts.Ticks.Font.Family, Anchor = "start", Baseline = "central" });
                }
            }
        }

        double rotation = (_options.RotationDegrees + rOpts.StartAngle) * Math.PI / 180;
        double sliceAngle = 2 * Math.PI / n;
        double angle = rotation;
        for (int i = 0; i < n; i++)
        {
            if (_state.IsIndexHidden(i) || ds.Data[i] is not { } v) { angle += sliceAngle; continue; }
            double a0 = angle, a1 = angle + sliceAngle;
            angle = a1;
            double r = rScale.PixelFor(v);
            string bg = ResolveBackground(ds, dsIndex, i, true);
            bool active = _state.Active == (dsIndex, i);
            var path = new SvgPath
            {
                D = ArcPath(cx, cy, 0, r, a0, a1),
                Fill = ColorUtil.WithAlpha(active ? ColorUtil.Adjust(bg, -0.1) : bg, 0.7),
                Stroke = bg, StrokeWidth = _options.Elements.ArcBorderWidth
            };
            double mid = (a0 + a1) / 2;
            scene.Elements.Add(new DataElement
            {
                Shape = path,
                DatasetIndex = dsIndex,
                DataIndex = i,
                CenterX = cx + Math.Cos(mid) * r / 2,
                CenterY = cy + Math.Sin(mid) * r / 2,
                Value = v,
                SeriesLabel = i < _data.Labels.Count ? _data.Labels[i] : ds.Label,
                Tooltip = new TooltipInfo
                {
                    Title = i < _data.Labels.Count ? _data.Labels[i] : null,
                    AnchorX = cx + Math.Cos(mid) * r,
                    AnchorY = cy + Math.Sin(mid) * r,
                    Items = { new TooltipItem { Color = bg, Text = v.ToString("0.##", CultureInfo.InvariantCulture) } }
                }
            });
        }

        // Perimeter category (point) labels.
        if (rOpts.PointLabels.Display && _data.Labels.Count > 0)
        {
            var pl = rOpts.PointLabels;
            double a = rotation;
            for (int i = 0; i < n; i++)
            {
                double mid = a + sliceAngle / 2;
                a += sliceAngle;
                if (i >= _data.Labels.Count) continue;
                double lr = maxR + pl.Padding + 6;
                double lx = cx + Math.Cos(mid) * lr;
                double ly = cy + Math.Sin(mid) * lr;
                string anchor = Math.Abs(Math.Cos(mid)) < 0.3 ? "middle" : Math.Cos(mid) > 0 ? "start" : "end";
                string label = pl.Callback?.Invoke(_data.Labels[i], i) ?? _data.Labels[i];
                scene.Background.Add(new SvgText
                {
                    X = lx, Y = ly, Text = label, Fill = pl.Color,
                    FontSize = pl.Font.Size, FontFamily = pl.Font.Family, FontWeight = pl.Font.Weight,
                    Anchor = anchor, Baseline = "central"
                });
            }
        }
    }

    /// <summary>Builds an SVG arc/ring path. Handles full circles.</summary>
    private static string ArcPath(double cx, double cy, double inner, double outer, double a0, double a1)
    {
        double span = a1 - a0;
        bool full = span >= 2 * Math.PI - 1e-3;
        if (full) a1 = a0 + 2 * Math.PI - 1e-3;

        int large = (a1 - a0) > Math.PI ? 1 : 0;
        double x0o = cx + outer * Math.Cos(a0), y0o = cy + outer * Math.Sin(a0);
        double x1o = cx + outer * Math.Cos(a1), y1o = cy + outer * Math.Sin(a1);

        if (inner <= 0.01)
        {
            return $"M {Svg.N(cx)} {Svg.N(cy)} L {Svg.N(x0o)} {Svg.N(y0o)} " +
                   $"A {Svg.N(outer)} {Svg.N(outer)} 0 {large} 1 {Svg.N(x1o)} {Svg.N(y1o)} Z";
        }

        double x0i = cx + inner * Math.Cos(a0), y0i = cy + inner * Math.Sin(a0);
        double x1i = cx + inner * Math.Cos(a1), y1i = cy + inner * Math.Sin(a1);
        return $"M {Svg.N(x0o)} {Svg.N(y0o)} " +
               $"A {Svg.N(outer)} {Svg.N(outer)} 0 {large} 1 {Svg.N(x1o)} {Svg.N(y1o)} " +
               $"L {Svg.N(x1i)} {Svg.N(y1i)} " +
               $"A {Svg.N(inner)} {Svg.N(inner)} 0 {large} 0 {Svg.N(x0i)} {Svg.N(y0i)} Z";
    }
}
