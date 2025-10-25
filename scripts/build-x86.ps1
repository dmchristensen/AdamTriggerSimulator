# Build single-file executable for Windows x86 (32-bit)
# Usage: .\scripts\build-x86.ps1

Write-Host "Building ADAM Trigger Simulator for Windows x86..." -ForegroundColor Cyan

$outputPath = "publish/win-x86"

dotnet publish src/AdamTriggerSimulator/AdamTriggerSimulator.csproj `
    -c Release `
    -r win-x86 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $outputPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Output: $outputPath\AdamTriggerSimulator.exe" -ForegroundColor Yellow

    $size = (Get-Item "$outputPath\AdamTriggerSimulator.exe").Length / 1MB
    Write-Host "Size: $($size.ToString('F2')) MB" -ForegroundColor Cyan
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit 1
}
