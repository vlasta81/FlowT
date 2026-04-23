# FlowT Benchmark Results

This directory contains detailed benchmark results for FlowT performance testing.

---

## 📊 Core Performance Results

### FlowT Internal Benchmarks
- **[FlowContext Operations](FlowT.Benchmarks.FlowContextBenchmarks-report-github.md)** - Set/Get/Push/Pop/Named keys, Service\<T\>
- **[Pipeline Execution](FlowT.Benchmarks.FlowPipelineBenchmarks-report-github.md)** - Specs, Policies, Handlers
- **[Memory Allocations](FlowT.Benchmarks.AllocationBenchmarks-report-github.md)** - Memory efficiency & throughput
- **[Named Keys Overhead](FlowT.Benchmarks.NamedKeysComparisonBenchmarks-report-github.md)** - Named vs default keys
- **[Plugin System](FlowT.Benchmarks.PluginBenchmarks-report-github.md)** - Plugin resolution, PerFlow caching, FlowPlugin binding
- **[Cancellation](FlowT.Benchmarks.CancellationBenchmarks-report-github.md)** - CancellationToken overhead ✨
- **[Publish Events](FlowT.Benchmarks.PublishEventBenchmarks-report-github.md)** - PublishAsync vs PublishInBackground ✨
- **[Timers](FlowT.Benchmarks.TimerBenchmarks-report-github.md)** - StartTimer cold/warm paths ✨

**Key Findings (23.04.2026, .NET 10.0.6):**
- ✅ **Simple handler: 28 ns / 144 B** — pipeline baseline
- ✅ **CancellationToken check: 0.29 ns** — essentially free
- ✅ **PublishAsync (1 handler): 45 ns / 32 B** — inline event delivery
- ✅ **PublishInBackground: ~867 ns / 464 B** — constant Task scheduling cost, handler count irrelevant
- ✅ **StartTimer warm: 46 ns / 0 B** — lazy-init dict, zero alloc after first timer
- ✅ **Plugin warm: 7.6 ns / 0 B** — per-flow cache, fastest repeat access
- ✅ **Plugin cold: 191 ns / 472 B** — first access including lazy dict allocation
- ✅ **Pipeline overhead: +98 ns / +248 B** — one cold + one warm plugin across policy + handler

### Streaming Performance
- **[Streaming Benchmarks](FlowT.Benchmarks.StreamingBenchmarks-report-github.md)** - Buffered vs streaming (100–10k items)
- **[Streaming Comparison](FlowT.Benchmarks.StreamingComparisonBenchmarks-report-github.md)** - Real overhead isolation (Sync vs Async)
- **[Streaming Guide](../Streaming-Benchmarks.md)** - Complete streaming documentation

**Key Findings (23.04.2026, .NET 10.0.6):**
- ✅ **28.5× real overhead** (Sync streaming, no Task.Yield) — pure IAsyncEnumerable cost
- ✅ **96% memory reduction** — 8,296 B → 336 B for 1,000 items (constant regardless of dataset)
- ✅ **314× async overhead** — Task.Yield simulation artifact, not production cost
- ✅ **Buffered 6–22% faster** vs previous run on .NET 10.0.5
- ⚠️ **Streaming 10k: +14%** vs .NET 10.0.5 — high StdDev, likely measurement noise

---

## 🏆 Framework Comparison Results

### FlowT vs Competitors
- **[FlowT vs DispatchR](FlowT.Benchmarks.FlowTvsDispatchRBenchmarks-report-github.md)** - Closest competitor (FlowT 2.9× faster)
- **[FlowT vs MediatR](FlowT.Benchmarks.FlowTvsMediatRBenchmarks-report-github.md)** - Most popular (FlowT 9× faster)
- **[FlowT vs MediatorNet](FlowT.Benchmarks.FlowTvsMediatorNetBenchmarks-report-github.md)** - 54× slower
- **[FlowT vs WolverineFx](FlowT.Benchmarks.FlowTvsWolverineFxBenchmarks-report-github.md)** - 14× slower
- **[FlowT vs Brighter](FlowT.Benchmarks.FlowTvsBrighterBenchmarks-report-github.md)** - 82× slower
- **[Complete Comparison](../DispatchR-Comparison.md)** - All frameworks ranked

**Performance Ranking (23.04.2026, .NET 10.0.6):**
1. 🥇 **FlowT** — ~29 ns / 144 B baseline
2. 🥈 **DispatchR** — 2.9× slower / 104 B (−28% less memory than FlowT)
3. 🥉 **MediatR** — 9× slower / 896 B
4. **WolverineFx** — 14× slower / 800 B
5. **Mediator.Net** — 54× slower / 2,144 B
6. **Brighter** — 82× slower / 4,777 B

---

## 🔥 Extreme Load Results

### Stress Test Scenarios
- **[Extreme Pipeline](FlowT.Benchmarks.ExtremePipelineBenchmarks.md)** - 10 specs + 10 policies + 10k items
  - **Perfect linear scaling** - 10× components = 10× time
  - **Zero overhead** for large payloads (10 MB)
  - **Excellent concurrency** - 100 parallel requests

**Scenarios Tested (23.04.2026, .NET 10.0.6):**
- **Large Pipelines** — 10 specs + 10 policies + 10 named keys: **1.040 μs / 464 B**
- **Large Payloads** — 10 MB + 10k items: **4.546 ms / 208 B**
- **High Concurrency** — 100 concurrent executions: **15.712 ms / 70,648 B**
- **Deep Nesting** — 10 policies + 10 MB payload: **4.602 ms / 304 B**

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
- .NET: 10.0.6
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

**Last Updated:** 2026-04-23 — .NET 10.0.6, BenchmarkDotNet v0.15.8
