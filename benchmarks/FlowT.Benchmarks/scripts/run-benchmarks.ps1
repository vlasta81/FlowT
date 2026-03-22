# FlowT Benchmarks - Main Launcher
# Interactive menu for running all benchmark types

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FlowT Benchmarks - Main Launcher     " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Select benchmark type:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [1] Standard Benchmarks" -ForegroundColor White
Write-Host "      - FlowContext operations" -ForegroundColor Gray
Write-Host "      - Pipeline execution" -ForegroundColor Gray
Write-Host "      - Allocations & throughput" -ForegroundColor Gray
Write-Host "      - Named Keys overhead" -ForegroundColor Gray
Write-Host ""
Write-Host "  [2] Comparison Benchmarks" -ForegroundColor White
Write-Host "      - FlowT vs DispatchR" -ForegroundColor Gray
Write-Host "      - FlowT vs MediatR" -ForegroundColor Gray
Write-Host "      - FlowT vs WolverineFx" -ForegroundColor Gray
Write-Host "      - FlowT vs Mediator.Net" -ForegroundColor Gray
Write-Host "      - FlowT vs Brighter" -ForegroundColor Gray
Write-Host ""
Write-Host "  [3] Extreme Benchmarks" -ForegroundColor White
Write-Host "      - Extreme Pipeline (10+10+10)" -ForegroundColor Gray
Write-Host "      - Large Payload (10 MB)" -ForegroundColor Gray
Write-Host "      - Concurrent Execution (100 parallel)" -ForegroundColor Gray
Write-Host "      - Deep Nesting (10 policies + 10 MB)" -ForegroundColor Gray
Write-Host ""
Write-Host "  [4] Streaming Benchmarks" -ForegroundColor White
Write-Host "      - Buffered vs Streaming responses" -ForegroundColor Gray
Write-Host "      - Memory efficiency (100, 1k, 10k items)" -ForegroundColor Gray
Write-Host "      - PagedStreamResponse performance" -ForegroundColor Gray
Write-Host ""
Write-Host "  [5] Plugin Benchmarks" -ForegroundColor White
Write-Host "      - Plugin<T>() cold vs. warm resolution" -ForegroundColor Gray
Write-Host "      - FlowPlugin Initialize overhead" -ForegroundColor Gray
Write-Host "      - Pipeline with shared plugin instance" -ForegroundColor Gray
Write-Host ""
Write-Host "  [6] All Benchmarks" -ForegroundColor White
Write-Host "      - Run complete suite (~25-40 minutes)" -ForegroundColor Gray
Write-Host ""
Write-Host "  [Q] Quit" -ForegroundColor Red
Write-Host ""

$choice = Read-Host "Enter choice (1-6, Q)"

switch ($choice.ToUpper()) {
    "1" {
        Write-Host ""
        Write-Host "🚀 Starting Standard Benchmarks..." -ForegroundColor Green
        Write-Host ""
        $scriptPath = Join-Path $PSScriptRoot "run-standard-benchmarks.ps1"
        pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Suite "All" -Export
    }
    "2" {
        Write-Host ""
        Write-Host "🚀 Starting Comparison Benchmarks..." -ForegroundColor Green
        Write-Host ""
        $scriptPath = Join-Path $PSScriptRoot "run-comparison-benchmarks.ps1"
        pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Framework "All" -Export
    }
    "3" {
        Write-Host ""
        Write-Host "🚀 Starting Extreme Benchmarks..." -ForegroundColor Green
        Write-Host ""
        $scriptPath = Join-Path $PSScriptRoot "run-extreme-benchmarks.ps1"
        pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Test "all" -Export
    }
    "4" {
        Write-Host ""
        Write-Host "🚀 Starting Streaming Benchmarks..." -ForegroundColor Green
        Write-Host ""
        $scriptPath = Join-Path $PSScriptRoot "run-streaming-benchmarks.ps1"
        pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Export
    }
    "5" {
        Write-Host ""
        Write-Host "🚀 Starting Plugin Benchmarks..." -ForegroundColor Green
        Write-Host ""
        $scriptPath = Join-Path $PSScriptRoot "run-plugin-benchmarks.ps1"
        pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Export
    }
    "6" {
        Write-Host ""
        Write-Host "🚀 Starting ALL Benchmarks..." -ForegroundColor Green
        Write-Host "⏱️  This will take approximately 25-40 minutes" -ForegroundColor Yellow
        Write-Host ""
        $confirm = Read-Host "Are you sure? (Y/N)"
        if ($confirm.ToUpper() -eq "Y") {
            Write-Host ""
            Write-Host "📊 Running Standard Benchmarks..." -ForegroundColor Cyan
            $scriptPath1 = Join-Path $PSScriptRoot "run-standard-benchmarks.ps1"
            pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath1 -Suite "All" -Export

            Write-Host ""
            Write-Host "📊 Running Comparison Benchmarks..." -ForegroundColor Cyan
            $scriptPath2 = Join-Path $PSScriptRoot "run-comparison-benchmarks.ps1"
            pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath2 -Framework "All" -Export

            Write-Host ""
            Write-Host "📊 Running Extreme Benchmarks..." -ForegroundColor Cyan
            $scriptPath3 = Join-Path $PSScriptRoot "run-extreme-benchmarks.ps1"
            pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath3 -Test "all" -Export

            Write-Host ""
            Write-Host "📊 Running Streaming Benchmarks..." -ForegroundColor Cyan
            $scriptPath4 = Join-Path $PSScriptRoot "run-streaming-benchmarks.ps1"
            pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath4 -Export

            Write-Host ""
            Write-Host "📊 Running Plugin Benchmarks..." -ForegroundColor Cyan
            $scriptPath5 = Join-Path $PSScriptRoot "run-plugin-benchmarks.ps1"
            pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath5 -Export

            Write-Host ""
            Write-Host "========================================" -ForegroundColor Green
            Write-Host "  ✅ All benchmarks completed!         " -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "📁 Results saved to: BenchmarkDotNet.Artifacts/results/" -ForegroundColor Cyan
        } else {
            Write-Host "Cancelled." -ForegroundColor Yellow
        }
    }
    "Q" {
        Write-Host "Exiting..." -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host ""
        Write-Host "❌ Invalid choice!" -ForegroundColor Red
        Write-Host ""
        Write-Host "💡 You can also run scripts directly:" -ForegroundColor Yellow
        Write-Host "   .\scripts\run-standard-benchmarks.ps1" -ForegroundColor Gray
        Write-Host "   .\scripts\run-comparison-benchmarks.ps1" -ForegroundColor Gray
        Write-Host "   .\scripts\run-extreme-benchmarks.ps1" -ForegroundColor Gray
        Write-Host "   .\scripts\run-streaming-benchmarks.ps1" -ForegroundColor Gray
        Write-Host "   .\scripts\run-plugin-benchmarks.ps1" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "📚 Documentation:" -ForegroundColor Cyan
Write-Host "   README.md                  - Overview and getting started" -ForegroundColor Gray
Write-Host "   docs/Quick-Start.md        - Running your first benchmark" -ForegroundColor Gray
Write-Host "   docs/Benchmark-Guide.md    - Methodology & best practices" -ForegroundColor Gray
Write-Host "   docs/results/              - Latest benchmark results" -ForegroundColor Gray
Write-Host ""
