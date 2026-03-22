<#
.SYNOPSIS
    Runs plugin system benchmarks for FlowT.

.DESCRIPTION
    Executes BenchmarkDotNet for PluginBenchmarks.
    Measures Plugin<T>() cold and warm resolution cost, FlowPlugin Initialize overhead,
    multi-type resolution, and full pipeline execution with shared plugin instances.

.PARAMETER Export
    If specified, exports results to markdown files in BenchmarkDotNet.Artifacts\results.

.PARAMETER Quick
    If specified, runs benchmarks in quick mode (fewer iterations, faster completion).

.EXAMPLE
    .\run-plugin-benchmarks.ps1
    Runs plugin benchmarks with default settings.

.EXAMPLE
    .\run-plugin-benchmarks.ps1 -Export
    Runs plugin benchmarks and exports results to markdown.

.EXAMPLE
    .\run-plugin-benchmarks.ps1 -Quick
    Runs plugin benchmarks in quick mode for fast feedback.
#>

param(
    [Parameter(HelpMessage = "Export results to markdown files")]
    [switch]$Export,

    [Parameter(HelpMessage = "Run benchmarks in quick mode (fewer iterations)")]
    [switch]$Quick
)

$ErrorActionPreference = "Stop"
$PSScriptLocation = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $PSScriptLocation
$BenchmarkProject = Join-Path $ProjectRoot "FlowT.Benchmarks.csproj"
$ResultsDir = Join-Path $ProjectRoot "BenchmarkDotNet.Artifacts\results"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   FlowT Plugin System Benchmarks              " -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $BenchmarkProject)) {
    Write-Host "ERROR: Benchmark project not found at: $BenchmarkProject" -ForegroundColor Red
    exit 1
}

if ($Export -and -not (Test-Path $ResultsDir)) {
    Write-Host "Creating results directory: $ResultsDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
}

Write-Host "📦 Building benchmarks in Release mode..." -ForegroundColor Yellow
dotnet build $BenchmarkProject -c Release --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "This will measure:" -ForegroundColor White
Write-Host "  ├─ Plugin<T>() cold path   — DI resolution + Initialize + dict write" -ForegroundColor Gray
Write-Host "  ├─ Plugin<T>() warm path   — locked dict lookup (cache hit)"           -ForegroundColor Gray
Write-Host "  ├─ FlowPlugin vs plain     — isolates Initialize() binding overhead"    -ForegroundColor Gray
Write-Host "  ├─ 3 plugin types          — cold vs. warm for multi-plugin scenarios"  -ForegroundColor Gray
Write-Host "  ├─ Pipeline without plugin — baseline flow execution"                   -ForegroundColor Gray
Write-Host "  ├─ Pipeline with plugin    — policy (cold) + handler (warm)"            -ForegroundColor Gray
Write-Host "  └─ Pipeline pre-warmed     — both stages hit cache"                     -ForegroundColor Gray
Write-Host ""

if ($Quick) {
    Write-Host "⚡ Quick mode: ~1-2 minutes" -ForegroundColor Yellow
} else {
    Write-Host "⏱️  Expected duration: ~3-5 minutes" -ForegroundColor Cyan
}
Write-Host ""

$arguments = @(
    "run",
    "-c", "Release",
    "--no-build",
    "--project", $BenchmarkProject,
    "--",
    "--filter", "*PluginBenchmarks*"
)

if ($Quick) {
    $arguments += "--job", "short"
}

if ($Export) {
    $arguments += "--exporters", "markdown"
}

Write-Host "▶ Running: dotnet $($arguments -join ' ')" -ForegroundColor DarkGray
Write-Host ""

& dotnet $arguments

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Benchmark execution failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "   ✅ Plugin benchmarks completed!             " -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

if ($Export) {
    Write-Host ""
    Write-Host "📁 Results saved to:" -ForegroundColor Cyan
    Write-Host "   $ResultsDir" -ForegroundColor White
    Write-Host ""

    $mdFiles = Get-ChildItem -Path $ResultsDir -Filter "*PluginBenchmarks*" -ErrorAction SilentlyContinue
    if ($mdFiles) {
        Write-Host "📄 Generated files:" -ForegroundColor Cyan
        foreach ($f in $mdFiles) {
            Write-Host "   $($f.Name)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
