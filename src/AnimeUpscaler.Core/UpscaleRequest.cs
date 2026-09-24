namespace AnimeUpscaler.Core;

public enum OutputFormat
{
    Png,
    Jpeg,
    WebP
}

public static class OutputFormatExtensions
{
    public static string Extension(this OutputFormat format) => format switch
    {
        OutputFormat.Png => "png",
        OutputFormat.Jpeg => "jpg",
        OutputFormat.WebP => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}

public sealed record UpscaleRequest(
    string InputPath,
    string OutputPath,
    ModelVariant Model,
    int Scale,
    OutputFormat Format);

