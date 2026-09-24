param(
    [string]$PublishDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts\AnimeUpscaler-win-x64')
)

$ErrorActionPreference = 'Stop'
$engineDirectory = Join-Path $PublishDirectory 'Assets\Engine'
$engine = Join-Path $engineDirectory 'realesrgan-ncnn-vulkan.exe'
if (-not (Test-Path -LiteralPath $engine)) { throw "Engine not found: $engine" }

Add-Type -AssemblyName System.Drawing
$input = Join-Path $env:TEMP 'anime-upscaler-smoke-input.png'
$bitmap = [Drawing.Bitmap]::new(32, 32)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([Drawing.Color]::White)
$pen = [Drawing.Pen]::new([Drawing.Color]::Black, 3)
$graphics.DrawEllipse($pen, 4, 4, 24, 24)
$bitmap.Save($input, [Drawing.Imaging.ImageFormat]::Png)
$pen.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

$cases = @(
    @{ Name='V1'; Model='models-v1'; EngineName='realesrgan-x4plus-anime'; Scale=4 },
    @{ Name='V2'; Model='models-v2'; EngineName='realesr-animevideov3'; Scale=2 },
    @{ Name='V2'; Model='models-v2'; EngineName='realesr-animevideov3'; Scale=4 },
    @{ Name='V3'; Model='models-v3'; EngineName='realesr-animevideov3'; Scale=2 },
    @{ Name='V3'; Model='models-v3'; EngineName='realesr-animevideov3'; Scale=3 },
    @{ Name='V3'; Model='models-v3'; EngineName='realesr-animevideov3'; Scale=4 }
)

Push-Location $engineDirectory
try {
    foreach ($case in $cases) {
        $output = Join-Path $env:TEMP "anime-upscaler-smoke-$($case.Name)-x$($case.Scale).png"
        Remove-Item -LiteralPath $output -Force -ErrorAction SilentlyContinue
        & $engine -i $input -o $output -s $case.Scale -m $case.Model -n $case.EngineName -t 0 -j '1:2:2' -f png
        if ($LASTEXITCODE -ne 0) { throw "$($case.Name) x$($case.Scale) exited with $LASTEXITCODE" }

        $result = [Drawing.Bitmap]::FromFile($output)
        try {
            $expected = 32 * $case.Scale
            if ($result.Width -ne $expected -or $result.Height -ne $expected) {
                throw "$($case.Name) x$($case.Scale) returned $($result.Width)x$($result.Height), expected ${expected}x${expected}"
            }
        }
        finally {
            $result.Dispose()
            Remove-Item -LiteralPath $output -Force
        }
        Write-Host "PASS $($case.Name) x$($case.Scale)"
    }
}
finally {
    Pop-Location
    Remove-Item -LiteralPath $input -Force -ErrorAction SilentlyContinue
}
