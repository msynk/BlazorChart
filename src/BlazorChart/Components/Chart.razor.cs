using BlazorChart.Models;
using BlazorChart.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorChart.Components;

/// <summary>
/// A native Blazor chart component rendered entirely with SVG (no JavaScript or canvas).
/// Configure it either via a single <see cref="Config"/> object or the convenience
/// <see cref="Type"/>/<see cref="Data"/>/<see cref="Options"/> parameters, mirroring Chart.js.
/// </summary>
public partial class Chart : ComponentBase, IAsyncDisposable
{
    /// <summary>Full configuration (type + data + options). Takes precedence when set.</summary>
    [Parameter] public ChartConfig? Config { get; set; }

    [Parameter] public ChartType Type { get; set; } = ChartType.Line;
    [Parameter] public ChartData? Data { get; set; }
    [Parameter] public ChartOptions? Options { get; set; }

    /// <summary>CSS width of the chart container.</summary>
    [Parameter] public string Width { get; set; } = "100%";
    /// <summary>Optional CSS height. When null the height follows the aspect ratio.</summary>
    [Parameter] public string? Height { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    /// <summary>Accessible label for the chart. When null a summary is generated.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Render a visually-hidden data table for screen readers (default true).</summary>
    [Parameter] public bool GenerateTable { get; set; } = true;

    /// <summary>Optional custom tooltip template. When set it replaces the default tooltip body.</summary>
    [Parameter] public RenderFragment<TooltipContext>? TooltipTemplate { get; set; }

    /// <summary>Raised when a data element is clicked: (datasetIndex, dataIndex).</summary>
    [Parameter] public EventCallback<(int DatasetIndex, int DataIndex)> OnElementClick { get; set; }

    private readonly RenderState _state = new();
    private ChartConfig _config = new();
    private ChartScene _scene = new();

    // Virtual SVG coordinate space.
    private double _vw = 600;
    private double _vh = 300;

    // Measured container size (real device pixels) reported by the ResizeObserver.
    private double? _measuredWidth;
    private double? _measuredHeight;
    private bool _sizeRegistered;
    private IJSObjectReference? _sizeHandle;
    private bool _suppressTransition;

    // Interaction state (does not trigger a scene rebuild).
    private DataElement? _hovered;
    private readonly HashSet<DataElement> _active = new();
    private TooltipInfo? _activeTooltip;
    private TooltipContext? _tooltipContext;
    private readonly List<SvgNode> _hoverNodes = new();

    // Keyboard navigation.
    private int _focusIndex = -1;
    private string? _liveMessage;

    // Increments to (re)play entry animations: on data change and after the first size measurement.
    private int _animKey;
    private long _lastSig = long.MinValue;
    private bool _initialized;

    // Zoom/pan interop.
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ElementReference _plotEl;
    private IJSObjectReference? _module;
    private IJSObjectReference? _zoomHandle;
    private DotNetObjectReference<Chart>? _dotRef;
    private bool _zoomRegistered;

    // Drag-zoom selection box in viewBox coordinates (x, y, w, h).
    private (double X, double Y, double W, double H)? _dragBox;

    // Unique id for this instance's SVG defs (clip paths, gradients).
    private readonly string _instanceId = "bc" + Guid.NewGuid().ToString("N")[..8];

    protected override void OnParametersSet()
    {
        _config = Config ?? new ChartConfig(Type, Data ?? new ChartData(), Options ?? new ChartOptions());
        Recompute();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A resize-triggered render suppresses geometry transitions; re-enable for later updates.
        if (_suppressTransition) _suppressTransition = false;

        // Responsive sizing: observe the container so we can render at real device pixels.
        if (_config.Options.Responsive && !_sizeRegistered)
        {
            _sizeRegistered = true;
            try
            {
                _module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorChart/chart-interop.js");
                _dotRef ??= DotNetObjectReference.Create(this);
                _sizeHandle = await _module.InvokeAsync<IJSObjectReference>("observe", _plotEl, _dotRef);
            }
            catch
            {
                // Interop unavailable (e.g. during prerender) — stay at the fixed virtual size.
                _sizeRegistered = false;
            }
        }

        if (_zoomRegistered || !_config.Options.Zoom.Enabled || _scene.IsRadialOrCircular)
            return;
        _zoomRegistered = true;
        var z = _config.Options.Zoom;
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorChart/chart-interop.js");
            _dotRef ??= DotNetObjectReference.Create(this);
            _zoomHandle = await _module.InvokeAsync<IJSObjectReference>(
                "register", _plotEl, _dotRef, new { wheel = z.Wheel, pan = z.Pan && !z.DragZoom, drag = z.DragZoom });
        }
        catch
        {
            // Interop unavailable (e.g. during prerender) — zoom stays inert.
            _zoomRegistered = false;
        }
    }

    /// <summary>Invoked by the ResizeObserver when the container's pixel size changes.</summary>
    [JSInvokable]
    public void OnResize(double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        if (_measuredWidth is { } w && _measuredHeight is { } h
            && Math.Abs(w - width) < 1 && Math.Abs(h - height) < 1) return;

        _measuredWidth = width;
        _measuredHeight = height;
        _suppressTransition = true;   // resizing must not animate/transition element geometry
        Recompute();
        StateHasChanged();
    }

    [JSInvokable]
    public void OnWheelZoom(double fracX, double fracY, double deltaY)
    {
        var z = _config.Options.Zoom;
        double factor = deltaY < 0 ? 1 - z.Speed : 1 + z.Speed;
        foreach (var id in AxesForMode())
        {
            double t = AxisFraction(id, fracX, fracY);
            var (min, max) = CurrentRange(id);
            double cursor = min + t * (max - min);
            double nMin = cursor - (cursor - min) * factor;
            double nMax = cursor + (max - cursor) * factor;
            if (nMax - nMin > 1e-9) _state.AxisRanges[id] = (nMin, nMax);
        }
        _suppressTransition = true;
        Recompute();
        StateHasChanged();
    }

    [JSInvokable]
    public void OnDragMove(double x0, double y0, double x1, double y1)
    {
        _dragBox = (Math.Min(x0, x1) * _vw, Math.Min(y0, y1) * _vh,
                    Math.Abs(x1 - x0) * _vw, Math.Abs(y1 - y0) * _vh);
        StateHasChanged();
    }

    [JSInvokable]
    public void OnDragEnd(double x0, double y0, double x1, double y1)
    {
        _dragBox = null;
        // Ignore tiny drags (treated as a click).
        if (Math.Abs(x1 - x0) < 0.01 && Math.Abs(y1 - y0) < 0.01) { StateHasChanged(); return; }

        foreach (var id in AxesForMode())
        {
            double ta = AxisFraction(id, x0, y0);
            double tb = AxisFraction(id, x1, y1);
            double lo = Math.Min(ta, tb), hi = Math.Max(ta, tb);
            var (min, max) = CurrentRange(id);
            double span = max - min;
            double nMin = min + lo * span, nMax = min + hi * span;
            if (nMax - nMin > 1e-9) _state.AxisRanges[id] = (nMin, nMax);
        }
        _suppressTransition = true;
        Recompute();
        StateHasChanged();
    }

    /// <summary>Converts an element fraction (0..1) to a 0..1 position along an axis, via the plot area.</summary>
    private double AxisFraction(string id, double fracX, double fracY)
    {
        if (_scene.PlotArea is not { } p) return id == "x" ? fracX : 1 - fracY;
        if (id == "x")
        {
            double x = fracX * _vw;
            return p.Width <= 0 ? 0 : Math.Clamp((x - p.Left) / p.Width, 0, 1);
        }
        double y = fracY * _vh;
        return p.Height <= 0 ? 0 : Math.Clamp(1 - (y - p.Top) / p.Height, 0, 1);
    }

    [JSInvokable]
    public void OnPan(double dx, double dy)
    {
        foreach (var id in AxesForMode())
        {
            var (min, max) = CurrentRange(id);
            double span = max - min;
            double delta = id == "x" ? -dx * span : dy * span;
            _state.AxisRanges[id] = (min + delta, max + delta);
        }
        _suppressTransition = true;
        Recompute();
        StateHasChanged();
    }

    [JSInvokable]
    public void OnResetZoom()
    {
        _state.AxisRanges.Clear();
        _suppressTransition = true;
        Recompute();
        StateHasChanged();
    }

    private IEnumerable<string> AxesForMode()
    {
        var mode = _config.Options.Zoom.Mode;
        foreach (var id in _scene.ZoomableAxes)
        {
            bool isX = id == "x";
            if (mode == ZoomMode.X && !isX) continue;
            if (mode == ZoomMode.Y && isX) continue;
            yield return id;
        }
    }

    private (double Min, double Max) CurrentRange(string id)
    {
        if (_state.AxisRanges.TryGetValue(id, out var r)) return r;
        if (_scene.AxisRanges.TryGetValue(id, out var s)) return s;
        return (0, 1);
    }

    private void Recompute()
    {
        bool circular = _config.Type is ChartType.Pie or ChartType.Doughnut
            or ChartType.PolarArea or ChartType.Radar;
        double aspect = _config.Options.AspectRatio ?? (circular ? 1 : 2);
        if (aspect <= 0) aspect = 2;

        // Use the measured container width (real pixels → constant font sizes) when responsive,
        // otherwise fall back to a fixed 600-unit virtual space (e.g. during prerender).
        bool responsive = _config.Options.Responsive;
        double basis = responsive && _measuredWidth is { } mw && mw > 0 ? mw : 600;
        _vw = basis;
        if (_config.Options.MaintainAspectRatio)
            _vh = _vw / aspect;
        else
            _vh = responsive && _measuredHeight is { } mh && mh > 0 ? mh : basis / aspect;

        _scene = new ChartRenderer(_config, _state, _vw, _vh).Render();
        ClearHover();
        _focusIndex = -1;

        // Decide whether to (re)play the entry animation. We key off a signature of the data
        // values (not pixel positions), so data changes replay the animation while resize/zoom/pan
        // — which leave the values unchanged — do not.
        long sig = ComputeSignature();
        bool dataChanged = _initialized && sig != _lastSig;
        _lastSig = sig;

        // Entry animation plays on first mount automatically (CSS). On later data changes we bump the
        // key to recreate the group so the animation replays. Resize/zoom/pan keep values unchanged
        // (same signature) so they don't replay.
        if (AnimationEnabled && dataChanged && !_suppressTransition)
            _animKey++;

        _initialized = true;
    }

    /// <summary>A cheap signature of the data values driving the chart (changes when data changes).</summary>
    private long ComputeSignature()
    {
        unchecked
        {
            long h = 17;
            h = h * 31 + (int)_config.Type;
            foreach (var el in _scene.Elements)
            {
                h = h * 31 + el.DatasetIndex;
                h = h * 31 + el.DataIndex;
                h = h * 31 + BitConverter.DoubleToInt64Bits(Math.Round(el.Value, 6));
            }
            return h;
        }
    }

    // ---- hover / interaction (no scene rebuild) ----

    private void OnEnter(DataElement e)
    {
        _hovered = e;
        BuildHover(e);
    }

    private void OnLeave() => ClearHover();

    private void ClearHover()
    {
        _hovered = null;
        _active.Clear();
        _activeTooltip = null;
        _tooltipContext = null;
        _hoverNodes.Clear();
    }

    private void BuildHover(DataElement e)
    {
        _active.Clear();
        _hoverNodes.Clear();
        var tip = _config.Options.Plugins.Tooltip;

        IEnumerable<DataElement> group = tip.Mode switch
        {
            InteractionMode.Index or InteractionMode.X or InteractionMode.Y when !_scene.IsRadialOrCircular
                => _scene.Elements.Where(x => x.DataIndex == e.DataIndex),
            InteractionMode.Dataset
                => _scene.Elements.Where(x => x.DatasetIndex == e.DatasetIndex),
            _ => new[] { e }
        };

        foreach (var el in group) _active.Add(el);

        // Combined tooltip.
        var combined = new TooltipInfo
        {
            Title = e.Tooltip.Title,
            AnchorX = e.Tooltip.AnchorX,
            AnchorY = e.Tooltip.AnchorY
        };
        var ordered = _active.OrderBy(a => a.DatasetIndex).ToList();
        foreach (var el in ordered)
            combined.Items.AddRange(el.Tooltip.Items);

        // ---- Tooltip callbacks (title / body extras / footer / label color) ----
        var cb = tip.Callbacks;
        if (cb.Title is not null || cb.BeforeBody is not null || cb.AfterBody is not null
            || cb.Footer is not null || cb.LabelColor is not null)
        {
            var items = ordered.Select(a => new TooltipItemContext
            {
                DatasetIndex = a.DatasetIndex,
                DataIndex = a.DataIndex,
                DatasetLabel = a.SeriesLabel,
                Label = a.Tooltip.Title,
                Value = a.Value,
                Color = a.Tooltip.Items.FirstOrDefault()?.Color ?? "#000",
                FormattedValue = a.Tooltip.Items.FirstOrDefault()?.Text ?? ""
            }).ToList();

            if (cb.Title?.Invoke(items) is { } titleText) combined.Title = titleText;
            if (cb.BeforeBody?.Invoke(items) is { } bb) combined.BeforeBody.AddRange(bb.Split('\n'));
            if (cb.AfterBody?.Invoke(items) is { } ab) combined.AfterBody.AddRange(ab.Split('\n'));
            if (cb.Footer?.Invoke(items) is { } ft) combined.Footer.AddRange(ft.Split('\n'));
            if (cb.LabelColor is not null)
                for (int i = 0; i < combined.Items.Count && i < items.Count; i++)
                    if (cb.LabelColor(items[i]) is { } lc) combined.Items[i].Color = lc;
        }

        // Positioner.
        if (tip.Position == TooltipPositioner.Average && _active.Count > 0)
        {
            combined.AnchorX = _active.Average(a => a.CenterX);
            combined.AnchorY = _active.Min(a => a.Tooltip.AnchorY);
        }
        _activeTooltip = combined;

        // Context for a custom template.
        _tooltipContext = new TooltipContext
        {
            Title = combined.Title,
            Points = _active.OrderBy(a => a.DatasetIndex).Select(a => new TooltipPoint
            {
                DatasetIndex = a.DatasetIndex,
                DataIndex = a.DataIndex,
                Label = a.SeriesLabel,
                Value = a.Value,
                Color = a.Tooltip.Items.FirstOrDefault()?.Color ?? "#000",
                FormattedValue = a.Tooltip.Items.FirstOrDefault()?.Text ?? ""
            }).ToList()
        };

        // Highlight overlay.
        bool indexMode = tip.Mode is InteractionMode.Index or InteractionMode.X && !_scene.IsRadialOrCircular;
        if (indexMode && _scene.PlotArea is { } pa)
            _hoverNodes.Add(new SvgLine
            {
                X1 = e.CenterX, Y1 = pa.Top, X2 = e.CenterX, Y2 = pa.Bottom,
                Stroke = "rgba(0,0,0,0.35)", StrokeWidth = 1, Dash = "4,3"
            });

        foreach (var el in _active)
        {
            if (el.Shape is SvgCircle c)
                _hoverNodes.Add(new SvgCircle
                {
                    Cx = c.Cx, Cy = c.Cy, R = c.R + 4,
                    Fill = "none", Stroke = c.Fill, StrokeWidth = 2, Opacity = 0.6
                });
        }
    }

    private async Task OnClickElement(DataElement e)
    {
        if (OnElementClick.HasDelegate)
            await OnElementClick.InvokeAsync((e.DatasetIndex, e.DataIndex));
    }

    // ---- keyboard navigation ----

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        int n = _scene.Elements.Count;
        if (n == 0) return;
        switch (e.Key)
        {
            case "ArrowRight":
            case "ArrowDown":
                Move(1);
                break;
            case "ArrowLeft":
            case "ArrowUp":
                Move(-1);
                break;
            case "Home":
                SetFocus(0);
                break;
            case "End":
                SetFocus(n - 1);
                break;
            case "Enter":
            case " ":
                if (_focusIndex >= 0) await OnClickElement(_scene.Elements[_focusIndex]);
                break;
            case "Escape":
                _focusIndex = -1;
                ClearHover();
                _liveMessage = null;
                break;
        }
    }

    private void Move(int dir)
    {
        int n = _scene.Elements.Count;
        if (_focusIndex < 0) SetFocus(dir > 0 ? 0 : n - 1);
        else SetFocus((_focusIndex + dir + n) % n);
    }

    private void SetFocus(int i)
    {
        _focusIndex = i;
        var el = _scene.Elements[i];
        _hovered = el;
        BuildHover(el);
        _hoverNodes.Add(FocusOutline(el));
        _liveMessage = Describe(el);
    }

    private static SvgNode FocusOutline(DataElement el) => el.Shape switch
    {
        SvgRect r => new SvgRect { X = r.X - 2, Y = r.Y - 2, Width = r.Width + 4, Height = r.Height + 4, Fill = "none", Stroke = "#1a1a1a", StrokeWidth = 2, CssClass = "bc-focus-ring" },
        SvgCircle c => new SvgCircle { Cx = c.Cx, Cy = c.Cy, R = c.R + 5, Fill = "none", Stroke = "#1a1a1a", StrokeWidth = 2, CssClass = "bc-focus-ring" },
        _ => new SvgCircle { Cx = el.CenterX, Cy = el.CenterY, R = 8, Fill = "none", Stroke = "#1a1a1a", StrokeWidth = 2, CssClass = "bc-focus-ring" }
    };

    private string Describe(DataElement el)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(el.Tooltip.Title)) parts.Add(el.Tooltip.Title!);
        foreach (var item in el.Tooltip.Items) parts.Add(item.Text);
        return string.Join(", ", parts);
    }

    private void ToggleLegend(LegendItemModel item)
    {
        if (_scene.Legend is null || !_scene.Legend.OnClickToggle) return;
        if (item.IsDataIndex)
        {
            if (!_state.HiddenIndices.Add(item.Index)) _state.HiddenIndices.Remove(item.Index);
        }
        else
        {
            if (!_state.HiddenDatasets.Add(item.Index)) _state.HiddenDatasets.Remove(item.Index);
        }
        Recompute();
        StateHasChanged();
    }

    // ---- view helpers ----

    private string ViewBox => $"0 0 {Svg.N(_vw)} {Svg.N(_vh)}";

    private string? ClipId => _scene.PlotArea is null ? null : $"{_instanceId}-clip";
    private string? ClipRef => ClipId is null ? null : $"url(#{ClipId})";

    private bool AnimationEnabled => _config.Options.Animation.Animate;

    /// <summary>
    /// Entry animations run on first mount (pure CSS, no JS dependency) and replay on data change.
    /// </summary>
    private bool CanAnimate => AnimationEnabled;

    /// <summary>
    /// Global (unscoped) animation rules emitted once per chart. Kept out of the component's
    /// isolated stylesheet so the rules reliably match the SVG shapes rendered by the child
    /// <c>SvgPrimitive</c> component (which carries a different CSS-isolation scope).
    /// </summary>
    private const string AnimationStyles = """
        <style>
        @keyframes bc-rise { from { opacity: 0; transform: translateY(12px) scaleY(0.9); } to { opacity: 1; transform: none; } }
        @keyframes bc-grow { from { opacity: 0; transform: scale(0.82); } to { opacity: 1; transform: none; } }
        @keyframes bc-draw { from { stroke-dashoffset: 1; } to { stroke-dashoffset: 0; } }
        @keyframes bc-fade { from { opacity: 0; } to { opacity: 1; } }
        @keyframes bc-scale-y { from { transform: scaleY(0); } to { transform: scaleY(1); } }
        @keyframes bc-scale-x { from { transform: scaleX(0); } to { transform: scaleX(1); } }
        @keyframes bc-pop { from { opacity: 0; transform: scale(0); } to { opacity: 1; transform: scale(1); } }
        .bc-animate { animation-duration: var(--bc-dur, 600ms); animation-timing-function: var(--bc-ease, ease-out); animation-fill-mode: both; }
        .bc-anim-rise { animation-name: bc-rise; transform-box: view-box; transform-origin: center bottom; }
        .bc-anim-grow { animation-name: bc-grow; transform-box: view-box; transform-origin: center; }
        .bc-anim-bars-v { animation-name: bc-scale-y; transform-box: view-box; transform-origin: center bottom; }
        .bc-anim-bars-h { animation-name: bc-scale-x; transform-box: view-box; transform-origin: left center; }
        .bc-draw { animation: bc-draw var(--bc-dur, 600ms) var(--bc-ease, ease-out) both; stroke-dasharray: 1; }
        .bc-fade { animation: bc-fade var(--bc-dur, 600ms) var(--bc-ease, ease-out) both; }
        .bc-focus-ring { stroke-dasharray: 3 2; }
        .bc-el:hover :is(rect, path, polygon, circle) { filter: brightness(0.92); }
        .bc-active :is(rect, path, polygon) { filter: brightness(0.9); }
        .bc-transition :is(rect, circle, path, polygon) {
            transition: x var(--bc-dur,600ms) var(--bc-ease,ease-out), y var(--bc-dur,600ms) var(--bc-ease,ease-out),
                        width var(--bc-dur,600ms) var(--bc-ease,ease-out), height var(--bc-dur,600ms) var(--bc-ease,ease-out),
                        cx var(--bc-dur,600ms) var(--bc-ease,ease-out), cy var(--bc-dur,600ms) var(--bc-ease,ease-out),
                        r var(--bc-dur,600ms) var(--bc-ease,ease-out), d var(--bc-dur,600ms) var(--bc-ease,ease-out), fill .3s ease;
        }
        @media (prefers-reduced-motion: reduce) { .bc-animate, .bc-draw, .bc-fade { animation: none; } .bc-transition :is(rect, circle, path, polygon) { transition: none; } }
        </style>
        """;

    private string DataGroupClass
    {
        get
        {
            if (!CanAnimate) return "bc-data";
            if (_scene.IsRadialOrCircular)
                return "bc-data bc-animate bc-anim-grow";
            // Bars grow from the baseline as a size change, in the correct direction for the orientation.
            if (_scene.HasBars)
                return _scene.HorizontalBars ? "bc-data bc-animate bc-anim-bars-h" : "bc-data bc-animate bc-anim-bars-v";
            // Line/scatter points rise in.
            return "bc-data bc-animate bc-anim-rise";
        }
    }

    /// <summary>Class for the series (line/area) group — animates with the same proven group mechanism.</summary>
    private string SeriesGroupClass => CanAnimate ? "bc-series bc-animate bc-anim-rise" : "bc-series";

    private string ElementClass(DataElement el)
    {
        string c = "bc-el";
        if (IsActive(el)) c += " bc-active";
        return c;
    }

    private string ElementStyle(int index) => "cursor:pointer";

    private string AnimStyle =>
        $"--bc-dur:{_config.Options.Animation.Duration}ms;--bc-ease:{_config.Options.Animation.Easing}";

    private bool IsActive(DataElement e) => _active.Count > 0 && _active.Contains(e);

    private string RootStyle
    {
        get
        {
            var s = $"width:{Width};";
            if (!string.IsNullOrEmpty(Height)) s += $"height:{Height};";
            if (!string.IsNullOrEmpty(Style)) s += Style;
            return s;
        }
    }

    private double Pct(double v, double total) => total <= 0 ? 0 : v / total * 100;

    private string ChartAriaLabel
    {
        get
        {
            if (!string.IsNullOrEmpty(AriaLabel)) return AriaLabel;
            if (_config.Options.Plugins.Title is { Display: true, Text.Length: > 0 } t) return t.Text;
            int series = _config.Data.Datasets.Count;
            return $"{_config.Type} chart with {series} data series.";
        }
    }

    private bool HasPointData => _config.Data.Datasets.Any(d => d.Points is { Count: > 0 });

    private static string AlignToFlex(Align a) => a switch
    {
        Align.Start => "flex-start",
        Align.End => "flex-end",
        _ => "center"
    };

    private static string TextAlign(Align a) => a switch
    {
        Align.Start => "left",
        Align.End => "right",
        _ => "center"
    };

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_sizeHandle is not null) { await _sizeHandle.InvokeVoidAsync("dispose"); await _sizeHandle.DisposeAsync(); }
            if (_zoomHandle is not null) { await _zoomHandle.InvokeVoidAsync("dispose"); await _zoomHandle.DisposeAsync(); }
            if (_module is not null) await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        catch (Exception) { }
        _dotRef?.Dispose();
    }
}
