using System.Globalization;
using System.Text.RegularExpressions;

namespace AnimeUpscaler.Core;

public sealed record EngineCommand(
    string FileName,
    string WorkingDirectory,
    IReadOnlyList<string> ArgumentList);

public static class EngineCommandBuilder
{
    public static EngineCommand Build(string engineDirectory, UpscaleRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(engineDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.InputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputPath);
        request.Model.Resolve(request.Scale);

        var arguments = new List<string>
        {
            "-i", request.InputPath,
            "-o", request.OutputPath,
            "-s", request.Scale.ToString(CultureInfo.InvariantCulture),
            "-m", request.Model.ModelDirectory,
            "-n", request.Model.EngineModelName,
            "-t", "0",
            "-j", "1:2:2",
            "-f", request.Format.Extension(),
            "-v"
        };

        return new EngineCommand(
            Path.Combine(engineDirectory, "realesrgan-ncnn-vulkan.exe"),
            engineDirectory,
            arguments);
    }
}

public static partial class EngineProgressParser
{
    [GeneratedRegex(@"(?<percent>\d{1,3}(?:[\.,]\d+)?)%", RegexOptions.CultureInvariant)]
    private static partial Regex PercentRegex();

    public static double? Parse(string line)
    {
        var match = PercentRegex().Match(line);
        if (!match.Success)
        {
            return null;
        }

        var normalized = match.Groups["percent"].Value.Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, 0, 100)
            : null;
    }
}

