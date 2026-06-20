namespace BlazorChart.Rendering;

public sealed class BlazorChartSvgText : BlazorChartSvgNode
{
    public double X, Y;
    public string Text = "";
    public string Fill = "#000";
    public string FontFamily = "sans-serif";
    public double FontSize = 12;
    public string FontWeight = "normal";
    public string FontStyle = "normal";
    /// <summary>start | middle | end</summary>
    public string Anchor = "start";
    /// <summary>auto | middle | hanging | central</summary>
    public string Baseline = "auto";
    public double Rotation;
}
