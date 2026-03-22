# FlowT Extreme Benchmarks Runner
# Tests FlowT under extreme load conditions

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("all", "extreme", "large", "concurrent", "nesting")]
    [string]$Test = "all",

    [Parameter(Mandatory=$false)]
    [switch]$Quick,

    [Parameter(Mandatory=$false)]
    [switch]$Export
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FlowT - Extreme Benchmarks           " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to benchmark project
$scriptPath = Split-Path -Parent $PSScriptRoot
Set-Location $scriptPath

# Build in Release mode
Write-Host "📦 Building benchmarks in Release mode..." -ForegroundColor Yellow
dotnet build -c Release --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Determine filter and description
$filter = ""
$description = ""
$expectedTime = ""

switch ($Test) {
    "extreme" {
        $filter = "*Extreme_10Specs_10Policies_10Keys*"
        $description = "10 specs + 10 policies + 10 keys"
        $expectedTime = if ($Quick) { "~10 seconds" } else { "~30 seconds" }
    }
    "large" {
        $filter = "*Extreme_LargePayload*"
        $description = "Large Payload (10 MB + 10k items)"
        $expectedTime = if ($Quick) { "~15 seconds" } else { "~45 seconds" }
    }
    "concurrent" {
        $filter = "*Extreme_Concurrent100*"
        $description = "100 concurrent executions"
        $expectedTime = if ($Quick) { "~20 seconds" } else { "~60 seconds" }
    }
    "nesting" {
        $filter = "*Extreme_DeepNesting*"
        $description = "Deep Nesting (10 policies + 10 MB payload)"
        $expectedTime = if ($Quick) { "~15 seconds" } else { "~45 seconds" }
    }
    default {
        $filter = "*ExtremePipelineBenchmarks*"
        $description = "All extreme tests"
        $expectedTime = if ($Quick) { "~1 minute" } else { "~3 minutes" }
    }
}

Write-Host "🎯 Running: $description" -ForegroundColor Yellow
Write-Host "Expected duration: $expectedTime" -ForegroundColor Cyan
Write-Host ""

if ($Test -eq "all") {
    Write-Host "Tests included:" -ForegroundColor White
    Write-Host "   1. 10 specs + 10 policies + 10 keys" -ForegroundColor Gray
    Write-Host "   2. Large payload (10 MB + 10k items)" -ForegroundColor Gray
    Write-Host "   3. 100 concurrent executions" -ForegroundColor Gray
    Write-Host "   4. Deep nesting (10 policies + large payload)" -ForegroundColor Gray
    Write-Host ""
}

# Build arguments
$benchmarkArgs = @(
    "run",
    "-c", "Release",
    "--no-build",
    "--",
    "--filter", $filter
)

if ($Quick) {
    Write-Host "⚡ Quick mode enabled (less iterations, faster results)" -ForegroundColor Yellow
    $benchmarkArgs += "--job", "short"
}

if ($Export) {
    $benchmarkArgs += "--exporters", "markdown", "html", "csv"
    Write-Host "📊 Results will be exported to: BenchmarkDotNet.Artifacts/results/" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Running: dotnet $($benchmarkArgs -join ' ')" -ForegroundColor DarkGray
Write-Host ""

# Run benchmarks
& dotnet $benchmarkArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Extreme benchmarks completed!     " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    if ($Export) {
        $resultsDir = Join-Path $scriptPath "BenchmarkDotNet.Artifacts\results"
        Write-Host ""
        Write-Host "📁 Results saved to: $resultsDir" -ForegroundColor Cyan
    }
} else {
    Write-Host ""
    Write-Host "❌ Benchmarks failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Usage Tips:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Run specific test:" -ForegroundColor White
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Test extreme    # 10+10+10 pipeline" -ForegroundColor Gray
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Test large      # 10 MB payload" -ForegroundColor Gray
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Test concurrent # 100 parallel" -ForegroundColor Gray
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Test nesting    # Deep nesting" -ForegroundColor Gray
Write-Host ""
Write-Host "  Development mode (faster):" -ForegroundColor White
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Quick" -ForegroundColor Gray
Write-Host ""
Write-Host "  Export results:" -ForegroundColor White
Write-Host "    .\scripts\run-extreme-benchmarks.ps1 -Export" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 See also:" -ForegroundColor Cyan
Write-Host "   .\scripts\run-standard-benchmarks.ps1    - Standard FlowT benchmarks" -ForegroundColor Gray
Write-Host "   .\scripts\run-comparison-benchmarks.ps1  - Compare with DispatchR, MediatR, etc." -ForegroundColor Gray
Write-Host ""
Write-Host "📚 Detailed guide: docs/Extreme-Benchmarks.md" -ForegroundColor Cyan
