using AnimeUpscaler.Core;

namespace AnimeUpscaler.Core.Tests;

public sealed class EngineAssetValidatorTests
{
    [Fact]
    public void Validate_reports_missing_executable_in_Indonesian()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var error = Assert.Throws<FileNotFoundException>(() =>
                EngineAssetValidator.Validate(directory, ModelCatalog.AnimeV3, 2));

            Assert.Contains("Engine upscaler tidak ditemukan", error.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
