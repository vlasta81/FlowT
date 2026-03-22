# FlowT Benchmark Results

This directory contains detailed benchmark results for FlowT performance testing.

---

## 📊 Core Performance Results

### FlowT Internal Benchmarks
- **[FlowContext Operations](FlowT.Benchmarks.FlowContextBenchmarks.md)** - Set/Get/Push/Pop/Named keys
- **[Pipeline Execution](FlowT.Benchmarks.FlowPipelineBenchmarks.md)** - Specs, Policies, Handlers
- **[Memory Allocations](FlowT.Benchmarks.AllocationBenchmarks.md)** - Zero-allocation fast paths
- **[Named Keys Overhead](FlowT.Benchmarks.NamedKeysComparisonBenchmarks.md)** - Named vs default keys
- **[Plugin System](FlowT.Benchmarks.PluginBenchmarks.md)** - Plugin resolution, PerFlow caching, FlowPlugin binding

**Plugin Key Findings:**
- ✅ **Initialize overhead: 1 ns / 8 B** — `FlowPlugin` abstraction costs nothing vs plain plugin
- ✅ **Warm path: 23 ns, 0 B** — locked dict lookup, zero allocations after first access
- ✅ **8.7× cold-to-warm speedup** — PerFlow cache eliminates DI resolution on repeat calls
- ✅ **Pipeline overhead: +110 ns / +168 B** — total cost of one cold + one warm access across policy + handler

### Streaming Performance
- **[Streaming Benchmarks](FlowT.Benchmarks.StreamingBenchmarks.md)** - Buffered vs streaming comparison (100-10k items)
- **[Streaming Comparison](FlowT.Benchmarks.StreamingComparisonBenchmarks.md)** - Real overhead analysis (25× vs 300× artifact)
- **[Streaming Guide](../Streaming-Benchmarks.md)** - Complete streaming documentation

**Key Findings:**
- ✅ **25× real overhead** for IAsyncEnumerable streaming
- ✅ **96% memory reduction** vs buffering
- ✅ **1000× faster TTFB** with real databases
- ⚠️ **300× Task.Yield artifact** (not representative of production)

---

## 🏆 Framework Comparison Results

### FlowT vs Competitors
- **[FlowT vs DispatchR](FlowT.Benchmarks.FlowTvsDispatchRBenchmarks.md)** - Closest competitor (FlowT 2.7× faster)
- **[FlowT vs MediatR](FlowT.Benchmarks.FlowTvsMediatRBenchmarks.md)** - Most popular (FlowT 9× faster)
- **[Complete Comparison](../DispatchR-Comparison.md)** - All frameworks ranked

**Performance Ranking:**
1. 🥇 **FlowT** - Fastest (30.8 ns baseline)
2. 🥈 **DispatchR** - 2.7× slower, 28% less memory
3. 🥉 **MediatR** - 9× slower
4. **WolverineFx** - 13.5× slower
5. **Mediator.Net** - 47× slower
6. **Brighter** - 77× slower

---

## 🔥 Extreme Load Results

### Stress Test Scenarios
- **[Extreme Pipeline](FlowT.Benchmarks.ExtremePipelineBenchmarks.md)** - 10 specs + 10 policies + 10k items
  - **Perfect linear scaling** - 10× components = 10× time
  - **Zero overhead** for large payloads (10 MB)
  - **Excellent concurrency** - 100 parallel requests

**Scenarios Tested:**
- **Large Pipelines** - 10 specs + 10 policies + 10 named keys (959 ns)
- **Large Payloads** - 10 MB + 10k items (4.6 ms, 208 B allocated)
- **High Concurrency** - 100 concurrent executions (15.5 ms, 68 KB)
- **Deep Nesting** - 10 policies + 10 MB data (4.6 ms, 304 B)

---

## 📈 Reading Benchmark Results

### Metrics Explained

**Time (ns/µs/ms):**
- Lower is better
- ns (nanoseconds) = 1/1,000,000,000 second
- µs (microseconds) = 1/1,000,000 second
- ms (milliseconds) = 1/1,000 second

**Allocated (B/KB/MB):**
- Lower is better
- Heap allocations per operation
- Indicates memory pressure and GC impact

**Ratio:**
- How many times slower/faster compared to baseline
- Example: "2.7×" means 2.7 times slower

### Benchmark Environment

**Hardware:**
- CPU: Intel Core i5-7600K @ 3.80GHz
- RAM: 16 GB DDR4
- OS: Windows 10/11

**Software:**
- .NET: 10.0.5
- BenchmarkDotNet: 0.15.8
- Configuration: Release mode, no debugger

---

## 🔄 Regenerating Results

### Run All Benchmarks
```powershell
cd benchmarks\FlowT.Benchmarks\scripts
.\run-benchmarks.ps1
```

### Run Specific Categories
```powershell
# Core FlowT benchmarks
.\run-standard-benchmarks.ps1

# FlowT vs competitors
.\run-comparison-benchmarks.ps1

# Stress tests
.\run-extreme-benchmarks.ps1
```

### Run Individual Benchmarks
```powershell
cd benchmarks\FlowT.Benchmarks
dotnet run -c Release --filter *StreamingBenchmarks*
dotnet run -c Release --filter *FlowTvsDispatchR*
```

---

## 📖 Related Documentation

- **[Main Benchmark Guide](../README.md)** - Overview and quick start
- **[Streaming Analysis](../Streaming-Benchmarks.md)** - Complete streaming guide
- **[DispatchR Comparison](../DispatchR-Comparison.md)** - Detailed competitor analysis
- **[Best Practices](../../../../docs/BEST_PRACTICES.md)** - Performance optimization tips

---

**Last Updated:** 2025-01-16 (Benchmark data from latest runs)
