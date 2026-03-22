# Quick Start Guide - FlowT Benchmarks

This guide will help you run FlowT benchmarks and interpret the results.

## 🎯 Prerequisites

- **.NET 10 SDK** or later
- **PowerShell 7+** (for running scripts)
- **Windows, Linux, or macOS**
- **8+ GB RAM** (16+ GB recommended for accurate results)
- **Quiet system** (close background applications during benchmarking)

---

## 🚀 Running Your First Benchmark

### **Option 1: Interactive Menu (Easiest)**

```bash
# Navigate to benchmarks directory
cd benchmarks\FlowT.Benchmarks

# Run interactive menu
.\scripts\run-benchmarks.ps1
```

**Menu will display:**
```
FlowT Benchmark Runner
================================
1) Standard Benchmarks (Core performance)
2) Comparison Benchmarks (FlowT vs competitors)
3) Extreme Benchmarks (Stress & load tests)
4) Streaming Benchmarks (Response streaming)
5) Plugin Benchmarks (Plugin system performance)
6) All Benchmarks (Complete suite)
Q) Quit

Select an option [1-6, Q]:
```

### **Option 2: Direct Execution**

Run specific benchmark categories:

```bash
# Core performance tests
.\scripts\run-standard-benchmarks.ps1

# Competitor comparisons
.\scripts\run-comparison-benchmarks.ps1

# Stress tests
.\scripts\run-extreme-benchmarks.ps1

# Streaming performance tests
.\scripts\run-streaming-benchmarks.ps1

# Plugin system performance
.\scripts\run-plugin-benchmarks.ps1
```

---

## 📊 Available Benchmark Suites

### **1. Standard Benchmarks** (`run-standard-benchmarks.ps1`)

Tests FlowT's internal components:

```bash
# Run all standard benchmarks
.\scripts\run-standard-benchmarks.ps1

# Run specific component
.\scripts\run-standard-benchmarks.ps1 -Suite Context      # FlowContext operations
.\scripts\run-standard-benchmarks.ps1 -Suite Pipeline     # Pipeline execution
.\scripts\run-standard-benchmarks.ps1 -Suite Allocations  # Memory efficiency
.\scripts\run-standard-benchmarks.ps1 -Suite NamedKeys    # Named keys overhead
```

**What's tested:**
- **FlowContext** - Set/Get/Push/Pop/Named keys operations
- **Pipeline** - Specs, Policies, Handlers execution
- **Allocations** - Memory efficiency, throughput, concurrency
- **Named Keys** - Overhead measurement vs default keys

**Expected duration:** 2-5 minutes per suite

### **2. Comparison Benchmarks** (`run-comparison-benchmarks.ps1`)

Compares FlowT with competitor frameworks:

```bash
# Run all competitor comparisons
.\scripts\run-comparison-benchmarks.ps1

# Compare with specific framework
.\scripts\run-comparison-benchmarks.ps1 -Framework DispatchR
.\scripts\run-comparison-benchmarks.ps1 -Framework MediatR
.\scripts\run-comparison-benchmarks.ps1 -Framework WolverineFx
.\scripts\run-comparison-benchmarks.ps1 -Framework MediatorNet
.\scripts\run-comparison-benchmarks.ps1 -Framework Brighter
```

**What's compared:**
- Simple handler (baseline)
- Handler + policy/behavior
- Handler + validation

**Expected duration:** 3-5 minutes per framework

### **3. Extreme Benchmarks** (`run-extreme-benchmarks.ps1`)

Stress tests validating behavior under extreme load:

```bash
# Run all extreme tests
.\scripts\run-extreme-benchmarks.ps1

# Run specific test
.\scripts\run-extreme-benchmarks.ps1 -Test Extreme      # 10 specs + 10 policies + 10 keys
.\scripts\run-extreme-benchmarks.ps1 -Test Large        # 10 MB payload
.\scripts\run-extreme-benchmarks.ps1 -Test Concurrent   # 100 parallel requests
.\scripts\run-extreme-benchmarks.ps1 -Test Nesting      # Deep nesting
```

**What's tested:**
- **Extreme Pipeline** - Linear scaling validation (30 components)
- **Large Payload** - Zero overhead for big data (10 MB)
- **Concurrent Execution** - Thread-safety (100 parallel)
- **Deep Nesting** - Combined complexity (10 policies + 10 MB)

**Expected duration:** 5-10 minutes per test

### **4. Streaming Benchmarks** (`run-streaming-benchmarks.ps1`)

Performance analysis of streaming vs buffered responses:

```bash
# Run all streaming benchmarks
.\scripts\run-streaming-benchmarks.ps1

# Run specific suite
dotnet run -c Release --filter "*StreamingBenchmarks*"          # Original (with Task.Yield)
dotnet run -c Release --filter "*StreamingComparisonBenchmarks*" # Comparison (real overhead)
```

**What's tested:**
- **StreamingBenchmarks** - Buffered vs streaming across 100, 1k, 10k items
- **StreamingComparisonBenchmarks** - Real overhead isolation (no Task.Yield)

**Key findings:**
- Real overhead: 25× (not 300×)
- Memory reduction: 96% (8,296 B → 336 B)
- TTFB improvement: 1000× with real databases

**Expected duration:** 2-3 minutes per suite

**Documentation:** See [Streaming Benchmarks Guide](Streaming-Benchmarks.md) for decision guide

### **5. Plugin Benchmarks** (`run-plugin-benchmarks.ps1`)

Plugin system performance — resolution cost, PerFlow caching, and `FlowPlugin` binding overhead:

```bash
# Run all plugin benchmarks
.\scripts\run-plugin-benchmarks.ps1

# Run specific benchmark
dotnet run -c Release --filter "*PluginBenchmarks*"
```

**What's tested:**
- **Cold access** - `Plugin<T>()` first call: DI resolution + cache population (204 ns / 464 B)
- **Warm access** - `Plugin<T>()` repeated call: locked dict lookup (23 ns / 0 B)
- **FlowPlugin vs plain plugin** - `Initialize` overhead: 1 ns / 8 B
- **3 plugin types** - Multi-type cold/warm resolution
- **Pipeline integration** - No plugin (baseline), cold+warm, pre-warmed scenarios

**Key findings:**
- PerFlow cache speedup: **8.7×** (204 ns → 23 ns)
- `FlowPlugin` overhead vs plain: **1 ns / 8 B** (negligible)
- Zero allocations on all warm paths
- Pipeline overhead (one cold + one warm): **+110 ns / +168 B**

**Expected duration:** ~3-5 minutes

**Results:** [📖 View Results](results/FlowT.Benchmarks.PluginBenchmarks.md)

---

## ⚙️ Script Options

All scripts support these common options:

### **Quick Mode**
Runs faster but less accurate (fewer iterations):

```bash
.\scripts\run-standard-benchmarks.ps1 -Quick
.\scripts\run-comparison-benchmarks.ps1 -Quick -Framework MediatR
.\scripts\run-extreme-benchmarks.ps1 -Quick
```

**Use when:** You want quick feedback during development  
**Duration:** 30-60 seconds per suite (vs 2-5 minutes normal)

### **Export Results**
Automatically exports results to `docs/results/`:

```bash
.\scripts\run-standard-benchmarks.ps1 -Export
.\scripts\run-comparison-benchmarks.ps1 -Export -Framework DispatchR
.\scripts\run-extreme-benchmarks.ps1 -Export
```

**Output:** Markdown files in `docs/results/` with formatted tables

---

## 📈 Understanding Results

### **Console Output**

BenchmarkDotNet will display results like this:

```
| Method                     | Mean     | Error    | StdDev   | Allocated |
|--------------------------- |---------:|---------:|---------:|----------:|
| FlowT_SimpleHandler        | 30.78 ns | 0.151 ns | 0.134 ns |     144 B |
| DispatchR_SimpleHandler    | 86.37 ns | 0.422 ns | 0.395 ns |     104 B |
```

### **Key Metrics Explained**

#### **Mean**
- Average execution time
- **Lower is better**
- Target: FlowT < 100 ns for simple handlers

#### **Error**
- Half of 99.9% confidence interval
- **Lower is better**
- Good: < 5% of Mean

#### **StdDev (Standard Deviation)**
- Consistency of measurements
- **Lower is better**
- Good: < 5% of Mean

#### **Allocated**
- Total memory allocated per operation
- **Lower is better**
- Target: FlowT < 500 B for simple handlers

#### **Gen0/Gen1/Gen2**
- Garbage collection counts per 1000 operations
- **Lower is better**
- Target: Gen0 < 0.1, Gen1/Gen2 = 0

---

## 🎯 Interpreting Performance

### **Speed Comparisons**

When comparing FlowT to competitors:

| Ratio | Meaning | Interpretation |
|-------|---------|----------------|
| **1.5× faster** | Slightly faster | Good |
| **2-5× faster** | Significantly faster | Very good |
| **5-10× faster** | Much faster | Excellent |
| **10+× faster** | Dramatically faster | Outstanding |

**Example:**
```
FlowT: 30 ns
DispatchR: 86 ns
Ratio: 86 / 30 = 2.87× faster ✅
```

### **Memory Comparisons**

| Ratio | Meaning | Interpretation |
|-------|---------|----------------|
| **0.5× allocation** | Half the memory | Excellent |
| **1× allocation** | Same memory | Good |
| **2× allocation** | Double memory | Acceptable if speed gain |
| **5+× allocation** | Much more memory | Investigate |

**Example:**
```
FlowT: 144 B
DispatchR: 104 B
Ratio: 144 / 104 = 1.38× more memory ⚠️

But FlowT is 2.87× faster, so trade-off is worth it! ✅
```

---

## 🔍 Common Issues & Troubleshooting

### **Issue: "No benchmarks found"**

**Cause:** Filter doesn't match any benchmark classes

**Solution:**
```bash
# Run without filter to see all available benchmarks
dotnet run -c Release --list flat

# Then run specific benchmark
dotnet run -c Release --filter "*FlowContextBenchmarks*"
```

### **Issue: High variance (large StdDev)**

**Cause:** Background processes, thermal throttling, power management

**Solutions:**
1. Close all background applications
2. Disable power-saving modes
3. Let CPU cool down between runs
4. Use `--memory-randomization` flag
5. Run benchmarks multiple times and compare

### **Issue: Out of memory**

**Cause:** Extreme benchmarks (Large Payload, Concurrent Execution)

**Solution:**
```bash
# Increase memory limit
$env:DOTNET_GCHeapHardLimit = "4GB"
.\scripts\run-extreme-benchmarks.ps1 -Test Concurrent
```

### **Issue: Slow execution (taking hours)**

**Cause:** Debug mode or too many iterations

**Solution:**
```bash
# Always use Release mode
dotnet build -c Release

# Use Quick mode during development
.\scripts\run-standard-benchmarks.ps1 -Quick
```

---

## 📊 Expected Results (Reference)

### **Standard Benchmarks**

| Benchmark | Target Time | Target Memory |
|-----------|-------------|---------------|
| FlowContext.Set | < 50 ns | < 200 B |
| FlowContext.Get | < 30 ns | 0 B |
| Simple Handler | < 100 ns | < 500 B |
| Handler + Policy | < 150 ns | < 750 B |

### **Comparison Benchmarks**

| Framework | Expected Speedup | Expected Memory |
|-----------|------------------|-----------------|
| vs DispatchR | 2-3× faster | +30-40% more |
| vs MediatR | 6-10× faster | 75-85% less |
| vs WolverineFx | 10-15× faster | 80% less |
| vs Brighter | 50-80× faster | 90% less |

### **Extreme Benchmarks**

| Test | Expected Time | Expected Memory |
|------|---------------|-----------------|
| Extreme Pipeline (10+10+10) | ~1 μs | < 500 B |
| Large Payload (10 MB) | ~5 ms | < 300 B |
| Concurrent (100 parallel) | ~20 ms | < 100 KB |
| Deep Nesting (10 + 10 MB) | ~5 ms | < 500 B |

---

## 🚀 Next Steps

### **After Running Benchmarks:**

1. **Review Results**
   - Check console output
   - Look for performance regressions
   - Compare with expected targets

2. **Export Results**
   ```bash
   .\scripts\run-standard-benchmarks.ps1 -Export
   ```
   Results saved to `docs/results/`

3. **Detailed Analysis**
   - Read [Benchmark Guide](Benchmark-Guide.md) for methodology
   - Check [DispatchR Comparison](DispatchR-Comparison.md) for deep dive
   - Review [Performance Targets](Performance-Targets.md) for baselines

4. **Contribute Results**
   - Document your environment (CPU, RAM, OS)
   - Add results to `docs/results/`
   - Update this guide if you find improvements

---

## 📚 Additional Resources

- [Benchmark Guide](Benchmark-Guide.md) - Deep dive into methodology
- [DispatchR Comparison](DispatchR-Comparison.md) - Detailed analysis
- [Extreme Benchmarks](Extreme-Benchmarks.md) - Stress test analysis
- [Named Keys Performance](Named-Keys.md) - Named keys overhead
- [Contributing Guide](Contributing.md) - Adding custom benchmarks

---

## 💡 Pro Tips

1. **Always use Release mode** - Debug mode is 10-100× slower
2. **Run multiple times** - Compare results for consistency
3. **Close background apps** - Minimize interference
4. **Let CPU cool down** - Between long-running benchmarks
5. **Document environment** - CPU, RAM, OS version matter
6. **Use Quick mode for development** - Full mode for final results
7. **Export results automatically** - Use `-Export` flag

---

**Happy Benchmarking! 🚀**

For questions or issues, open an issue on [GitHub](https://github.com/vlasta81/FlowT/issues).
