# FlowT Benchmark Guide

This guide explains how FlowT benchmarks work, how to interpret results, and best practices for running accurate measurements.

## Table of Contents

- [Understanding BenchmarkDotNet](#understanding-benchmarkdotnet)
- [Benchmark Categories](#benchmark-categories)
- [Key Metrics Explained](#key-metrics-explained)
- [Running Benchmarks](#running-benchmarks)
- [Interpreting Results](#interpreting-results)
- [Common Pitfalls](#common-pitfalls)
- [Best Practices](#best-practices)

## Understanding BenchmarkDotNet

FlowT uses [BenchmarkDotNet](https://benchmarkdotnet.org/) - the de facto standard for .NET performance testing. It provides:

- **Warmup iterations** - Eliminate JIT compilation and cache effects
- **Multiple runs** - Statistical analysis with mean, median, std dev
- **Memory diagnostics** - Track allocations via ETW profiling
- **Outlier detection** - Remove anomalous results
- **Export options** - Markdown, HTML, CSV, JSON

### Benchmark Attributes

```csharp
[MemoryDiagnoser]           // Track allocations
[SimpleJob(warmupCount: 3)] // 3 warmup iterations
[MarkdownExporter]          // Export results
public class MyBenchmark
{
    [Benchmark]
    public void MyTest() { ... }
}
```

## Benchmark Categories

### 1. Standard Benchmarks

**Purpose:** Measure core FlowT features in isolation

- **FlowContextBenchmarks** - Context creation and key operations
- **FlowPipelineBenchmarks** - Pipeline execution with various components
- **AllocationBenchmarks** - Memory efficiency across scenarios
- **NamedKeysBenchmarks** - Overhead of named vs default keys

**When to run:** After any change to FlowT core

**Expected duration:** ~3-5 minutes per suite

### 2. Comparison Benchmarks

**Purpose:** Compare FlowT with competing mediator frameworks

**Frameworks tested:**
- DispatchR (newest, closest competitor)
- MediatR (industry standard)
- Mediator.Net (enterprise features)
- WolverineFx (message-based architecture)
- Brighter (command processor pattern)

**Scenarios:**
- Simple handler (baseline)
- Handler + policies/behaviors (pipeline overhead)
- Handler + validation (pre-execution checks)

**When to run:** Before releases, when evaluating alternatives

**Expected duration:** ~3-5 minutes per framework

### 3. Extreme Benchmarks

**Purpose:** Verify FlowT scales linearly under extreme load

**Tests:**
- **10+10+10** - 10 specs + 10 policies + 10 keys
- **Large payload** - 10 MB data + 10k list items
- **100 concurrent** - Parallel execution safety
- **Deep nesting** - 10 policies + large payload

**When to run:** Before releases, when changing pipeline logic

**Expected duration:** ~3 minutes for all tests

### 4. Streaming Benchmarks

**Purpose:** Measure performance characteristics of streaming responses vs buffered responses

**Test suites:**
- **StreamingBenchmarks** - Compare buffered vs streaming across dataset sizes (100, 1k, 10k items)
- **StreamingComparisonBenchmarks** - Isolate real streaming overhead (removes Task.Yield simulation artifact)

**Key metrics:**
- Execution time overhead (~25× real, not 300×)
- Memory efficiency (96% reduction)
- Time to First Byte (TTFB) improvement (1000× with real DB)
- Scalability (constant memory regardless of dataset size)

**When to run:** When implementing/optimizing streaming endpoints, before releases

**Expected duration:** ~2-3 minutes per suite

**Documentation:** See [Streaming Benchmarks Guide](Streaming-Benchmarks.md) for detailed analysis and decision guide

### 5. Plugin System Benchmarks

**Purpose:** Measure the cost of plugin resolution, PerFlow caching, and the `FlowPlugin` abstract base overhead vs plain plugins

**Test suites:**
- **PluginBenchmarks** - 9 benchmarks covering cold/warm access, multi-type resolution, and full pipeline integration

**Tests:**
- **Cold access** - `Plugin<T>()` first call (DI resolution + cache population): 204 ns / 464 B
- **Warm access** - `Plugin<T>()` repeated call (locked dict lookup): 23 ns / 0 B
- **FlowPlugin vs plain plugin** - Initialize overhead: **1 ns / 8 B** (negligible)
- **3 plugin types cold** - All three resolved on first access: 344 ns / 520 B
- **3 plugin types warm** - All three from cache: 68.6 ns / 0 B
- **Pipeline no plugin (baseline)** - Plain pipeline cost: 154 ns / 392 B
- **Pipeline cold + warm** - Policy resolves cold, handler uses warm: 264 ns / 560 B (+110 ns / +168 B overhead)
- **Pipeline pre-warmed** - Both plugin accesses fully cached: 82.5 ns / 96 B

**Key metrics:**
- Cold-to-warm speedup: **8.7×** (204 ns → 23 ns)
- `FlowPlugin` vs plain plugin: **+1 ns / +8 B** (zero real-world impact)
- Zero allocations on all warm paths
- Pipeline overhead with PerFlow caching: **+110 ns / +168 B** (one cold + one warm per execution)

**When to run:** After any change to `Plugin<T>()`, `AddFlowPlugin`, or `FlowPlugin`

**Expected duration:** ~3-5 minutes

**Script:** `.\scripts\run-plugin-benchmarks.ps1`

**Results:** [📖 View Results](results/FlowT.Benchmarks.PluginBenchmarks.md)

## Key Metrics Explained

### Execution Time

```
Mean = 30.8 ns    ← Average across all iterations
Error = 0.42 ns   ← Standard error (99.9% confidence)
StdDev = 1.09 ns  ← Standard deviation
```

**What it means:**
- **Mean** - Primary metric for speed comparison
- **Error** - Confidence interval (lower is better)
- **StdDev** - Consistency (lower = more predictable)

**Rule of thumb:**
- `< 50 ns` - Excellent (cache-line level)
- `50-100 ns` - Very good (few CPU instructions)
- `100-500 ns` - Good (acceptable overhead)
- `> 1 μs` - Review (potential optimization needed)

### Memory Allocation

```
Allocated = 144 B  ← Total heap allocation
```

**What it means:**
- Bytes allocated on the **managed heap** (Gen0)
- Does **not** include stack allocations
- Lower = less GC pressure

**Rule of thumb:**
- `< 200 B` - Excellent (minimal GC impact)
- `200-500 B` - Good (acceptable for most scenarios)
- `500-1000 B` - Acceptable (watch for hot paths)
- `> 1 KB` - Review (may cause GC pauses)

### Ratio Comparison

```
FlowT      | 30.8 ns | 144 B | 1.00x | 1.00x
DispatchR  | 86.4 ns | 104 B | 2.81x | 0.72x
```

**What it means:**
- **Speed ratio** - FlowT is **2.81× faster** than DispatchR
- **Memory ratio** - FlowT uses **1.39× more memory** (144/104)

## Running Benchmarks

### Quick Mode (Development)

```powershell
# Fast iterations (1-2 minutes)
.\scripts\run-standard-benchmarks.ps1 -Quick
```

**Pros:** Fast feedback during development  
**Cons:** Less accurate (fewer iterations)

### Full Mode (Release)

```powershell
# Accurate measurements (3-5 minutes)
.\scripts\run-standard-benchmarks.ps1 -Export
```

**Pros:** Publication-ready results  
**Cons:** Slower (more iterations for statistical significance)

### Best Practices During Runs

✅ **DO:**
- Close unnecessary applications
- Disable antivirus/Windows Defender (temporarily)
- Run on AC power (not battery)
- Use Release build
- Wait for warmup completion

❌ **DON'T:**
- Run in Debug mode
- Keep browser/IDE open
- Use laptop on battery
- Run multiple benchmarks simultaneously
- Interrupt benchmark execution

## Interpreting Results

### Speed Comparison Example

```
| Method | Mean    | Ratio |
|--------|---------|-------|
| FlowT  | 30.8 ns | 1.00x |
| Other  | 277 ns  | 9.00x |
```

**Analysis:**
- FlowT is **9× faster** than Other
- Difference: `277 - 30.8 = 246 ns` per execution
- At 1M requests/sec: **246 ms saved** = 25% CPU reduction

### Memory Comparison Example

```
| Method | Allocated | Ratio |
|--------|-----------|-------|
| FlowT  | 144 B     | 1.00x |
| Other  | 120 B     | 0.83x |
```

**Analysis:**
- FlowT uses **20% more memory** (144 vs 120 B)
- Difference: `144 - 120 = 24 B` per request
- At 1M requests/sec: **24 MB/sec** extra allocation
- **Trade-off:** +24 bytes buys FlowContext features (services, storage, events)

### When Speed Matters More

- **Hot paths** (called millions of times)
- **Latency-sensitive** (real-time, low-latency)
- **CPU-bound** (high request throughput)

➡️ **Choose fastest option** (usually FlowT)

### When Memory Matters More

- **Memory-constrained** (IoT, embedded)
- **Long-lived processes** (background workers)
- **Large-scale** (thousands of parallel handlers)

➡️ **Evaluate trade-offs** (features vs footprint)

## Common Pitfalls

### ❌ Pitfall #1: Debug Mode

**Problem:** Debug builds are 5-10× slower  
**Solution:** Always use `-c Release`

```powershell
# ❌ Wrong
dotnet run -- --filter *MyBenchmark*

# ✅ Correct
dotnet run -c Release -- --filter *MyBenchmark*
```

### ❌ Pitfall #2: Background Noise

**Problem:** Other apps skew results  
**Solution:** Close unnecessary processes

**Typical culprits:**
- Chrome/Edge (hundreds of tabs)
- Visual Studio (IntelliSense indexing)
- Windows Defender (real-time scanning)
- OneDrive/Dropbox (file sync)

### ❌ Pitfall #3: Insufficient Warmup

**Problem:** First runs include JIT compilation  
**Solution:** BenchmarkDotNet handles this (don't skip warmup)

### ❌ Pitfall #4: Comparing Different Machines

**Problem:** CPU/RAM differences invalidate comparisons  
**Solution:** Use **ratios** (1.00x, 2.81x), not absolute numbers

### ❌ Pitfall #5: Chasing Noise

**Problem:** Optimizing 1-2 ns differences (within error margin)  
**Solution:** Focus on differences > 10% or > 5 ns

## Best Practices

### ✅ 1. Establish Baselines

Before optimizing, run benchmarks and save results:

```powershell
.\scripts\run-standard-benchmarks.ps1 -Export
git add docs/results/*.md
git commit -m "Baseline: v1.0.0 benchmarks"
```

### ✅ 2. Measure Before/After

```powershell
# Before optimization
.\scripts\run-standard-benchmarks.ps1 -Export
mv docs/results/FlowT.*.md docs/results/before/

# Apply changes
git commit -m "Optimize: FlowContext creation"

# After optimization
.\scripts\run-standard-benchmarks.ps1 -Export
mv docs/results/FlowT.*.md docs/results/after/

# Compare
diff docs/results/before/ docs/results/after/
```

### ✅ 3. Use Quick Mode for Iteration

```powershell
# During development (fast feedback)
.\scripts\run-standard-benchmarks.ps1 -Quick

# Before committing (accurate)
.\scripts\run-standard-benchmarks.ps1 -Export
```

### ✅ 4. Test on Target Hardware

If deploying to:
- **Cloud VMs** - Benchmark on similar instance type
- **Containers** - Test with resource limits
- **On-premises** - Match production CPU/RAM

### ✅ 5. Track Trends Over Time

```
v1.0.0: FlowContext creation = 25.3 ns
v1.1.0: FlowContext creation = 23.8 ns (✅ 6% faster)
v1.2.0: FlowContext creation = 30.1 ns (⚠️ 26% slower)
```

**Action:** Investigate v1.2.0 regression

### ✅ 6. Combine Speed + Memory

Don't optimize one at the expense of the other:

```
Scenario A: 30 ns, 144 B  ← Balanced
Scenario B: 20 ns, 500 B  ← Fast but wasteful
Scenario C: 50 ns, 80 B   ← Efficient but slow
```

**Best choice depends on your workload:**
- High throughput → A or B
- Memory-constrained → C

### ✅ 7. Document Trade-offs

When making architectural choices:

```markdown
## Decision: FlowContext vs Minimal Context

**FlowContext** (144 B):
- ✅ Services, storage, HTTP helpers
- ✅ Event notification
- ❌ +40 bytes vs minimal

**Minimal** (104 B):
- ✅ Smallest footprint
- ❌ No infrastructure features

**Verdict:** FlowContext (features worth +40 B)
```

## Next Steps

- **Run your first benchmark** → See [Quick-Start.md](Quick-Start.md)
- **Compare with competitors** → See [DispatchR-Comparison.md](DispatchR-Comparison.md)
- **Test extreme loads** → See [Extreme-Benchmarks.md](Extreme-Benchmarks.md)
- **Add custom benchmarks** → See [Contributing.md](Contributing.md)

---

**Questions or issues?** Open an issue on [GitHub](https://github.com/vlasta81/FlowT)
