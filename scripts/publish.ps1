$ErrorActionPreference = 'Stop'

$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts\AnimeUpscaler-win-x64'

dotnet test (Join-Path $root 'tests\AnimeUpscaler.Core.Tests\AnimeUpscaler.Core.Tests.csproj') -c Release --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet publish (Join-Path $root 'src\AnimeUpscaler\AnimeUpscaler.csproj') `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false `
    -o $output --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Copy-Item (Join-Path $root 'README.md') $output -Force
Copy-Item (Join-Path $root 'THIRD-PARTY-NOTICES.md') $output -Force

Write-Host "Published to $output"

