#Requires -PSEdition Core
# FlowT Standard Benchmarks Runner
# Runs core FlowT performance benchmarks (no competitor comparisons)

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Context", "Pipeline", "Allocations", "NamedKeys", "Cancellation", "PublishEvent", "Timer", "Specification")]
    [string]$Suite = "All",

    [Parameter(Mandatory=$false)]
    [switch]$Quick,

    [Parameter(Mandatory=$false)]
    [switch]$Export
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FlowT - Standard Benchmarks          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to benchmark project directory (parent of scripts folder)
$projectPath = Split-Path -Parent $PSScriptRoot
Set-Location $projectPath

# Build in Release mode
Write-Host "📦 Building benchmarks in Release mode..." -ForegroundColor Yellow
dotnet build -c Release --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Create timestamped output directory for this run
$timestamp = if ($env:FLOWTBENCH_TIMESTAMP) { $env:FLOWTBENCH_TIMESTAMP } else { Get-Date -Format "yyyy-MM-dd_HH-mm" }
$RunDir = Join-Path $projectPath "BenchmarkDotNet.Artifacts\runs\$timestamp"
New-Item -ItemType Directory -Path $RunDir -Force | Out-Null
Write-Host "📂 Run output: $RunDir" -ForegroundColor DarkGray
Write-Host ""

# Determine filter based on suite
$filter = switch ($Suite) {
    "Context" { 
        Write-Host "🎯 Running FlowContext Benchmarks..." -ForegroundColor Yellow
        "*FlowContextBenchmarks*" 
    }
    "Pipeline" { 
        Write-Host "🎯 Running Flow Pipeline Benchmarks..." -ForegroundColor Yellow
        "*FlowPipelineBenchmarks*" 
    }
    "Allocations" { 
        Write-Host "🎯 Running Allocation Benchmarks..." -ForegroundColor Yellow
        "*AllocationBenchmarks*" 
    }
    "NamedKeys" { 
        Write-Host "🎯 Running Named Keys Benchmarks..." -ForegroundColor Yellow
        "*NamedKeysComparisonBenchmarks*" 
    }
    "Cancellation" { 
        Write-Host "🎯 Running Cancellation Benchmarks..." -ForegroundColor Yellow
        "*CancellationBenchmarks*" 
    }
    "PublishEvent" { 
        Write-Host "🎯 Running Publish Event Benchmarks..." -ForegroundColor Yellow
        "*PublishEventBenchmarks*" 
    }
    "Timer" { 
        Write-Host "🎯 Running Timer Benchmarks..." -ForegroundColor Yellow
        "*TimerBenchmarks*" 
    }
    "Specification" { 
        Write-Host "🎯 Running FlowSpecification Benchmarks..." -ForegroundColor Yellow
        "*FlowSpecificationBenchmarks*" 
    }
    default {
        Write-Host "🎯 Running All Standard Benchmarks..." -ForegroundColor Yellow
        Write-Host "   - FlowContext operations" -ForegroundColor Gray
        Write-Host "   - Pipeline execution" -ForegroundColor Gray
        Write-Host "   - Memory allocations" -ForegroundColor Gray
        Write-Host "   - Named keys overhead" -ForegroundColor Gray
        Write-Host "   - Cancellation overhead" -ForegroundColor Gray
        Write-Host "   - Event publishing" -ForegroundColor Gray
        Write-Host "   - Timer operations" -ForegroundColor Gray
        Write-Host "   - FlowSpecification overhead" -ForegroundColor Gray
        "*FlowContextBenchmarks*|*FlowPipelineBenchmarks*|*AllocationBenchmarks*|*NamedKeysComparisonBenchmarks*"
    }
}

Write-Host ""
Write-Host "Expected duration: " -ForegroundColor Cyan -NoNewline
if ($Quick) {
    Write-Host "~2-3 minutes (Quick mode)" -ForegroundColor Yellow
} else {
    Write-Host "~5-8 minutes (Full accuracy)" -ForegroundColor Yellow
}
Write-Host ""

# For "All" suite, run each benchmark class separately (BenchmarkDotNet doesn't support OR operator in --filter)
if ($Suite -eq "All") {
    $suiteList = @(
        @{Name="FlowContext"; Filter="*FlowContextBenchmarks*"},
        @{Name="Pipeline"; Filter="*FlowPipelineBenchmarks*"},
        @{Name="Allocation"; Filter="*AllocationBenchmarks*"},
        @{Name="NamedKeys"; Filter="*NamedKeysComparisonBenchmarks*"},
        @{Name="Cancellation"; Filter="*CancellationBenchmarks*"},
        @{Name="PublishEvent"; Filter="*PublishEventBenchmarks*"},
        @{Name="Timer"; Filter="*TimerBenchmarks*"},
        @{Name="Specification"; Filter="*FlowSpecificationBenchmarks*"}
    )

    $allSucceeded = $true

    foreach ($suiteItem in $suiteList) {
        Write-Host "🎯 Running $($suiteItem.Name) benchmarks..." -ForegroundColor Cyan

        $benchmarkArgs = @(
            "run",
            "-c", "Release",
            "--no-build",
            "--",
            "--filter", $suiteItem.Filter
        )

        if ($Quick) {
            $benchmarkArgs += "--job", "short"
        }

        if ($Export) {
            $benchmarkArgs += "--exporters", "markdown"
        }

        $benchmarkArgs += "--artifacts", $RunDir

        Write-Host "Running: dotnet $($benchmarkArgs -join ' ')" -ForegroundColor DarkGray
        Write-Host ""

        & dotnet $benchmarkArgs

        if ($LASTEXITCODE -ne 0) {
            $allSucceeded = $false
            Write-Host "❌ $($suiteItem.Name) benchmarks failed!" -ForegroundColor Red
            break
        }

        Write-Host ""
    }

    if ($allSucceeded) {
        $LASTEXITCODE = 0
    } else {
        $LASTEXITCODE = 1
    }
} else {
    # Single suite - run normally
    $benchmarkArgs = @(
        "run",
        "-c", "Release",
        "--no-build",
        "--",
        "--filter", $filter
    )

    if ($Quick) {
        Write-Host "⚡ Quick mode enabled (fewer iterations, faster results)" -ForegroundColor Yellow
        $benchmarkArgs += "--job", "short"
    }

    if ($Export) {
        $benchmarkArgs += "--exporters", "markdown"
        Write-Host "📊 Results will be exported to: $RunDir\results\" -ForegroundColor Cyan
    }

    $benchmarkArgs += "--artifacts", $RunDir

    Write-Host ""
    Write-Host "Running: dotnet $($benchmarkArgs -join ' ')" -ForegroundColor DarkGray
    Write-Host ""

    # Run benchmarks
    & dotnet $benchmarkArgs
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Benchmarks completed!             " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    if ($Export) {
        Write-Host ""
        Write-Host "📁 Results saved to: $RunDir\results\" -ForegroundColor Cyan
    }
} else {
    Write-Host ""
    Write-Host "❌ Benchmarks failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Usage Tips:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Run specific suite:" -ForegroundColor White
Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite Context" -ForegroundColor Gray
Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite Pipeline" -ForegroundColor Gray
Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite NamedKeys" -ForegroundColor Gray
    Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite Cancellation" -ForegroundColor Gray
    Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite PublishEvent" -ForegroundColor Gray
    Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite Timer" -ForegroundColor Gray
    Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Suite Specification" -ForegroundColor Gray
Write-Host ""
Write-Host "  Development mode (faster):" -ForegroundColor White
Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Quick" -ForegroundColor Gray
Write-Host ""
Write-Host "  Export results:" -ForegroundColor White
Write-Host "    .\scripts\run-standard-benchmarks.ps1 -Export" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 See also:" -ForegroundColor Cyan
Write-Host "   .\scripts\run-comparison-benchmarks.ps1  - Compare with competitors" -ForegroundColor Gray
Write-Host "   .\scripts\run-extreme-benchmarks.ps1      - Test extreme load scenarios" -ForegroundColor Gray
