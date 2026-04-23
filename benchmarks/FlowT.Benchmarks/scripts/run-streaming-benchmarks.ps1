#Requires -PSEdition Core
<#
.SYNOPSIS
    Runs streaming benchmarks comparing buffered (List) vs streaming (PagedStreamResponse) performance.

.DESCRIPTION
    Executes BenchmarkDotNet for streaming response benchmarks.
    Compares memory efficiency and throughput for different dataset sizes (100, 1k, 10k items).

.PARAMETER Suite
    Which streaming suite to run: All, Streaming, Comparison. Defaults to All.

.PARAMETER Export
    If specified, exports results to markdown files in BenchmarkDotNet.Artifacts\results.

.PARAMETER Quick
    If specified, runs benchmarks in quick mode (fewer iterations, faster completion).

.EXAMPLE
    .\run-streaming-benchmarks.ps1
    Runs streaming benchmarks with default settings.

.EXAMPLE
    .\run-streaming-benchmarks.ps1 -Export
    Runs streaming benchmarks and exports results to markdown.

.EXAMPLE
    .\run-streaming-benchmarks.ps1 -Quick
    Runs streaming benchmarks in quick mode for fast feedback.
#>

param(
    [Parameter(HelpMessage = "Which streaming suite to run: All, Streaming, Comparison")]
    [ValidateSet("All", "Streaming", "Comparison")]
    [string]$Suite = "All",

    [Parameter(HelpMessage = "Export results to markdown files")]
    [switch]$Export,

    [Parameter(HelpMessage = "Run benchmarks in quick mode (fewer iterations)")]
    [switch]$Quick
)

$ErrorActionPreference = "Stop"
$PSScriptLocation = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $PSScriptLocation
$BenchmarkProject = Join-Path $ProjectRoot "FlowT.Benchmarks.csproj"
$timestamp = if ($env:FLOWTBENCH_TIMESTAMP) { $env:FLOWTBENCH_TIMESTAMP } else { Get-Date -Format "yyyy-MM-dd_HH-mm" }
$RunDir = Join-Path $ProjectRoot "BenchmarkDotNet.Artifacts\runs\$timestamp"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   FlowT Streaming Benchmarks" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verify project exists
if (-not (Test-Path $BenchmarkProject)) {
    Write-Host "ERROR: Benchmark project not found at: $BenchmarkProject" -ForegroundColor Red
    exit 1
}

# Create results directory if exporting
if ($Export -and -not (Test-Path $RunDir)) {
    Write-Host "Creating run directory: $RunDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $RunDir -Force | Out-Null
}

# Build arguments
$arguments = @(
    "run",
    "-c", "Release",
    "--project", $BenchmarkProject
)

# Both StreamingBenchmarks and StreamingComparisonBenchmarks match *Streaming*
$filter = switch ($Suite) {
    "Streaming"  { "*StreamingBenchmarks*" }
    "Comparison" { "*StreamingComparisonBenchmarks*" }
    default      { "*Streaming*" }
}
$arguments += "--filter", $filter

if ($Quick) {
    Write-Host "Running in QUICK mode (fewer iterations)..." -ForegroundColor Yellow
    $arguments += "--job", "short"
}

if ($Export) {
    $arguments += "--exporters", "markdown"
}

$arguments += "--artifacts", $RunDir

Write-Host "Starting streaming benchmarks..." -ForegroundColor Green
Write-Host "This will compare:" -ForegroundColor Green
Write-Host "  - Buffered (List<T>) vs Streaming (PagedStreamResponse<T>)" -ForegroundColor Cyan
Write-Host "  - StreamingComparison: Pure Sync vs Async Simulation overhead" -ForegroundColor Cyan
Write-Host "  - Dataset sizes: 100, 1,000, 10,000 items" -ForegroundColor Cyan
Write-Host "  - Metrics: Execution time + Memory allocations" -ForegroundColor Cyan
Write-Host ""

# Run benchmarks
$process = Start-Process -FilePath "dotnet" -ArgumentList $arguments -NoNewWindow -Wait -PassThru

if ($process.ExitCode -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Benchmark execution failed with exit code: $($process.ExitCode)" -ForegroundColor Red
    exit $process.ExitCode
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "   Benchmarks completed successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# Export results if requested
if ($Export) {
    Write-Host ""
    Write-Host "Results are available at:" -ForegroundColor Cyan
    Write-Host "  $RunDir\results\" -ForegroundColor White
    Write-Host ""
    Write-Host "Markdown files:" -ForegroundColor Cyan

    $resultsPath = Join-Path $RunDir "results"

    if (Test-Path $resultsPath) {
        $markdownFiles = Get-ChildItem -Path $resultsPath -Filter "*.md" |
            Where-Object { $_.Name -like "*StreamingBenchmarks*" }

        if ($markdownFiles.Count -gt 0) {
            foreach ($file in $markdownFiles) {
                Write-Host "  ✓ $($file.Name)" -ForegroundColor Green
            }
        }
        else {
            Write-Host "  ⚠ No markdown files found" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "  ⚠ Results directory not found" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "View detailed results in:" -ForegroundColor Cyan
Write-Host "  $RunDir\results\" -ForegroundColor White
Write-Host ""

# Display summary
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Expected Performance Characteristics" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Small datasets (100 items):" -ForegroundColor Yellow
Write-Host "  • Buffered: Slightly faster (less overhead)" -ForegroundColor White
Write-Host "  • Streaming: Comparable performance" -ForegroundColor White
Write-Host ""
Write-Host "Medium datasets (1,000 items):" -ForegroundColor Yellow
Write-Host "  • Buffered: ~20 KB allocations" -ForegroundColor White
Write-Host "  • Streaming: ~50% reduction in allocations" -ForegroundColor White
Write-Host ""
Write-Host "Large datasets (10,000 items):" -ForegroundColor Yellow
Write-Host "  • Buffered: ~200 KB allocations" -ForegroundColor White
Write-Host "  • Streaming: ~95% reduction in allocations (O(1) memory)" -ForegroundColor White
Write-Host ""
Write-Host "Key Insights:" -ForegroundColor Cyan
Write-Host "  ✓ Streaming shines with large datasets (>1000 items)" -ForegroundColor Green
Write-Host "  ✓ Memory usage stays constant regardless of dataset size" -ForegroundColor Green
Write-Host "  ✓ Enables progressive response delivery (TTFB improvement)" -ForegroundColor Green
Write-Host ""
