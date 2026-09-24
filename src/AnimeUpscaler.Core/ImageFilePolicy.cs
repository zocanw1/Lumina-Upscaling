namespace AnimeUpscaler.Core;

public static class ImageFilePolicy
{
    private static readonly HashSet<string> SupportedInputExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

    public static bool IsSupportedInput(string path) =>
        !string.IsNullOrWhiteSpace(path) && SupportedInputExtensions.Contains(Path.GetExtension(path));

    public static string CreateTemporaryOutputPath(OutputFormat format)
    {
        var directory = Path.Combine(Path.GetTempPath(), "AnimeUpscaler");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"result-{Guid.NewGuid():N}.{format.Extension()}");
    }
}
