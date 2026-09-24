namespace AnimeUpscaler.Core;

public sealed record ModelVariant(
    string Key,
    string DisplayName,
    string ModelDirectory,
    string EngineModelName,
    IReadOnlyList<int> SupportedScales)
{
    public ModelFiles Resolve(int scale)
    {
        if (!SupportedScales.Contains(scale))
        {
            throw new ArgumentException($"{DisplayName} tidak mendukung skala {scale}x.", nameof(scale));
        }

        var stem = Key == "v1"
            ? "realesrgan-x4plus-anime"
            : $"realesr-animevideov3-x{scale}";

        return new ModelFiles($"{stem}.param", $"{stem}.bin");
    }

    public override string ToString() => DisplayName;
}

public sealed record ModelFiles(string ParameterFile, string WeightFile);

public static class ModelCatalog
{
    public static readonly ModelVariant AnimeV1 = new(
        "v1", "Real-ESRGAN Anime V1", "models-v1", "realesrgan-x4plus-anime", [4]);

    public static readonly ModelVariant AnimeV2 = new(
        "v2", "Real-ESRGAN Anime V2", "models-v2", "realesr-animevideov3", [2, 4]);

    public static readonly ModelVariant AnimeV3 = new(
        "v3", "Real-ESRGAN Anime V3", "models-v3", "realesr-animevideov3", [2, 3, 4]);

    public static IReadOnlyList<ModelVariant> All { get; } = [AnimeV1, AnimeV2, AnimeV3];
}

