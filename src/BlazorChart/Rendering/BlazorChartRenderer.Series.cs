using System.Text;

namespace BlazorChart;

public sealed partial class BlazorChartRenderer
{
    private void DrawBars(BlazorChartScene scene, BlazorChartArea plot, BlazorChartAxisScale indexScale,
        Dictionary<string, BlazorChartAxisScale> valueScales, List<(BlazorChartDataset d, int i)> barItems, bool indexIsCategory)
    {
        if (barItems.Count == 0) return;

        // Build slots: stacked datasets share a slot, others get their own.
        var slotKeys = new List<string>();
        var dsSlot = new Dictionary<int, int>();
        foreach (var (ds, i) in barItems)
        {
            bool stacked = valueScales[ds.YAxisID].Options.Stacked;
            string key = stacked ? $"stack:{ds.Stack ?? "default"}:{ds.YAxisID}" : $"ds:{i}";
            if (!slotKeys.Contains(key)) slotKeys.Add(key);
            dsSlot[i] = slotKeys.IndexOf(key);
        }
        int slotCount = Math.Max(1, slotKeys.Count);

        double band = indexScale.BandWidth();
        var first = barItems[0].d;
        double categorySize = band * first.CategoryPercentage;
        double slotSize = categorySize / slotCount;

        var stackOffset = new Dictionary<(int slot, int di, int sign), double>();

        // Precompute per-(slot,index) totals for 100% stacking.
        var stack100Totals = new Dictionary<(int slot, int di), double>();
        foreach (var (ds, i) in barItems)
        {
            var so = valueScales[ds.YAxisID].Options;
            if (!(so.Stacked && so.Stacked100)) continue;
            int slot = dsSlot[i];
            for (int di = 0; di < ds.Data.Count; di++)
                if (ds.Data[di] is { } v)
                    stack100Totals[(slot, di)] = stack100Totals.GetValueOrDefault((slot, di), 0) + Math.Abs(v);
        }

        foreach (var (ds, i) in barItems)
        {
            var vScale = valueScales[ds.YAxisID];
            bool stacked = vScale.Options.Stacked;
            bool stacked100 = stacked && vScale.Options.Stacked100;
            int slot = dsSlot[i];
            double barSize = slotSize * ds.BarPercentage;
            if (ds.BarThickness is { } bt) barSize = bt;
            if (ds.MaxBarThickness is { } mbt) barSize = Math.Min(barSize, mbt);

            int count = ds.Count;
            string? patternFill = ds.BackgroundPattern is { } pat ? RegisterPattern(scene, pat) : null;
            for (int di = 0; di < count; di++)
            {
                double baseVal, topVal, tooltipVal;
                bool isRange = ds.RangeData is { } rd && di < rd.Count && rd[di].HasValue;

                if (isRange)
                {
                    var (low, high) = ds.RangeData![di]!.Value;
                    baseVal = low; topVal = high; tooltipVal = high;
                }
                else
                {
                    if (di >= ds.Data.Count || ds.Data[di] is not { } v) continue;
                    double value = v;
                    if (stacked100)
                    {
                        double total = stack100Totals.GetValueOrDefault((slot, di), 0);
                        if (total > 0) value = value / total * 100;
                    }
                    tooltipVal = v;
                    int sign = value >= 0 ? 1 : -1;
                    if (stacked)
                    {
                        baseVal = stackOffset.GetValueOrDefault((slot, di, sign), 0);
                        topVal = baseVal + value;
                        stackOffset[(slot, di, sign)] = topVal;
                    }
                    else
                    {
                        baseVal = Math.Clamp(0, Math.Min(vScale.Min, vScale.Max), Math.Max(vScale.Min, vScale.Max));
                        topVal = value;
                    }
                }

                double centerAlong = indexIsCategory ? indexScale.PixelForIndex(di, true) : indexScale.PixelFor(di);
                double slotCenter = centerAlong - categorySize / 2 + slot * slotSize + slotSize / 2;

                string bg = ResolveBackground(ds, i, di, false, tooltipVal);
                string border = ResolveBorder(ds, i, di, false, tooltipVal);
                if (patternFill is not null) bg = patternFill;
                int signFinal = topVal >= baseVal ? 1 : -1;
                double inflate = ds.InflateAmount ?? 0;
                BlazorChartSvgRect rect;
                double cx, cy, originX, originY;

                if (IsVertical)
                {
                    double yBase = vScale.PixelFor(baseVal);
                    double yTop = vScale.PixelFor(topVal);
                    rect = new BlazorChartSvgRect
                    {
                        X = slotCenter - barSize / 2 - inflate,
                        Y = Math.Min(yBase, yTop) - inflate,
                        Width = barSize + inflate * 2,
                        Height = Math.Max(1, Math.Abs(yBase - yTop)) + inflate * 2,
                        Fill = bg,
                        Rx = ds.BorderRadius
                    };
                    cx = slotCenter; cy = yTop;
                    originX = slotCenter; originY = yBase;   // grow from the baseline
                    scene.BarBaseline = yBase;
                }
                else
                {
                    double xBase = vScale.PixelFor(baseVal);
                    double xTop = vScale.PixelFor(topVal);
                    rect = new BlazorChartSvgRect
                    {
                        X = Math.Min(xBase, xTop) - inflate,
                        Y = slotCenter - barSize / 2 - inflate,
                        Width = Math.Max(1, Math.Abs(xBase - xTop)) + inflate * 2,
                        Height = barSize + inflate * 2,
                        Fill = bg,
                        Rx = ds.BorderRadius
                    };
                    cx = xTop; cy = slotCenter;
                    originX = xBase; originY = slotCenter;   // grow from the baseline
                    scene.BarBaseline = xBase;
                }

                // Per-corner radius emits a rounded path instead of a plain rect.
                BlazorChartSvgNode shapeNode = rect;
                bool perCorner = ds.BorderRadiusCorners is { } c0 &&
                    (c0.TopLeft > 0 || c0.TopRight > 0 || c0.BottomRight > 0 || c0.BottomLeft > 0);
                if (perCorner)
                    shapeNode = new BlazorChartSvgPath
                    {
                        D = RoundedRectPath(rect.X, rect.Y, rect.Width, rect.Height, ds.BorderRadiusCorners!.Value),
                        Fill = bg,
                        Stroke = ds.BorderWidth > 0 ? border : null,
                        StrokeWidth = ds.BorderWidth
                    };

                // Border honoring borderSkipped (uniform rects only; per-corner paths carry their own stroke).
                BlazorChartSvgNode? borderNode = null;
                if (!perCorner && ds.BorderWidth > 0)
                {
                    var skip = ResolveSkip(isRange ? BlazorChartBorderSkipped.None : ds.BorderSkipped, IsVertical, signFinal);
                    if (skip == BlazorChartBorderSkipped.None)
                    {
                        rect.Stroke = border;
                        rect.StrokeWidth = ds.BorderWidth;
                    }
                    else
                    {
                        // Drawn as part of the element so it animates together with the fill.
                        borderNode = new BlazorChartSvgPath
                        {
                            D = BarBorderPath(rect, skip), Fill = "none", Stroke = border, StrokeWidth = ds.BorderWidth
                        };
                    }
                }

                string text = isRange
                    ? $"{(ds.Label is null ? "" : ds.Label + ": ")}[{baseVal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}, {topVal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}]"
                    : BuildItemText(ds, i, di, tooltipVal, bg);

                // Each bar grows from its own baseline in the correct direction (size change, not a slide).
                string enterAnim = IsVertical ? "bc-scale-y" : "bc-scale-x";

                scene.Elements.Add(new BlazorChartDataElement
                {
                    Shape = shapeNode,
                    BorderShape = borderNode,
                    EnterAnim = enterAnim,
                    AnimOriginX = originX,
                    AnimOriginY = originY,
                    DatasetIndex = i,
                    DataIndex = di,
                    CenterX = cx,
                    CenterY = cy,
                    Value = tooltipVal,
                    SeriesLabel = ds.Label,
                    Tooltip = new BlazorChartTooltipInfo
                    {
                        Title = di < _data.Labels.Count ? _data.Labels[di] : null,
                        AnchorX = cx,
                        AnchorY = IsVertical ? Math.Min(rect.Y, cy) : cy,
                        Items = { new BlazorChartTooltipItem { Color = bg, Text = text } }
                    }
                });

                if (!isRange) AddDataLabel(scene, tooltipVal, cx, IsVertical ? rect.Y - 4 : cx, i, di);
            }
        }
    }

    /// <summary>Resolves Start/End border-skip to a concrete edge based on orientation and sign.</summary>
    private static BlazorChartBorderSkipped ResolveSkip(BlazorChartBorderSkipped s, bool vertical, int sign) => s switch
    {
        BlazorChartBorderSkipped.Start => vertical ? (sign >= 0 ? BlazorChartBorderSkipped.Bottom : BlazorChartBorderSkipped.Top)
                                        : (sign >= 0 ? BlazorChartBorderSkipped.Left : BlazorChartBorderSkipped.Right),
        BlazorChartBorderSkipped.End => vertical ? (sign >= 0 ? BlazorChartBorderSkipped.Top : BlazorChartBorderSkipped.Bottom)
                                      : (sign >= 0 ? BlazorChartBorderSkipped.Right : BlazorChartBorderSkipped.Left),
        _ => s
    };

    /// <summary>Builds an open border path for a bar, omitting the skipped edge.</summary>
    private static string BarBorderPath(BlazorChartSvgRect r, BlazorChartBorderSkipped skip)
    {
        double x1 = r.X, y1 = r.Y, x2 = r.X + r.Width, y2 = r.Y + r.Height;
        var sb = new StringBuilder();
        void Edge(double ax, double ay, double bx, double by)
            => sb.Append($"M {BlazorChartSvg.N(ax)} {BlazorChartSvg.N(ay)} L {BlazorChartSvg.N(bx)} {BlazorChartSvg.N(by)} ");
        if (skip != BlazorChartBorderSkipped.Top) Edge(x1, y1, x2, y1);
        if (skip != BlazorChartBorderSkipped.Right) Edge(x2, y1, x2, y2);
        if (skip != BlazorChartBorderSkipped.Bottom) Edge(x2, y2, x1, y2);
        if (skip != BlazorChartBorderSkipped.Left) Edge(x1, y2, x1, y1);
        return sb.ToString().Trim();
    }

    private void DrawLine(BlazorChartScene scene, BlazorChartArea plot, BlazorChartAxisScale indexScale, BlazorChartAxisScale vScale,
        BlazorChartDataset ds, int dsIndex, bool indexIsCategory, bool centered)
    {
        var pts = new List<(double x, double y, int di, double v)>();

        if (ds.Points is { } points)
        {
            // Line over a linear/time x axis driven by explicit (x, y) points.
            var ordered = points.Select((p, di) => (p, di)).OrderBy(t => t.p.X).ToList();
            foreach (var (p, di) in ordered)
                pts.Add((indexScale.PixelFor(p.X), vScale.PixelFor(p.Y), di, p.Y));
            FlushLine(scene, plot, vScale, ds, dsIndex, pts, indexScale, indexIsCategory, centered);
            return;
        }

        for (int di = 0; di < ds.Data.Count; di++)
        {
            if (ds.Data[di] is not { } v)
            {
                if (!ds.SpanGaps) { FlushLine(scene, plot, vScale, ds, dsIndex, pts, indexScale, indexIsCategory, centered); pts.Clear(); }
                continue;
            }
            double x = indexIsCategory ? indexScale.PixelForIndex(di, centered) : indexScale.PixelFor(di);
            double y = vScale.PixelFor(v);
            pts.Add((x, y, di, v));
        }
        FlushLine(scene, plot, vScale, ds, dsIndex, pts, indexScale, indexIsCategory, centered);
    }

    private void FlushLine(BlazorChartScene scene, BlazorChartArea plot, BlazorChartAxisScale vScale, BlazorChartDataset ds, int dsIndex,
        List<(double x, double y, int di, double v)> pts,
        BlazorChartAxisScale? indexScale = null, bool indexIsCategory = false, bool centered = false)
    {
        if (pts.Count == 0) return;

        var dec = _options.Plugins.Decimation;
        if (dec.Enabled && pts.Count > dec.Threshold && dec.Samples >= 2 && dec.Samples < pts.Count)
            pts = BlazorChartDecimation.Lttb(pts, dec.Samples);

        string border = ResolveBorder(ds, dsIndex, 0, false);
        var xy = pts.Select(p => (p.x, p.y)).ToList();
        string d = BuildPath(xy, ds.Tension, ds.Stepped, ds.CubicInterpolationMode);

        bool progressive = _options.Animation.Animate && _options.Animation.Progressive;
        if (progressive) scene.ProgressiveDraw = true;

        if (ds.Fill != BlazorChartFillMode.None && ds.ShowLine)
        {
            string? fillD = null;

            // Fill to another dataset's line (range area).
            if (ds.Fill == BlazorChartFillMode.Dataset && ds.FillTargetIndex is { } ti
                && ti >= 0 && ti < _data.Datasets.Count && indexScale is not null)
            {
                var target = ComputeLinePoints(_data.Datasets[ti], indexScale, vScale, indexIsCategory, centered);
                if (target.Count > 0)
                    fillD = AreaBetween(xy, ds.Tension, ds.Stepped, target.Select(p => (p.x, p.y)).ToList());
            }

            if (fillD is null)
            {
                double baseVal = ds.Fill switch
                {
                    BlazorChartFillMode.Start => Math.Min(vScale.Min, vScale.Max),
                    BlazorChartFillMode.End => Math.Max(vScale.Min, vScale.Max),
                    BlazorChartFillMode.Value => ds.FillValue ?? 0,
                    _ => Math.Clamp(0, Math.Min(vScale.Min, vScale.Max), Math.Max(vScale.Min, vScale.Max))
                };
                double baseY = vScale.PixelFor(baseVal);
                fillD = d + $" L {BlazorChartSvg.N(xy[^1].x)} {BlazorChartSvg.N(baseY)} L {BlazorChartSvg.N(xy[0].x)} {BlazorChartSvg.N(baseY)} Z";
            }

            scene.Series.Add(new BlazorChartSvgPath { D = fillD, Fill = ResolveFill(scene, ds, border, plot), Stroke = null, AnimateFade = progressive });
        }

        if (ds.ShowLine)
        {
            if (ds.Segment is { } seg)
            {
                // Draw each consecutive segment with its own resolved style.
                double defWidth = ds.BorderWidth <= 1 ? _options.Elements.LineBorderWidth : ds.BorderWidth;
                for (int k = 0; k < pts.Count - 1; k++)
                {
                    var a = pts[k];
                    var b = pts[k + 1];
                    var sctx = new BlazorChartSegmentContext(a.di, b.di, a.v, b.v);
                    string color = seg.BorderColor?.Invoke(sctx) ?? border;
                    double width = seg.BorderWidth?.Invoke(sctx) ?? defWidth;
                    var dash = seg.BorderDash?.Invoke(sctx);
                    scene.Series.Add(new BlazorChartSvgPath
                    {
                        D = $"M {BlazorChartSvg.N(a.x)} {BlazorChartSvg.N(a.y)} L {BlazorChartSvg.N(b.x)} {BlazorChartSvg.N(b.y)}",
                        Fill = "none", Stroke = color, StrokeWidth = width,
                        Dash = dash is null ? "" : BlazorChartSvg.Dash(dash),
                        LineCap = ds.BorderCapStyle, LineJoin = ds.BorderJoinStyle,
                        AnimateFade = progressive
                    });
                }
            }
            else
            {
                bool dashed = ds.BorderDash is { Count: > 0 };
                scene.Series.Add(new BlazorChartSvgPath
                {
                    D = d, Fill = "none", Stroke = border,
                    StrokeWidth = ds.BorderWidth <= 1 ? _options.Elements.LineBorderWidth : ds.BorderWidth,
                    Dash = BlazorChartSvg.Dash(ds.BorderDash), LineCap = ds.BorderCapStyle, LineJoin = ds.BorderJoinStyle,
                    // Draw-on reveals the stroke left to right; dashed strokes can't (dasharray is in use), so they fade.
                    AnimateDraw = progressive && !dashed,
                    AnimateFade = progressive && dashed
                });
            }
        }

        if (ds.PointRadius > 0 || ds.PointStyle != BlazorChartPointStyle.None)
            foreach (var p in pts)
                AddPoint(scene, ds, dsIndex, p.di, p.x, p.y, p.v, ds.PointRadius, border);
    }

    /// <summary>Resolves an area fill paint (pattern, gradient, explicit color, or translucent border).</summary>
    private string ResolveFill(BlazorChartScene scene, BlazorChartDataset ds, string border, BlazorChartArea plot)
    {
        if (ds.BackgroundPattern is { } pat) return RegisterPattern(scene, pat);
        if (ds.FillGradient is { Stops.Count: > 0 } g) return RegisterGradient(scene, g);
        return ds.FillColor ?? BlazorChartColorUtil.WithAlpha(border, 0.2);
    }

    /// <summary>Computes the pixel polyline for a dataset's line (nulls skipped).</summary>
    private List<(double x, double y, int di, double v)> ComputeLinePoints(
        BlazorChartDataset ds, BlazorChartAxisScale indexScale, BlazorChartAxisScale vScale, bool indexIsCategory, bool centered)
    {
        var pts = new List<(double x, double y, int di, double v)>();
        if (ds.Points is { } points)
        {
            foreach (var (p, di) in points.Select((p, i) => (p, i)).OrderBy(t => t.p.X))
                pts.Add((indexScale.PixelFor(p.X), vScale.PixelFor(p.Y), di, p.Y));
            return pts;
        }
        for (int di = 0; di < ds.Data.Count; di++)
        {
            if (ds.Data[di] is not { } v) continue;
            double x = indexIsCategory ? indexScale.PixelForIndex(di, centered) : indexScale.PixelFor(di);
            pts.Add((x, vScale.PixelFor(v), di, v));
        }
        return pts;
    }

    /// <summary>Builds a closed area path between an upper and lower polyline.</summary>
    private static string AreaBetween(List<(double x, double y)> top, double tension, BlazorChartSteppedLine stepped,
        List<(double x, double y)> bottom)
    {
        var sb = new StringBuilder(BuildPath(top, tension, stepped));
        var rev = new List<(double x, double y)>(bottom);
        rev.Reverse();
        sb.Append(' ').Append('L').Append(' ').Append(BlazorChartSvg.N(rev[0].x)).Append(' ').Append(BlazorChartSvg.N(rev[0].y));
        var tail = BuildPath(rev, tension, stepped);
        // Replace leading "M" of the tail with "L" so it connects.
        if (tail.StartsWith('M')) tail = "L" + tail[1..];
        sb.Append(' ').Append(tail).Append(" Z");
        return sb.ToString();
    }

    /// <summary>Draws stacked line areas (cumulative) for datasets on a stacked value axis.</summary>
    private void DrawStackedAreas(BlazorChartScene scene, BlazorChartArea plot, BlazorChartAxisScale indexScale,
        Dictionary<string, BlazorChartAxisScale> valueScales, List<(BlazorChartDataset d, int i)> items,
        bool indexIsCategory, bool centered)
    {
        // Group by (axis, stack); within each group accumulate per data index.
        foreach (var group in items.GroupBy(t => (t.d.YAxisID, t.d.Stack ?? "default")))
        {
            var vScale = valueScales[group.Key.YAxisID];
            var cumulative = new Dictionary<int, double>();

            foreach (var (ds, i) in group)
            {
                var topPts = new List<(double x, double y, int di, double v)>();
                var basePts = new List<(double x, double y)>();
                for (int di = 0; di < ds.Data.Count; di++)
                {
                    if (ds.Data[di] is not { } v) continue;
                    double baseVal = cumulative.GetValueOrDefault(di, 0);
                    double topVal = baseVal + v;
                    cumulative[di] = topVal;
                    double x = indexIsCategory ? indexScale.PixelForIndex(di, centered) : indexScale.PixelFor(di);
                    topPts.Add((x, vScale.PixelFor(topVal), di, topVal));
                    basePts.Add((x, vScale.PixelFor(baseVal)));
                }
                if (topPts.Count == 0) continue;

                string border = ResolveBorder(ds, i, 0, false);
                var topXy = topPts.Select(p => (p.x, p.y)).ToList();

                bool progressive = _options.Animation.Animate && _options.Animation.Progressive;
                if (progressive) scene.ProgressiveDraw = true;
                bool dashed = ds.BorderDash is { Count: > 0 };

                if (ds.Fill != BlazorChartFillMode.None)
                {
                    string fillD = AreaBetween(topXy, ds.Tension, ds.Stepped, basePts);
                    scene.Series.Add(new BlazorChartSvgPath { D = fillD, Fill = ResolveFill(scene, ds, border, plot), Stroke = null, AnimateFade = progressive });
                }

                scene.Series.Add(new BlazorChartSvgPath
                {
                    D = BuildPath(topXy, ds.Tension, ds.Stepped), Fill = "none", Stroke = border,
                    StrokeWidth = ds.BorderWidth <= 1 ? _options.Elements.LineBorderWidth : ds.BorderWidth,
                    Dash = BlazorChartSvg.Dash(ds.BorderDash), LineCap = ds.BorderCapStyle, LineJoin = ds.BorderJoinStyle,
                    AnimateDraw = progressive && !dashed,
                    AnimateFade = progressive && dashed
                });

                if (ds.PointRadius > 0)
                    foreach (var p in topPts)
                        AddPoint(scene, ds, i, p.di, p.x, p.y, ds.Data[p.di] ?? 0, ds.PointRadius, border);
            }
        }
    }

    private void DrawScatter(BlazorChartScene scene, BlazorChartArea plot, BlazorChartAxisScale indexScale, BlazorChartAxisScale vScale,
        BlazorChartDataset ds, int dsIndex, bool bubble)
    {
        if (ds.Points is not { } points) return;
        string border = ResolveBorder(ds, dsIndex, 0, false);
        for (int di = 0; di < points.Count; di++)
        {
            var p = points[di];
            double x = indexScale.PixelFor(p.X);
            double y = vScale.PixelFor(p.Y);
            double r = bubble ? (p.R ?? 5) : ds.PointRadius <= 3 ? 4 : ds.PointRadius;
            AddPoint(scene, ds, dsIndex, di, x, y, p.Y, r, border, p.X);
        }
    }

    private void AddPoint(BlazorChartScene scene, BlazorChartDataset ds, int dsIndex, int di,
        double x, double y, double value, double radius, string border, double? xValue = null)
    {
        var ctx = Ctx(ds, dsIndex, di, value);
        bool active = _state.Active == (dsIndex, di);

        double r = ds.PointRadiusFn?.Invoke(ctx) ?? radius;
        var style = ds.PointStyleFn?.Invoke(ctx) ?? ds.PointStyle;
        string fill = ds.PointBackgroundColorFn?.Invoke(ctx) ?? ds.PointBackgroundColor ?? ResolveBackground(ds, dsIndex, di, false, value);
        string stroke = ds.PointBorderColorFn?.Invoke(ctx) ?? ds.PointBorderColor ?? border;
        double bw = ds.PointBorderWidth;

        if (active)
        {
            r = Math.Max(r, ds.PointHoverRadius);
            if (ds.PointHoverBackgroundColor is { } hb) fill = hb;
            if (ds.PointHoverBorderColor is { } hbc) stroke = hbc;
            if (ds.PointHoverBorderWidth is { } hbw) bw = hbw;
        }

        var shape = BlazorChartPointShapes.Build(style, x, y, r, fill, stroke, bw);
        if (shape is null) return;

        bool cartesian = _config.Type is not (BlazorChartType.Pie or BlazorChartType.Doughnut or BlazorChartType.PolarArea or BlazorChartType.Radar);

        string text = xValue is { } xv
            ? $"({xv.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}, {value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})"
            : BuildItemText(ds, dsIndex, di, value, fill);

        scene.Elements.Add(new BlazorChartDataElement
        {
            Shape = shape,
            EnterAnim = cartesian ? "bc-pop" : null,
            AnimOriginX = x,
            AnimOriginY = y,
            DatasetIndex = dsIndex,
            DataIndex = di,
            CenterX = x,
            CenterY = y,
            Value = value,
            SeriesLabel = ds.Label,
            Tooltip = new BlazorChartTooltipInfo
            {
                Title = xValue is null && di < _data.Labels.Count ? _data.Labels[di] : ds.Label,
                AnchorX = x,
                AnchorY = y,
                Items = { new BlazorChartTooltipItem { Color = fill, Text = text, PointStyle = style } }
            }
        });
    }

    private void AddDataLabel(BlazorChartScene scene, double value, double x, double y, int dsIndex = 0, int dataIndex = 0)
    {
        var dl = _options.Plugins.DataLabels;
        if (!dl.Display) return;
        if (dl.DisplayFn is { } show && !show(value, dsIndex, dataIndex)) return;

        string text = dl.FormatterCtx?.Invoke(value, dsIndex, dataIndex)
            ?? dl.Formatter?.Invoke(value)
            ?? value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        if (dl.BackgroundColor is { } bgc)
        {
            double w = BlazorChartTextMeasure.Width(text, dl.Font.Size, dl.Font.Weight) + dl.Padding * 2;
            double h = dl.Font.Size + dl.Padding * 2;
            scene.Foreground.Add(new BlazorChartSvgRect
            {
                X = x - w / 2, Y = y - h / 2, Width = w, Height = h, Rx = dl.BorderRadius, Fill = bgc
            });
        }

        scene.Foreground.Add(new BlazorChartSvgText
        {
            X = x, Y = y,
            Text = text,
            Fill = dl.Color, FontFamily = dl.Font.Family, FontSize = dl.Font.Size, FontWeight = dl.Font.Weight,
            Anchor = "middle", Baseline = dl.BackgroundColor is null ? "auto" : "central", Rotation = dl.Rotation
        });
    }

    /// <summary>Builds an SVG path through points, supporting bezier tension, monotone and stepped lines.</summary>
    private static string BuildPath(List<(double x, double y)> p, double tension, BlazorChartSteppedLine stepped,
        BlazorChartCubicInterpolationMode mode = BlazorChartCubicInterpolationMode.Default)
    {
        if (p.Count == 0) return "";
        var sb = new StringBuilder();
        sb.Append($"M {BlazorChartSvg.N(p[0].x)} {BlazorChartSvg.N(p[0].y)}");
        if (p.Count == 1) return sb.ToString();

        if (stepped != BlazorChartSteppedLine.False)
        {
            for (int i = 1; i < p.Count; i++)
            {
                var a = p[i - 1]; var b = p[i];
                switch (stepped)
                {
                    case BlazorChartSteppedLine.Before:
                        sb.Append($" L {BlazorChartSvg.N(a.x)} {BlazorChartSvg.N(b.y)} L {BlazorChartSvg.N(b.x)} {BlazorChartSvg.N(b.y)}");
                        break;
                    case BlazorChartSteppedLine.After:
                        sb.Append($" L {BlazorChartSvg.N(b.x)} {BlazorChartSvg.N(a.y)} L {BlazorChartSvg.N(b.x)} {BlazorChartSvg.N(b.y)}");
                        break;
                    default: // middle
                        double mx = (a.x + b.x) / 2;
                        sb.Append($" L {BlazorChartSvg.N(mx)} {BlazorChartSvg.N(a.y)} L {BlazorChartSvg.N(mx)} {BlazorChartSvg.N(b.y)} L {BlazorChartSvg.N(b.x)} {BlazorChartSvg.N(b.y)}");
                        break;
                }
            }
            return sb.ToString();
        }

        if (mode == BlazorChartCubicInterpolationMode.Monotone && p.Count > 2)
            return MonotonePath(p);

        if (tension <= 0)
        {
            for (int i = 1; i < p.Count; i++)
                sb.Append($" L {BlazorChartSvg.N(p[i].x)} {BlazorChartSvg.N(p[i].y)}");
            return sb.ToString();
        }

        // Cardinal spline -> cubic beziers.
        for (int i = 0; i < p.Count - 1; i++)
        {
            var p0 = p[Math.Max(0, i - 1)];
            var p1 = p[i];
            var p2 = p[i + 1];
            var p3 = p[Math.Min(p.Count - 1, i + 2)];
            double c1x = p1.x + (p2.x - p0.x) / 6 * tension;
            double c1y = p1.y + (p2.y - p0.y) / 6 * tension;
            double c2x = p2.x - (p3.x - p1.x) / 6 * tension;
            double c2y = p2.y - (p3.y - p1.y) / 6 * tension;
            sb.Append($" C {BlazorChartSvg.N(c1x)} {BlazorChartSvg.N(c1y)}, {BlazorChartSvg.N(c2x)} {BlazorChartSvg.N(c2y)}, {BlazorChartSvg.N(p2.x)} {BlazorChartSvg.N(p2.y)}");
        }
        return sb.ToString();
    }

    /// <summary>Monotone cubic interpolation (Fritsch–Carlson) emitted as cubic beziers — never overshoots.</summary>
    private static string MonotonePath(List<(double x, double y)> p)
    {
        int n = p.Count;
        var dx = new double[n - 1];
        var dy = new double[n - 1];
        var d = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            dx[i] = p[i + 1].x - p[i].x;
            dy[i] = p[i + 1].y - p[i].y;
            d[i] = dx[i] != 0 ? dy[i] / dx[i] : 0;
        }

        var m = new double[n];
        m[0] = d[0];
        m[n - 1] = d[n - 2];
        for (int i = 1; i < n - 1; i++)
            m[i] = d[i - 1] * d[i] <= 0 ? 0 : (d[i - 1] + d[i]) / 2;

        for (int i = 0; i < n - 1; i++)
        {
            if (d[i] == 0) { m[i] = 0; m[i + 1] = 0; continue; }
            double a = m[i] / d[i];
            double b = m[i + 1] / d[i];
            double s = a * a + b * b;
            if (s > 9)
            {
                double t = 3 / Math.Sqrt(s);
                m[i] = t * a * d[i];
                m[i + 1] = t * b * d[i];
            }
        }

        var sb = new StringBuilder();
        sb.Append($"M {BlazorChartSvg.N(p[0].x)} {BlazorChartSvg.N(p[0].y)}");
        for (int i = 0; i < n - 1; i++)
        {
            double c1x = p[i].x + dx[i] / 3;
            double c1y = p[i].y + m[i] * dx[i] / 3;
            double c2x = p[i + 1].x - dx[i] / 3;
            double c2y = p[i + 1].y - m[i + 1] * dx[i] / 3;
            sb.Append($" C {BlazorChartSvg.N(c1x)} {BlazorChartSvg.N(c1y)}, {BlazorChartSvg.N(c2x)} {BlazorChartSvg.N(c2y)}, {BlazorChartSvg.N(p[i + 1].x)} {BlazorChartSvg.N(p[i + 1].y)}");
        }
        return sb.ToString();
    }

    /// <summary>Builds a rounded-rectangle path with per-corner radii.</summary>
    private static string RoundedRectPath(double x, double y, double w, double h, BlazorChartBorderRadiusCorners c)
    {
        double max = Math.Min(w, h) / 2;
        double tl = Math.Clamp(c.TopLeft, 0, max);
        double tr = Math.Clamp(c.TopRight, 0, max);
        double br = Math.Clamp(c.BottomRight, 0, max);
        double bl = Math.Clamp(c.BottomLeft, 0, max);
        return $"M {BlazorChartSvg.N(x + tl)} {BlazorChartSvg.N(y)} " +
               $"L {BlazorChartSvg.N(x + w - tr)} {BlazorChartSvg.N(y)} A {BlazorChartSvg.N(tr)} {BlazorChartSvg.N(tr)} 0 0 1 {BlazorChartSvg.N(x + w)} {BlazorChartSvg.N(y + tr)} " +
               $"L {BlazorChartSvg.N(x + w)} {BlazorChartSvg.N(y + h - br)} A {BlazorChartSvg.N(br)} {BlazorChartSvg.N(br)} 0 0 1 {BlazorChartSvg.N(x + w - br)} {BlazorChartSvg.N(y + h)} " +
               $"L {BlazorChartSvg.N(x + bl)} {BlazorChartSvg.N(y + h)} A {BlazorChartSvg.N(bl)} {BlazorChartSvg.N(bl)} 0 0 1 {BlazorChartSvg.N(x)} {BlazorChartSvg.N(y + h - bl)} " +
               $"L {BlazorChartSvg.N(x)} {BlazorChartSvg.N(y + tl)} A {BlazorChartSvg.N(tl)} {BlazorChartSvg.N(tl)} 0 0 1 {BlazorChartSvg.N(x + tl)} {BlazorChartSvg.N(y)} Z";
    }
}
