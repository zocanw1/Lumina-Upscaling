using AnimeUpscaler.Core;

namespace AnimeUpscaler.Core.Tests;

public sealed class ComparisonLayoutTests
{
    [Theory]
    [InlineData(1000, 0, 0)]
    [InlineData(1000, 25, 250)]
    [InlineData(1000, 100, 1000)]
    [InlineData(1000, 140, 1000)]
    public void Reveal_width_clamps_percent(double width, double percent, double expected)
    {
        Assert.Equal(expected, ComparisonLayout.RevealWidth(width, percent));
    }

    [Theory]
    [InlineData(0.5, 1)]
    [InlineData(1.5, 1.5)]
    [InlineData(8, 4)]
    public void Zoom_is_limited_to_useful_range(double requested, double expected)
    {
        Assert.Equal(expected, ComparisonLayout.ClampZoom(requested));
    }

    [Fact]
    public void Divider_is_centered_on_the_revealed_edge()
    {
        Assert.Equal(498.5, ComparisonLayout.DividerOffset(1000, 50, 3));
    }
}
