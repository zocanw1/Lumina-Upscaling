using AnimeUpscaler.Core;

namespace AnimeUpscaler.Core.Tests;

public sealed class EngineCommandBuilderTests
{
    [Theory]
    [InlineData(OutputFormat.Png, "png")]
    [InlineData(OutputFormat.Jpeg, "jpg")]
    [InlineData(OutputFormat.WebP, "webp")]
    public void Output_format_maps_to_engine_extension(OutputFormat format, string extension)
    {
        Assert.Equal(extension, format.Extension());
    }

    [Fact]
    public void Builder_uses_argument_list_so_paths_are_not_shell_commands()
    {
        var request = new UpscaleRequest(
            "C:\\Images\\a & b.png",
            "C:\\Output\\result.png",
            ModelCatalog.AnimeV3,
            3,
            OutputFormat.Png);

        var command = EngineCommandBuilder.Build("C:\\App\\Engine", request);

        Assert.Equal("C:\\App\\Engine\\realesrgan-ncnn-vulkan.exe", command.FileName);
        Assert.Contains("C:\\Images\\a & b.png", command.ArgumentList);
        Assert.Contains("models-v3", command.ArgumentList);
        Assert.Contains("realesr-animevideov3", command.ArgumentList);
        Assert.DoesNotContain("cmd.exe", command.FileName, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("42,50%", 42.5)]
    [InlineData("processing 7.25%", 7.25)]
    [InlineData("done", null)]
    public void Parser_reads_engine_percent(string line, double? expected)
    {
        Assert.Equal(expected, EngineProgressParser.Parse(line));
    }
}
