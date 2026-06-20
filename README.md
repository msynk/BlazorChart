# BlazorChart

A native Blazor charting library rendered entirely with **SVG** — no JavaScript charting engine and no `<canvas>`. The API closely mirrors [Chart.js](https://www.chartjs.org/), so if you know Chart.js you already know BlazorChart.

JavaScript interop is used only for optional responsive resizing and zoom/pan gestures; everything else (geometry, scales, layout, animation) is computed in C# and rendered as plain SVG markup.

## Features

- **Chart types**: line, bar, radar, pie, doughnut, polar area, bubble, scatter — plus mixed charts (per-dataset type overrides)
- **Scales**: linear, logarithmic, category, time, and radial-linear; multiple/independent axes
- **Line/area**: bezier tension, monotone cubic interpolation, stepped lines, dashed borders, gap handling (`SpanGaps`), per-segment styling, area fills (to origin/value/dataset) with gradients
- **Bars**: vertical/horizontal, stacked, floating (range) bars, per-corner border radius, pattern fills
- **Styling**: solid colors, linear/radial gradients, repeating patterns, scriptable (per-element) options
- **Plugins**: title/subtitle, legend (clickable toggles), tooltips (custom templates, callbacks, positioners), annotations, doughnut center text
- **Interaction**: hover highlighting, click events, mouse-wheel zoom, pan, and drag-to-zoom (cartesian charts)
- **Animation**: CSS-driven entry animations and geometry transitions, with `prefers-reduced-motion` support
- **Accessibility**: `role="img"` with generated aria labels, keyboard navigation of data points, a visually-hidden data table for screen readers
- **Responsive**: container-aware sizing via a `ResizeObserver`

## Requirements

- .NET 10 (`net10.0`)
- A Blazor project (Server, WebAssembly, or the unified Blazor Web App model)

## Installation

Add a project reference to the `BlazorChart` library:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/src/BlazorChart/BlazorChart.csproj" />
</ItemGroup>
```

The component's CSS is delivered through Blazor's CSS isolation bundle, so make sure your host page references the scoped styles bundle (default Blazor templates already do):

```html
<link rel="stylesheet" href="@Assets["YourApp.styles.css"]" />
```

The JavaScript module for responsive sizing and zoom is loaded automatically from `_content/BlazorChart/chart-interop.js` — no manual `<script>` tag required.

## Usage

Import the namespaces, typically in `_Imports.razor`:

```razor
@using BlazorChart.Components
@using BlazorChart.Models
```

### Quick start

Use the convenience `Type` / `Data` / `Options` parameters:

```razor
<Chart Type="ChartType.Line" Data="_data" Options="_options" />

@code {
    private readonly ChartData _data = new()
    {
        Labels = new() { "Jan", "Feb", "Mar", "Apr", "May" },
        Datasets =
        {
            new ChartDataset
            {
                Label = "Visitors",
                Data = new() { 120, 190, 160, 250, 220 },
                BorderColor = "#36a2eb",
                Tension = 0.4,
                Fill = FillMode.Origin
            }
        }
    };

    private readonly ChartOptions _options = new()
    {
        Plugins = new PluginOptions
        {
            Legend = new LegendOptions { Position = Position.Bottom }
        }
    };
}
```

### Using a single Config object

You can also pass a complete `ChartConfig` (type + data + options), which takes precedence over the individual parameters:

```razor
<Chart Config="_config" />

@code {
    private readonly ChartConfig _config = new(
        ChartType.Bar,
        new ChartData
        {
            Labels = new() { "Q1", "Q2", "Q3", "Q4" },
            Datasets = { new ChartDataset { Label = "Revenue", Data = new() { 50, 75, 60, 90 } } }
        },
        new ChartOptions());
}
```

### Scatter / bubble data

Charts that need `(x, y)` (or `(x, y, r)`) points use `Points` instead of `Data`:

```razor
<Chart Type="ChartType.Scatter" Data="_scatter" />

@code {
    private readonly ChartData _scatter = new()
    {
        Datasets =
        {
            new ChartDataset
            {
                Label = "Samples",
                Points = new()
                {
                    new DataPoint(1, 4),
                    new DataPoint(2, 9),
                    new DataPoint(3, 6)
                }
            }
        }
    };
}
```

## Component parameters

| Parameter | Type | Description |
|---|---|---|
| `Config` | `ChartConfig?` | Full configuration; takes precedence over `Type`/`Data`/`Options`. |
| `Type` | `ChartType` | Chart type (default `Line`). |
| `Data` | `ChartData?` | Labels and datasets. |
| `Options` | `ChartOptions?` | Scales, plugins, animation, zoom, etc. |
| `Width` | `string` | CSS width of the container (default `100%`). |
| `Height` | `string?` | Optional CSS height; follows aspect ratio when null. |
| `Class` / `Style` | `string?` | Extra CSS on the root element. |
| `AriaLabel` | `string?` | Accessible label; a summary is generated when null. |
| `GenerateTable` | `bool` | Render a hidden data table for screen readers (default `true`). |
| `TooltipTemplate` | `RenderFragment<TooltipContext>?` | Custom tooltip body. |
| `OnElementClick` | `EventCallback<(int DatasetIndex, int DataIndex)>` | Raised when a data element is clicked. |

## Zoom & pan

Enable interaction through `ChartOptions.Zoom` (cartesian charts only):

```razor
<Chart Type="ChartType.Line" Data="_data" Options="_zoomOptions" />

@code {
    private readonly ChartOptions _zoomOptions = new()
    {
        Zoom = new ZoomOptions
        {
            Enabled = true,
            Wheel = true,
            Pan = true,
            Mode = ZoomMode.X
        }
    };
}
```

## Project structure

```
src/BlazorChart/        The charting library (Razor Class Library)
  Components/           The Chart component and SVG primitives
  Models/               Config, data, options, enums, scales, plugins
  Rendering/            Scene/geometry computation, scales, plugins
  wwwroot/              chart-interop.js (resize + zoom)
src/BlazorChart.Demo/  A Blazor Web App demonstrating every feature
```

## Running the demo

```bash
dotnet run --project src/BlazorChart.Demo
```

The demo includes pages for line, bar, area, pie, doughnut, polar, radar, scatter, mixed, multi-axis, time, scales, legends, tooltips, annotations, animations, scriptable options, zoom, and a live playground.

## Building

```bash
dotnet build src/BlazorChart.slnx
```
