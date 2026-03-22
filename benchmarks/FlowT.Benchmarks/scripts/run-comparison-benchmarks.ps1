# FlowT Comparison Benchmarks Runner
# Compares FlowT with competing frameworks: DispatchR, MediatR, Mediator.Net, WolverineFx, Brighter

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "DispatchR", "MediatR", "MediatorNet", "WolverineFx", "Brighter")]
    [string]$Framework = "All",

    [Parameter(Mandatory=$false)]
    [switch]$Quick,

    [Parameter(Mandatory=$false)]
    [switch]$Export
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  FlowT - Comparison Benchmarks        " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to benchmark project
$scriptPath = Split-Path -Parent $PSScriptRoot
Set-Location $scriptPath

# Check which frameworks are available
Write-Host "🔍 Checking available frameworks..." -ForegroundColor Yellow

$availableFrameworks = @()

# Check DispatchR
if (Select-String -Path "FlowT.Benchmarks.csproj" -Pattern 'PackageReference Include="DispatchR.Mediator"' -Quiet) {
    $availableFrameworks += "DispatchR"
    Write-Host "   ✅ DispatchR" -ForegroundColor Green
} else {
    Write-Host "   ❌ DispatchR (not installed)" -ForegroundColor Red
}

# Check MediatR
if (Select-String -Path "FlowT.Benchmarks.csproj" -Pattern 'PackageReference Include="MediatR"' -Quiet) {
    $availableFrameworks += "MediatR"
    Write-Host "   ✅ MediatR" -ForegroundColor Green
} else {
    Write-Host "   ❌ MediatR (not installed)" -ForegroundColor Red
}

# Check Mediator.Net
if (Select-String -Path "FlowT.Benchmarks.csproj" -Pattern "Mediator.Net" -Quiet) {
    $availableFrameworks += "Mediator.Net"
    Write-Host "   ✅ Mediator.Net" -ForegroundColor Green
} else {
    Write-Host "   ❌ Mediator.Net (not installed)" -ForegroundColor Red
}

# Check WolverineFx
if (Select-String -Path "FlowT.Benchmarks.csproj" -Pattern "WolverineFx" -Quiet) {
    $availableFrameworks += "WolverineFx"
    Write-Host "   ✅ WolverineFx" -ForegroundColor Green
} else {
    Write-Host "   ❌ WolverineFx (not installed)" -ForegroundColor Red
}

# Check Brighter
if (Select-String -Path "FlowT.Benchmarks.csproj" -Pattern "Paramore.Brighter" -Quiet) {
    $availableFrameworks += "Brighter"
    Write-Host "   ✅ Brighter" -ForegroundColor Green
} else {
    Write-Host "   ❌ Brighter (not installed)" -ForegroundColor Red
}

Write-Host ""

if ($availableFrameworks.Count -eq 0) {
    Write-Host "❌ No comparison frameworks found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 To add frameworks, run:" -ForegroundColor Yellow
    Write-Host "   dotnet add package DispatchR.Mediator" -ForegroundColor Gray
    Write-Host "   dotnet add package MediatR" -ForegroundColor Gray
    Write-Host "   dotnet add package Mediator.Net" -ForegroundColor Gray
    Write-Host "   dotnet add package WolverineFx" -ForegroundColor Gray
    Write-Host "   dotnet add package Paramore.Brighter" -ForegroundColor Gray
    Write-Host "   dotnet add package Paramore.Brighter.Extensions.DependencyInjection" -ForegroundColor Gray
    exit 1
}

# Build in Release mode
Write-Host "📦 Building benchmarks in Release mode..." -ForegroundColor Yellow
dotnet build -c Release --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""

# Determine filter
$filter = switch ($Framework) {
    "DispatchR" { 
        Write-Host "🎯 Running FlowT vs DispatchR comparison..." -ForegroundColor Yellow
        "*FlowTvsDispatchRBenchmarks*" 
    }
    "MediatR" { 
        Write-Host "🎯 Running FlowT vs MediatR comparison..." -ForegroundColor Yellow
        "*FlowTvsMediatRBenchmarks*" 
    }
    "MediatorNet" { 
        Write-Host "🎯 Running FlowT vs Mediator.Net comparison..." -ForegroundColor Yellow
        "*FlowTvsMediatorNetBenchmarks*" 
    }
    "WolverineFx" { 
        Write-Host "🎯 Running FlowT vs WolverineFx comparison..." -ForegroundColor Yellow
        "*FlowTvsWolverineFxBenchmarks*" 
    }
    "Brighter" { 
        Write-Host "🎯 Running FlowT vs Brighter comparison..." -ForegroundColor Yellow
        "*FlowTvsBrighterBenchmarks*" 
    }
    default { 
        Write-Host "🎯 Running All Comparison Benchmarks..." -ForegroundColor Yellow
        Write-Host "   Comparing FlowT with: $($availableFrameworks -join ', ')" -ForegroundColor Gray
        "*vs*" 
    }
}

Write-Host ""
Write-Host "Expected duration: " -ForegroundColor Cyan -NoNewline
if ($Quick) {
    Write-Host "~1-2 minutes per framework (Quick mode)" -ForegroundColor Yellow
} else {
    Write-Host "~3-5 minutes per framework (Full accuracy)" -ForegroundColor Yellow
}
Write-Host ""

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
    Write-Host "  ✅ Comparison completed!             " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    if ($Export) {
        $resultsDir = Join-Path $scriptPath "BenchmarkDotNet.Artifacts\results"
        Write-Host ""
        Write-Host "📁 Results saved to: $resultsDir" -ForegroundColor Cyan
    }

    Write-Host ""
    Write-Host "📊 Summary of available comparisons:" -ForegroundColor Cyan
    foreach ($fw in $availableFrameworks) {
        Write-Host "   ✅ FlowT vs $fw" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "❌ Benchmarks failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Usage Tips:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Compare with specific framework:" -ForegroundColor White
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Framework DispatchR" -ForegroundColor Gray
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Framework MediatR" -ForegroundColor Gray
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Framework MediatorNet" -ForegroundColor Gray
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Framework WolverineFx" -ForegroundColor Gray
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Framework Brighter" -ForegroundColor Gray
Write-Host ""
Write-Host "  Development mode (faster):" -ForegroundColor White
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Quick" -ForegroundColor Gray
Write-Host ""
Write-Host "  Export results:" -ForegroundColor White
Write-Host "    .\scripts\run-comparison-benchmarks.ps1 -Export" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 See also:" -ForegroundColor Cyan
Write-Host "   .\scripts\run-standard-benchmarks.ps1  - Standard FlowT benchmarks" -ForegroundColor Gray
Write-Host "   .\scripts\run-extreme-benchmarks.ps1   - Extreme load scenarios" -ForegroundColor Gray
