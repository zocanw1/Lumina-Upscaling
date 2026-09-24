namespace AnimeUpscaler.Core;

public static class ComparisonLayout
{
    public static double RevealWidth(double viewportWidth, double sliderPercent)
    {
        var width = Math.Max(0, viewportWidth);
        var percent = Math.Clamp(sliderPercent, 0, 100);
        return width * percent / 100d;
    }

    public static double DividerOffset(double viewportWidth, double sliderPercent, double dividerWidth)
    {
        return Math.Max(0, RevealWidth(viewportWidth, sliderPercent) - Math.Max(0, dividerWidth) / 2d);
    }

    public static double ClampZoom(double zoom) => Math.Clamp(zoom, 1, 4);
}
