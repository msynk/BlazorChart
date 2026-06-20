using System.Globalization;

namespace BlazorChart;

/// <summary>A computed tick on an axis.</summary>
public readonly record struct BlazorChartAxisTick(double Value, string Label, double Pixel, bool Minor = false);
