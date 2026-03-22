# FlowT Benchmarks

Comprehensive benchmark suite for the **FlowT** orchestration library, comparing performance against popular .NET mediator frameworks and testing FlowT's internal components under various load conditions.

## 🎯 Quick Start

```bash
# Navigate to benchmarks directory
cd benchmarks\FlowT.Benchmarks

# Run interactive menu
.\scripts\run-benchmarks.ps1

# Or run specific benchmark categories
.\scripts\run-standard-benchmarks.ps1          # Core FlowT performance tests
.\scripts\run-comparison-benchmarks.ps1        # FlowT vs competitors
.\scripts\run-extreme-benchmarks.ps1           # Stress & load tests
.\scripts\run-streaming-benchmarks.ps1         # Streaming response benchmarks
.\scripts\run-plugin-benchmarks.ps1            # Plugin system benchmarks
```

---

## 📊 Benchmark Categories

### 1️⃣ **Core Performance Benchmarks**
Tests FlowT's internal components and operations:

| Benchmark | Description | Documentation |
|-----------|-------------|---------------|
| **FlowContext** | Context operations (Set/Get/Push/Pop/Named keys) | [📖 View Results](docs/results/FlowContext.md) |
| **FlowPipeline** | Pipeline execution (Specs, Policies, Handlers) | [📖 View Results](docs/results/FlowPipeline.md) |
| **Allocations** | Memory efficiency & throughput | [📖 View Results](docs/results/Allocations.md) |
| **Named Keys** | Named keys overhead vs default keys | [📖 View Results](docs/results/NamedKeys.md) |

### 2️⃣ **Framework Comparison Benchmarks**
Head-to-head comparisons with popular mediator frameworks:

| Framework | Speed vs FlowT | Memory vs FlowT | Documentation |
|-----------|----------------|-----------------|---------------|
| **DispatchR** | 2.7× slower | 28% less | [📖 Detailed Analysis](docs/DispatchR-Comparison.md) |
| **MediatR** | 9× slower | 6× more | [📖 View Results](docs/results/MediatR-Comparison.md) |
| **Mediator.Net** | 47× slower | 15× more | [📖 View Results](docs/results/MediatorNet-Comparison.md) |
| **WolverineFx** | 13.5× slower | 5× more | [📖 View Results](docs/results/WolverineFx-Comparison.md) |
| **Brighter** | 77× slower | 25× more | [📖 View Results](docs/results/Brighter-Comparison.md) |

**Key Takeaway:** FlowT is the fastest .NET mediator framework, with DispatchR as a close second optimizing for memory.

### 3️⃣ **Extreme Load Benchmarks**
Stress tests validating FlowT's behavior under extreme conditions:

| Test | Description | Result | Documentation |
|------|-------------|--------|---------------|
| **Extreme Pipeline** | 10 specs + 10 policies + 10 keys | 959 ns | [📖 View Results](docs/results/Extreme-Pipeline.md) |
| **Large Payload** | 10 MB data + 10k items | 4.6 ms | [📖 View Results](docs/results/Extreme-Payload.md) |
| **Concurrent Execution** | 100 parallel requests | 15.5 ms | [📖 View Results](docs/results/Extreme-Concurrency.md) |
| **Deep Nesting** | 10 policies + 10 MB payload | 4.6 ms | [📖 View Results](docs/results/Extreme-Nesting.md) |

**Key Takeaway:** FlowT exhibits perfect linear scaling with zero overhead for large payloads.

### 4️⃣ **Streaming Response Benchmarks**
Performance analysis of streaming vs buffered responses:

| Benchmark | Description | Key Metrics | Documentation |
|-----------|-------------|-------------|---------------|
| **StreamingBenchmarks** | Buffered vs streaming (100, 1k, 10k items) | 300× apparent slowdown (Task.Yield artifact) | [📖 View Results](docs/results/FlowT.Benchmarks.StreamingBenchmarks.md) |
| **StreamingComparisonBenchmarks** | Isolate real overhead (no Task.Yield) | **25× real overhead**, 96% memory reduction | [📖 View Results](docs/results/FlowT.Benchmarks.StreamingComparisonBenchmarks.md) |

**Key Findings:**
- ⚡ **Real overhead: 25×** (not 300×) - 18 ns per item vs 200 ns Task.Yield artifact
- 💾 **Memory: 96% reduction** - 8,296 B → 336 B (constant regardless of dataset size)
- 🚀 **TTFB: 1000× faster** - Progressive delivery vs waiting for all queries
- ♾️ **Scalability: Unlimited** - Handle millions of items without OOM

**When to use streaming:**
- Large datasets (>1,000 items)
- Memory-constrained environments
- Real-time dashboards/feeds
- Infinite scroll/pagination

**Documentation:** [📖 Streaming Benchmarks Guide](docs/Streaming-Benchmarks.md) - Complete analysis, decision guide, real-world examples

### 5️⃣ **Plugin System Benchmarks**
Performance analysis of plugin resolution, PerFlow caching, and `FlowPlugin` binding overhead:

| Benchmark | Description | Key Metrics | Documentation |
|-----------|-------------|-------------|---------------|
| **Cold access** | `Plugin<T>()` first call — DI resolution + cache population | 204 ns / 464 B | [📖 View Results](docs/results/FlowT.Benchmarks.PluginBenchmarks.md) |
| **Warm access** | `Plugin<T>()` repeated call — locked dict lookup | **23 ns / 0 B** | [📖 View Results](docs/results/FlowT.Benchmarks.PluginBenchmarks.md) |
| **Pipeline integration** | Policy (cold) + handler (warm) sharing one plugin | 264 ns / 560 B | [📖 View Results](docs/results/FlowT.Benchmarks.PluginBenchmarks.md) |

**Key Findings:**
- ⚡ **PerFlow cache speedup: 8.7×** — 204 ns → 23 ns after first access
- 💾 **Zero allocations on warm path** — locked dict lookup costs 0 B
- 🔌 **FlowPlugin overhead: 1 ns / 8 B** — `Initialize` adds nothing vs plain plugin
- 🔁 **Pipeline overhead: +110 ns / +168 B** — total cost of one cold + one warm call across policy + handler

**Script:** `.\scripts\run-plugin-benchmarks.ps1`

---

## 📈 Performance Summary

### **FlowT vs Fastest Competitor (DispatchR)**

| Scenario | FlowT | DispatchR | **FlowT Advantage** |
|----------|-------|-----------|---------------------|
| Simple handler | **30.8 ns** / 144 B | 86.4 ns / 104 B | **2.81× faster** ⚡ |
| Handler + Policy | **60.9 ns** / 232 B | 98.3 ns / 208 B | **1.61× faster** ⚡ |
| Handler + Validation | **45.3 ns** / 144 B | 82.8 ns / 112 B | **1.83× faster** ⚡ |

**Trade-off Analysis:**
- ✅ **FlowT:** 1.6-2.8× faster execution, rich platform (context, analyzers, modules)
- ✅ **DispatchR:** 22-28% less memory, minimalist pipeline

**Verdict:** FlowT invests +40 bytes (+38%) to deliver 2.7× speed + enterprise features (18 analyzers, modules, type-safe interrupts, distributed tracing).

### **Complete Framework Ranking**

| Rank | Framework | Speed | Memory | Notes |
|------|-----------|-------|--------|-------|
| 🥇 | **FlowT** | Baseline (fastest) | Baseline | Platform with rich features |
| 🥈 | **DispatchR** | 2.7× slower | **0.72× (best)** | Minimalist pipeline |
| 🥉 | **MediatR** | 9× slower | 6× more | Most popular |
| 4️⃣ | **WolverineFx** | 13.5× slower | 5× more | Feature-rich |
| 5️⃣ | **Mediator.Net** | 47× slower | 15× more | Legacy |
| 6️⃣ | **Brighter** | 77× slower | 25× more | Messaging-focused |

---

## 📚 Documentation

### **Getting Started**
- [📖 Quick Start Guide](docs/Quick-Start.md) - Running benchmarks, interpreting results
- [📖 Benchmark Guide](docs/Benchmark-Guide.md) - Deep dive into methodology, best practices
- [📖 Contributing Guide](docs/Contributing.md) - Adding custom benchmarks

### **Detailed Analysis**
- [📖 DispatchR Deep Dive](docs/DispatchR-Comparison.md) - Why FlowT is faster but uses more memory
- [📖 Extreme Load Analysis](docs/Extreme-Benchmarks.md) - Linear scaling, zero overhead validation
- [📖 Streaming Benchmarks Guide](docs/Streaming-Benchmarks.md) - Buffered vs streaming, real overhead analysis, decision guide
- [📖 Named Keys Performance](docs/Named-Keys.md) - Overhead measurement, real-world scenarios

### **Results Archives**
- [📁 Latest Results](docs/results/) - All benchmark outputs (markdown reports)
- [📊 Performance Targets](docs/Performance-Targets.md) - Expected results reference

---

## 🚀 Running Benchmarks

### **Interactive Menu (Recommended)**
```bash
cd benchmarks\FlowT.Benchmarks
.\scripts\run-benchmarks.ps1
```

**Menu Options:**
1. Standard Benchmarks (FlowContext, Pipeline, Allocations, Named Keys)
2. Comparison Benchmarks (FlowT vs Competitors)
3. Extreme Benchmarks (Stress & load tests)
4. Streaming Benchmarks (Response streaming performance)
5. All Benchmarks (Complete suite)

### **Direct Execution**

#### **1. Standard Benchmarks**
```bash
# Run all standard benchmarks
.\scripts\run-standard-benchmarks.ps1

# Run specific suite
.\scripts\run-standard-benchmarks.ps1 -Suite Context      # FlowContext operations
.\scripts\run-standard-benchmarks.ps1 -Suite Pipeline     # Pipeline execution
.\scripts\run-standard-benchmarks.ps1 -Suite Allocations  # Memory tests
.\scripts\run-standard-benchmarks.ps1 -Suite NamedKeys    # Named keys overhead

# Quick mode (faster, less accurate)
.\scripts\run-standard-benchmarks.ps1 -Quick

# Export results to docs/results/
.\scripts\run-standard-benchmarks.ps1 -Export
```

#### **2. Comparison Benchmarks**
```bash
# Run all competitor comparisons
.\scripts\run-comparison-benchmarks.ps1

# Compare with specific framework
.\scripts\run-comparison-benchmarks.ps1 -Framework DispatchR
.\scripts\run-comparison-benchmarks.ps1 -Framework MediatR
.\scripts\run-comparison-benchmarks.ps1 -Framework WolverineFx
.\scripts\run-comparison-benchmarks.ps1 -Framework Brighter

# Quick mode
.\scripts\run-comparison-benchmarks.ps1 -Quick -Framework DispatchR
```

#### **3. Extreme Benchmarks**
```bash
# Run all extreme tests
.\scripts\run-extreme-benchmarks.ps1

# Run specific test
.\scripts\run-extreme-benchmarks.ps1 -Test Extreme      # 10+10+10 pipeline
.\scripts\run-extreme-benchmarks.ps1 -Test Large        # 10 MB payload
.\scripts\run-extreme-benchmarks.ps1 -Test Concurrent   # 100 parallel requests
.\scripts\run-extreme-benchmarks.ps1 -Test Nesting      # Deep nesting

# Quick mode
.\scripts\run-extreme-benchmarks.ps1 -Quick
```

#### **4. Streaming Benchmarks**
```bash
# Run all streaming benchmarks
.\scripts\run-streaming-benchmarks.ps1

# Run specific suite
dotnet run -c Release --filter "*StreamingBenchmarks*"          # Original (with Task.Yield)
dotnet run -c Release --filter "*StreamingComparisonBenchmarks*" # Comparison (real overhead)

# Quick mode
.\scripts\run-streaming-benchmarks.ps1 -Quick
```

### **Manual Execution (Advanced)**
```bash
# Build in Release mode
dotnet build -c Release

# Run specific benchmark class
dotnet run -c Release --filter "*FlowContextBenchmarks*"

# Run with profiler
dotnet run -c Release --filter "*FlowContextBenchmarks*" --profiler ETW

# Run with memory randomization
dotnet run -c Release --memory-randomization
```

---

## 📊 Understanding Results

### **Key Metrics**
- **Mean** - Average execution time (lower is better)
- **Error** - Half of 99.9% confidence interval (lower is better)
- **StdDev** - Standard deviation (lower = more consistent)
- **Allocated** - Total memory allocated per operation (lower is better)
- **Gen0/Gen1/Gen2** - Garbage collection counts per 1000 operations

### **What to Look For**

#### **Speed Targets**
- FlowContext operations: **< 50 ns**
- Simple handler: **< 100 ns**
- Handler + policy: **< 150 ns**
- FlowT vs competitors: **2-10× faster**

#### **Memory Targets**
- FlowContext: **< 200 bytes**
- Simple handler: **< 500 bytes**
- FlowT vs competitors: **50-85% less allocation**
- **Streaming responses:** **< 500 bytes** (constant regardless of dataset size)

#### **Streaming Targets**
- Real overhead per item: **< 20 ns**
- Memory reduction: **> 90%** for large datasets (>1k items)
- TTFB improvement: **> 100×** with real database queries
- Scalability: **Constant memory** regardless of dataset size

#### **Consistency**
- StdDev should be < 5% of Mean
- Low Gen0 collections (< 0.1 per 1000 ops)

---

## 📁 Repository Structure

```
FlowT.Benchmarks/
├── README.md                                    # This file
├── docs/
│   ├── Quick-Start.md                           # Getting started guide
│   ├── Benchmark-Guide.md                       # Methodology & best practices
│   ├── DispatchR-Comparison.md                  # Detailed DispatchR analysis
│   ├── Extreme-Benchmarks.md                    # Extreme load analysis
│   ├── Streaming-Benchmarks.md                  # Streaming vs buffered analysis ✨ NEW
│   ├── Named-Keys.md                            # Named keys performance
│   ├── Performance-Targets.md                   # Expected results reference
│   ├── Contributing.md                          # Adding custom benchmarks
│   └── results/                                 # Benchmark results (markdown)
│       ├── FlowT.Benchmarks.FlowContextBenchmarks.md
│       ├── FlowT.Benchmarks.FlowPipelineBenchmarks.md
│       ├── FlowT.Benchmarks.AllocationBenchmarks.md
│       ├── FlowT.Benchmarks.NamedKeysComparisonBenchmarks.md
│       ├── FlowT.Benchmarks.FlowTvsMediatRBenchmarks.md
│       ├── FlowT.Benchmarks.FlowTvsDispatchRBenchmarks.md
│       ├── FlowT.Benchmarks.ExtremePipelineBenchmarks.md
│       ├── FlowT.Benchmarks.StreamingBenchmarks.md              ✨ NEW
│       └── FlowT.Benchmarks.StreamingComparisonBenchmarks.md    ✨ NEW
├── scripts/                                     # Benchmark execution scripts
│   ├── run-benchmarks.ps1                       # Interactive menu
│   ├── run-standard-benchmarks.ps1              # Core performance tests
│   ├── run-comparison-benchmarks.ps1            # Competitor comparisons
│   ├── run-extreme-benchmarks.ps1               # Stress tests
│   └── run-streaming-benchmarks.ps1             # Streaming performance tests ✨ NEW
├── *.cs                                         # Benchmark implementations
│   ├── StreamingBenchmarks.cs                   # ✨ NEW
│   ├── StreamingComparisonBenchmarks.cs         # ✨ NEW
│   └── ... (other benchmark classes)
└── BenchmarkDotNet.Artifacts/                   # Raw BenchmarkDotNet output
```

---

## 🔧 Customization

### **Adding Custom Benchmarks**

1. Create a new benchmark class:
```csharp
using BenchmarkDotNet.Attributes;
using FlowT;

namespace FlowT.Benchmarks;

[MemoryDiagnoser]
[MarkdownExporter]
public class MyCustomBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark(Baseline = true)]
    public void MyBaseline()
    {
        // Baseline implementation
    }

    [Benchmark]
    public void MyOptimization()
    {
        // Optimized implementation
    }
}
```

2. Run your benchmark:
```bash
dotnet run -c Release --filter "*MyCustomBenchmarks*"
```

3. Document results in `docs/results/MyCustom.md`

---

## ⚡ Optimization Tips

If benchmarks show performance regressions:

1. **High allocations?**
   - Check for unnecessary boxing
   - Verify ValueTask usage
   - Look for closure allocations
   - Profile with `[MemoryDiagnoser]`

2. **Slow execution?**
   - Profile with `[InliningDiagnoser]`
   - Check for unnecessary async state machines
   - Verify hot-path optimizations
   - Use `[DisassemblyDiagnoser]` to inspect generated code

3. **Poor throughput?**
   - Check for lock contention
   - Verify thread-safety mechanisms
   - Profile with `[ThreadingDiagnoser]`
   - Inspect GC pressure

---

## 🔍 Additional Resources

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Writing High-Performance .NET Code](https://github.com/adamsitnik/awesome-dot-net-performance)
- [FlowT Main Repository](https://github.com/vlasta81/FlowT)
- [FlowT Performance Philosophy](../../README.md#-performance-comparison)

---

## 🤝 Contributing

When adding benchmarks:
1. ✅ Use descriptive names
2. ✅ Add XML documentation
3. ✅ Include `[MemoryDiagnoser]` and `[MarkdownExporter]`
4. ✅ Set appropriate baselines with `[Baseline = true]`
5. ✅ Document expected results in `docs/results/`
6. ✅ Test before committing
7. ✅ Update this README with new benchmarks

---

## 📝 Notes

- **System Requirements:** Benchmarks should be run on a quiet system with minimal background processes
- **Consistency:** Run benchmarks multiple times and compare results to ensure statistical significance
- **Environment:** Results are hardware-dependent - document your test environment
- **Versioning:** Benchmark results are tied to specific framework versions (documented in result files)
- **Streaming Performance:** Real-world overhead is ~25× (not 300×), Task.Yield in simulations adds artificial delay

---

**Last Updated:** 2026-03-21  
**FlowT Version:** Latest  
**BenchmarkDotNet Version:** 0.15.8  
**Target Framework:** .NET 10.0.5
