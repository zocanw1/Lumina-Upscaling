using AnimeUpscaler.Core;

namespace AnimeUpscaler.Core.Tests;

public sealed class ImageFilePolicyTests
{
    [Theory]
    [InlineData("art.PNG", true)]
    [InlineData("art.jpg", true)]
    [InlineData("art.jpeg", true)]
    [InlineData("art.webp", true)]
    [InlineData("art.gif", false)]
    public void Supported_input_extensions_are_explicit(string path, bool expected)
    {
        Assert.Equal(expected, ImageFilePolicy.IsSupportedInput(path));
    }

    [Fact]
    public void Temporary_output_uses_requested_extension()
    {
        var path = ImageFilePolicy.CreateTemporaryOutputPath(OutputFormat.WebP);

        Assert.EndsWith(".webp", path, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AnimeUpscaler", path);
    }
}
