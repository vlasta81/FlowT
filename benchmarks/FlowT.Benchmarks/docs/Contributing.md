# Contributing to FlowT Benchmarks

Thank you for your interest in improving FlowT benchmarks! This guide explains how to add custom benchmarks, submit results, and contribute improvements.

## Table of Contents

- [Getting Started](#getting-started)
- [Adding a New Benchmark](#adding-a-new-benchmark)
- [Benchmark Standards](#benchmark-standards)
- [Testing Your Benchmark](#testing-your-benchmark)
- [Submitting Results](#submitting-results)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Code Review Process](#code-review-process)

## Getting Started

### Prerequisites

Before contributing benchmarks:

1. ✅ **Read the Benchmark Guide** - [Benchmark-Guide.md](Benchmark-Guide.md)
2. ✅ **Run existing benchmarks** - [Quick-Start.md](Quick-Start.md)
3. ✅ **Understand BenchmarkDotNet** - [Documentation](https://benchmarkdotnet.org/)

### Environment Setup

```bash
# Clone the repository
git clone https://github.com/vlasta81/FlowT.git
cd FlowT

# Build the solution
dotnet build -c Release

# Verify benchmarks work
cd benchmarks/FlowT.Benchmarks
.\scripts\run-standard-benchmarks.ps1 -Quick
```

## Adding a New Benchmark

### 1. Choose a Benchmark Category

**Standard Benchmarks** (`benchmarks/FlowT.Benchmarks/`)
- Measures core FlowT features
- Example: `FlowContextBenchmarks.cs`, `FlowPipelineBenchmarks.cs`

**Comparison Benchmarks** (`benchmarks/FlowT.Benchmarks/`)
- Compares FlowT with competitors
- Example: `FlowTvsDispatchRBenchmarks.cs`, `FlowTvsMediatRBenchmarks.cs`

**Extreme Benchmarks** (`benchmarks/FlowT.Benchmarks/`)
- Tests under extreme load
- Example: `ExtremePipelineBenchmarks.cs`

### 2. Create Benchmark File

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FlowT;

namespace FlowT.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MarkdownExporter]
public class MyNewBenchmarks
{
    // Setup (runs once before all benchmarks)
    [GlobalSetup]
    public void Setup()
    {
        // Initialize resources
    }

    // Benchmark method
    [Benchmark(Baseline = true)]
    public async Task<string> FlowT_MyFeature()
    {
        // Test FlowT implementation
        return await result;
    }

    [Benchmark]
    public async Task<string> Competitor_MyFeature()
    {
        // Test competitor implementation
        return await result;
    }

    // Cleanup (runs once after all benchmarks)
    [GlobalCleanup]
    public void Cleanup()
    {
        // Dispose resources
    }
}
```

### 3. Follow Naming Conventions

**File names:**
- Standard: `{Feature}Benchmarks.cs` (e.g., `FlowContextBenchmarks.cs`)
- Comparison: `FlowTvs{Competitor}Benchmarks.cs` (e.g., `FlowTvsMediatRBenchmarks.cs`)
- Extreme: `Extreme{Scenario}Benchmarks.cs` (e.g., `ExtremePipelineBenchmarks.cs`)

**Method names:**
- `{Framework}_{Scenario}` (e.g., `FlowT_SimpleHandler`, `MediatR_SimpleHandler`)
- Use descriptive names (avoid `Test1`, `Method2`)

**Class names:**
- Plural form (e.g., `FlowContextBenchmarks`, not `FlowContextBenchmark`)

## Benchmark Standards

### Required Attributes

```csharp
[MemoryDiagnoser]  // REQUIRED - Track allocations
[MarkdownExporter] // REQUIRED - Export results
```

Optional attributes:

```csharp
[SimpleJob(warmupCount: 3)]           // Custom warmup
[MinColumn, MaxColumn, MedianColumn]  // Additional stats
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
```

### Baseline Benchmark

Always mark one benchmark as baseline:

```csharp
[Benchmark(Baseline = true)]
public void FlowT_Scenario()
{
    // This becomes the 1.00x reference
}

[Benchmark]
public void Competitor_Scenario()
{
    // Shown as ratio (e.g., 2.81x slower)
}
```

### Realistic Scenarios

✅ **DO:**
- Use realistic request/response objects
- Include typical validation/authorization
- Test common error scenarios
- Match production usage patterns

❌ **DON'T:**
- Test empty handlers
- Use trivial data (e.g., `return 42;`)
- Skip error handling
- Ignore async operations

### Example: Good vs Bad Benchmark

❌ **BAD** (Unrealistic):

```csharp
[Benchmark]
public int FlowT_Simple()
{
    var request = new Request();
    return 42; // Trivial logic
}
```

✅ **GOOD** (Realistic):

```csharp
[Benchmark]
public async Task<UserResponse> FlowT_CreateUser()
{
    var request = new CreateUserRequest
    {
        Email = "user@example.com",
        Name = "John Doe",
        Age = 30
    };

    // Validation
    if (string.IsNullOrEmpty(request.Email))
        throw new ValidationException("Email required");

    // Business logic
    return await _flow.ExecuteAsync(request);
}
```

## Testing Your Benchmark

### 1. Local Testing

```powershell
# Quick mode (fast feedback)
dotnet run -c Release -- --filter *MyNewBenchmarks* --job short

# Full mode (accurate results)
dotnet run -c Release -- --filter *MyNewBenchmarks*
```

### 2. Verify Results

Check that:
- ✅ All benchmarks complete without errors
- ✅ Mean values are reasonable (not 0 ns or extreme outliers)
- ✅ Memory allocations are tracked
- ✅ Ratios make sense (baseline is 1.00x)

### 3. Run Multiple Times

```powershell
# Run 3 times to check consistency
for ($i=1; $i -le 3; $i++) {
    dotnet run -c Release -- --filter *MyNewBenchmarks*
    Start-Sleep -Seconds 5
}
```

**Good results:**
```
Run 1: 30.8 ns ± 0.42 ns
Run 2: 31.1 ns ± 0.38 ns
Run 3: 30.5 ns ± 0.45 ns
```

**Bad results (investigate):**
```
Run 1: 30.8 ns ± 0.42 ns
Run 2: 45.2 ns ± 12.3 ns  ← High variation
Run 3: 28.1 ns ± 0.31 ns
```

## Submitting Results

### 1. Export Results

```powershell
# Run with export flag
.\scripts\run-benchmarks.ps1
# Select your benchmark suite

# Results saved to:
# benchmarks/FlowT.Benchmarks/docs/results/*.md
```

### 2. Include System Information

Add this to your PR description:

```markdown
## System Information

- **OS:** Windows 11 23H2 (22631.4037)
- **CPU:** AMD Ryzen 9 5900X (12 cores, 24 threads)
- **RAM:** 32 GB DDR4-3600
- **.NET:** 10.0.100
- **BenchmarkDotNet:** 0.15.8
```

### 3. Document Changes

```markdown
## Benchmark: FlowT vs NewFramework

**Why this comparison matters:**
NewFramework is gaining popularity for [specific reason].

**Key findings:**
- FlowT is 3.2× faster (30 ns vs 96 ns)
- FlowT uses 15% more memory (144 B vs 125 B)
- Trade-off: +19 bytes buys FlowContext features

**Recommendation:**
Use FlowT for most scenarios, consider NewFramework for [specific use case].
```

## Pull Request Guidelines

### Branch Naming

```bash
# Feature branches
git checkout -b benchmark/add-newframework-comparison
git checkout -b benchmark/improve-extreme-tests

# Documentation
git checkout -b docs/update-benchmark-guide
```

### Commit Messages

```bash
# ✅ Good
git commit -m "Add FlowT vs NewFramework comparison benchmark"
git commit -m "Improve extreme pipeline test with 20 policies"
git commit -m "Update Benchmark-Guide.md with memory analysis tips"

# ❌ Bad
git commit -m "Update"
git commit -m "Fix stuff"
git commit -m "WIP"
```

### PR Description Template

```markdown
## Summary
Brief description of what you're adding/changing.

## Motivation
Why is this benchmark/change needed?

## Changes
- Added `NewFrameworkBenchmarks.cs`
- Updated `run-comparison-benchmarks.ps1` to include NewFramework
- Added results to `docs/results/NewFramework-Comparison.md`

## Test Results

### System Info
- OS: Windows 11
- CPU: AMD Ryzen 9 5900X
- RAM: 32 GB
- .NET: 10.0.100

### Performance Summary
| Method | Mean | Allocated | Ratio |
|--------|------|-----------|-------|
| FlowT  | 30 ns | 144 B    | 1.00x |
| NewFw  | 96 ns | 125 B    | 3.20x |

## Checklist
- [ ] Followed naming conventions
- [ ] Added `[MemoryDiagnoser]` and `[MarkdownExporter]`
- [ ] Tested locally with `-Quick` and full mode
- [ ] Included realistic scenarios
- [ ] Updated documentation (if needed)
- [ ] Exported results to `docs/results/`
```

### PR Size Guidelines

✅ **Ideal PR:**
- 1-3 benchmark files
- 1-2 documentation updates
- < 500 lines changed

⚠️ **Large PR (split if possible):**
- 5+ benchmark files
- Major refactoring
- > 1000 lines changed

## Code Review Process

### 1. Automated Checks

Your PR will be checked for:
- ✅ Builds successfully in Release mode
- ✅ All benchmarks run without errors
- ✅ Markdown exports are valid
- ✅ File naming conventions followed

### 2. Manual Review

Reviewers will check:
- **Accuracy** - Are scenarios realistic?
- **Fairness** - Are all frameworks tested equally?
- **Documentation** - Is the purpose clear?
- **Results** - Do numbers make sense?

### 3. Feedback Incorporation

Expected response times:
- **Minor changes** (formatting, naming) - 1 day
- **Major changes** (rewrite benchmark) - 3-5 days
- **Results rerun** (different machine) - depends on availability

### 4. Merge Criteria

PR will be merged when:
- ✅ All automated checks pass
- ✅ At least 1 maintainer approval
- ✅ No unresolved comments
- ✅ Documentation is complete
- ✅ Results are reproducible

## Common Review Comments

### "Benchmark is too simple"

```csharp
// ❌ Rejected
[Benchmark]
public int Test() => 42;

// ✅ Accepted
[Benchmark]
public async Task<UserResponse> CreateUser()
{
    var request = new CreateUserRequest { /* realistic data */ };
    return await _flow.ExecuteAsync(request);
}
```

### "Add memory diagnostics"

```csharp
// ❌ Missing
public class MyBenchmarks { }

// ✅ Correct
[MemoryDiagnoser]
public class MyBenchmarks { }
```

### "Use more descriptive names"

```csharp
// ❌ Unclear
[Benchmark]
public void Test1() { }

// ✅ Clear
[Benchmark]
public void FlowT_CreateUser_WithValidation() { }
```

### "Results seem wrong"

If your results show:
- 0 ns execution time
- 0 B memory allocation
- Extreme outliers (e.g., 1 ns one run, 10 µs next)

**Action required:**
1. Verify Release mode: `dotnet run -c Release`
2. Close background apps
3. Run multiple times to check consistency
4. Include system information in PR

## Questions or Help?

- 💬 **Discussion** - Open a [GitHub Discussion](https://github.com/vlasta81/FlowT/discussions)
- 🐛 **Bug Report** - Open an [Issue](https://github.com/vlasta81/FlowT/issues)
- 📧 **Email** - Contact maintainer directly (see README)

---

**Thank you for contributing to FlowT!** 🎉
