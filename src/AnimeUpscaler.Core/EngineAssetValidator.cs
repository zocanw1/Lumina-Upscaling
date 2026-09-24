namespace AnimeUpscaler.Core;

public static class EngineAssetValidator
{
    public static void Validate(string engineDirectory, ModelVariant model, int scale)
    {
        var executable = Path.Combine(engineDirectory, "realesrgan-ncnn-vulkan.exe");
        if (!File.Exists(executable))
        {
            throw new FileNotFoundException("Engine upscaler tidak ditemukan. Instalasi aplikasi tidak lengkap.", executable);
        }

        var files = model.Resolve(scale);
        var modelDirectory = Path.Combine(engineDirectory, model.ModelDirectory);
        foreach (var file in new[] { files.ParameterFile, files.WeightFile })
        {
            var path = Path.Combine(modelDirectory, file);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Model {model.DisplayName} {scale}x tidak ditemukan.", path);
            }
        }
    }
}

