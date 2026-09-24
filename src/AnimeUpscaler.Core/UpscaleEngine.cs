using System.Diagnostics;
using System.Text;

namespace AnimeUpscaler.Core;

public sealed class UpscaleEngineException(string message, int exitCode, string details)
    : Exception(message)
{
    public int ExitCode { get; } = exitCode;
    public string Details { get; } = details;
}

public sealed class UpscaleEngine(string engineDirectory)
{
    public async Task RunAsync(
        UpscaleRequest request,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        EngineAssetValidator.Validate(engineDirectory, request.Model, request.Scale);

        if (!File.Exists(request.InputPath))
        {
            throw new FileNotFoundException("Gambar sumber tidak ditemukan.", request.InputPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(request.OutputPath)
            ?? throw new ArgumentException("Lokasi output tidak valid.", nameof(request)));

        var command = EngineCommandBuilder.Build(engineDirectory, request);
        using var process = new Process
        {
            StartInfo = CreateStartInfo(command),
            EnableRaisingEvents = true
        };

        var diagnostics = new StringBuilder();
        DataReceivedEventHandler onLine = (_, eventArgs) =>
        {
            if (string.IsNullOrWhiteSpace(eventArgs.Data))
            {
                return;
            }

            lock (diagnostics)
            {
                diagnostics.AppendLine(eventArgs.Data);
            }

            var percent = EngineProgressParser.Parse(eventArgs.Data);
            if (percent.HasValue)
            {
                progress?.Report(percent.Value);
            }
        };

        process.OutputDataReceived += onLine;
        process.ErrorDataReceived += onLine;

        try
        {
            if (!process.Start())
            {
                throw new UpscaleEngineException("Engine upscaler gagal dimulai.", -1, string.Empty);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (InvalidOperationException)
                {
                }
            });

            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (process.ExitCode != 0 || !File.Exists(request.OutputPath))
            {
                var details = diagnostics.ToString();
                throw new UpscaleEngineException(ToUserMessage(details), process.ExitCode, details);
            }

            progress?.Report(100);
        }
        finally
        {
            process.OutputDataReceived -= onLine;
            process.ErrorDataReceived -= onLine;
        }
    }

    private static ProcessStartInfo CreateStartInfo(EngineCommand command)
    {
        var startInfo = new ProcessStartInfo(command.FileName)
        {
            WorkingDirectory = command.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in command.ArgumentList)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static string ToUserMessage(string details)
    {
        if (details.Contains("vkCreate", StringComparison.OrdinalIgnoreCase)
            || details.Contains("vulkan", StringComparison.OrdinalIgnoreCase))
        {
            return "Vulkan tidak dapat dijalankan. Perbarui driver GPU AMD lalu coba lagi.";
        }

        if (details.Contains("out of memory", StringComparison.OrdinalIgnoreCase)
            || details.Contains("failed to allocate", StringComparison.OrdinalIgnoreCase))
        {
            return "Memori GPU tidak cukup. Tutup aplikasi berat atau gunakan gambar yang lebih kecil.";
        }

        return "Upscaling gagal. Pastikan gambar valid dan driver GPU sudah terbaru.";
    }
}
