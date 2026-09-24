using AnimeUpscaler.Core;

namespace AnimeUpscaler.Core.Tests;

public sealed class ModelCatalogTests
{
    [Fact]
    public void Catalog_exposes_only_supported_scales()
    {
        Assert.Equal([4], ModelCatalog.AnimeV1.SupportedScales);
        Assert.Equal([2, 4], ModelCatalog.AnimeV2.SupportedScales);
        Assert.Equal([2, 3, 4], ModelCatalog.AnimeV3.SupportedScales);
    }

    [Fact]
    public void Resolve_rejects_unsupported_scale()
    {
        var error = Assert.Throws<ArgumentException>(() => ModelCatalog.AnimeV1.Resolve(2));
        Assert.Contains("tidak mendukung skala 2x", error.Message);
    }
}

