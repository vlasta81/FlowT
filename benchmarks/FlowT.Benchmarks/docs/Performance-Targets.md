# FlowT Performance Targets

This document defines expected performance baselines for FlowT operations. Use these values to verify your benchmarks are running correctly and to detect performance regressions.

## Table of Contents

- [System Requirements](#system-requirements)
- [Core Operations](#core-operations)
- [Pipeline Execution](#pipeline-execution)
- [Context Operations](#context-operations)
- [Comparison Targets](#comparison-targets)
- [Extreme Load Targets](#extreme-load-targets)
- [Detecting Regressions](#detecting-regressions)

## System Requirements

### Reference Hardware

All baseline measurements are from:

- **CPU:** AMD Ryzen 9 5900X (12-core, 3.7 GHz base, 4.8 GHz boost)
- **RAM:** 32 GB DDR4-3600 CL16
- **OS:** Windows 11 23H2 (Build 22631.4037)
- **.NET:** 10.0.100
- **BenchmarkDotNet:** 0.15.8

### Adjusting for Your Hardware

**If your CPU is faster/slower:**

```
Adjusted Target = Baseline × (Baseline CPU GHz / Your CPU GHz)
```

**Example:** Your CPU is 3.0 GHz (vs 3.7 GHz baseline)
```
Adjusted Target = 30 ns × (3.7 / 3.0) = 37 ns
```

**Rule of thumb:**
- ±20% variance is normal (CPU differences, background tasks)
- > 50% deviation requires investigation

## Core Operations

### FlowContext Creation

| Operation                 | Target   | Allocated | Notes                          |
|---------------------------|----------|-----------|--------------------------------|
| Empty context             | 20-30 ns | 144 B     | Baseline (no services)         |
| With IServiceProvider     | 25-35 ns | 144 B     | Typical ASP.NET Core scenario  |
| With HttpContext          | 30-40 ns | 144 B     | Web API scenario               |

**Validation:**
```csharp
[Benchmark]
public FlowContext CreateContext()
{
    return new FlowContext(_serviceProvider);
}
```

**Expected output:**
```
Method        | Mean   | Allocated
CreateContext | 27.3 ns| 144 B
```

### Context Storage Operations

| Operation                 | Target   | Allocated | Notes                          |
|---------------------------|----------|-----------|--------------------------------|
| Set (default key)         | 10-15 ns | 0 B       | Type-based lookup              |
| Get (default key)         | 10-15 ns | 0 B       | Type-based retrieval           |
| Set (named key)           | 15-20 ns | 32 B      | String hashing overhead        |
| Get (named key)           | 15-20 ns | 0 B       | String-based retrieval         |
| TryGet (exists)           | 10-15 ns | 0 B       | Success path                   |
| TryGet (not exists)       | 8-12 ns  | 0 B       | Early exit                     |

**Key insight:** Named keys add ~5-7 ns overhead (string hashing)

## Pipeline Execution

### Simple Scenarios

| Scenario                  | Target   | Allocated | Components                     |
|---------------------------|----------|-----------|--------------------------------|
| Empty handler             | 25-35 ns | 144 B     | Just context creation          |
| Single handler            | 30-40 ns | 144 B     | One handler execution          |
| Handler + 1 spec          | 40-50 ns | 168 B     | Specification check            |
| Handler + 1 policy        | 45-55 ns | 184 B     | Policy execution               |

### Complex Scenarios

| Scenario                  | Target    | Allocated | Components                     |
|---------------------------|-----------|-----------|--------------------------------|
| 3 specs + 3 policies      | 100-120 ns| 280 B     | Typical pipeline               |
| 5 specs + 5 policies      | 160-180 ns| 400 B     | Complex pipeline               |
| 10 specs + 10 policies    | 280-320 ns| 700 B     | Extreme pipeline               |

**Scaling rule:** Each additional component adds ~25-30 ns

### Validation:**

```csharp
[Benchmark]
public async Task<string> SimpleHandler()
{
    var request = new PingRequest { Message = "Hello" };
    return await _flow.ExecuteAsync(request);
}
```

**Expected output:**
```
Method        | Mean   | Allocated
SimpleHandler | 32.1 ns| 144 B
```

## Context Operations

### HTTP Context Integration

| Operation                 | Target   | Allocated | Notes                          |
|---------------------------|----------|-----------|--------------------------------|
| GetHttpContext()          | 5-8 ns   | 0 B       | Property access                |
| GetUser()                 | 10-15 ns | 0 B       | HttpContext.User               |
| IsAuthenticated()         | 10-15 ns | 0 B       | Identity check                 |
| GetHeader(name)           | 15-20 ns | 24 B      | String allocation              |
| SetStatusCode(code)       | 8-12 ns  | 0 B       | Response property set          |
| GetQueryParam(key)        | 20-25 ns | 32 B      | Query parsing + string alloc   |
| GetRouteValue(key)        | 15-20 ns | 24 B      | Route data lookup              |
| GetClientIp()             | 25-35 ns | 40 B      | Connection info + string       |
| SetHeader(name, value)    | 20-25 ns | 48 B      | Two string allocations         |
| GetContentType()          | 10-15 ns | 0 B       | Content-Type header            |

**Note:** String allocations are expected (immutable strings)

### Service Resolution

| Operation                 | Target   | Allocated | Notes                          |
|---------------------------|----------|-----------|--------------------------------|
| GetService<T>()           | 30-40 ns | 0-64 B    | DI container lookup            |
| GetRequiredService<T>()   | 35-45 ns | 0-64 B    | With null check                |
| GetServices<T>()          | 50-70 ns | 120 B     | Collection allocation          |

**Note:** Allocation depends on service lifetime (transient allocates, singleton doesn't)

## Comparison Targets

### FlowT vs Competitors

Expected speedup ratios (FlowT as baseline = 1.00×):

| Framework    | Simple  | + Policy | + Validation | Memory Ratio |
|--------------|---------|----------|--------------|--------------|
| FlowT        | 1.00×   | 1.00×    | 1.00×        | 1.00×        |
| DispatchR    | 2.5-3.0×| 1.5-2.0× | 1.8-2.2×     | 0.70-0.75×   |
| MediatR      | 8-10×   | 6-8×     | 7-9×         | 1.5-2.0×     |
| Mediator.Net | 45-50× | 35-40×   | 40-45×       | 3.0-4.0×     |
| WolverineFx  | 12-15× | 10-13×   | 11-14×       | 2.5-3.5×     |
| Brighter     | 75-80× | 60-70×   | 70-75×       | 5.0-6.0×     |

**Ranking (Speed):**
1. FlowT (baseline)
2. DispatchR (2.7× average)
3. MediatR (9× average)
4. WolverineFx (13.5× average)
5. Mediator.Net (47× average)
6. Brighter (77× average)

### Absolute Performance Targets

| Framework    | Simple Handler | Handler + Policy | Handler + Validation |
|--------------|----------------|------------------|----------------------|
| FlowT        | 30-35 ns       | 60-70 ns         | 45-55 ns             |
| DispatchR    | 80-90 ns       | 95-105 ns        | 80-90 ns             |
| MediatR      | 270-290 ns     | 400-450 ns       | 350-400 ns           |
| Mediator.Net | 1400-1500 ns   | 2200-2400 ns     | 1900-2100 ns         |
| WolverineFx  | 400-450 ns     | 650-750 ns       | 550-650 ns           |
| Brighter     | 2300-2500 ns   | 4300-4700 ns     | 3500-3900 ns         |

## Extreme Load Targets

### Extreme Pipeline (10+10+10)

| Metric        | Target    | Notes                          |
|---------------|-----------|--------------------------------|
| Execution time| 280-320 ns| Linear scaling (10× baseline)  |
| Allocated     | 650-750 B | ~65 B per component            |
| Specs executed| 10        | All must pass                  |
| Policies run  | 10        | Sequential execution           |
| Keys accessed | 10        | Named key overhead             |

**Scaling validation:**
```
1 component:  ~30 ns
10 components: ~300 ns (10× = linear ✅)
20 components: ~600 ns (20× = linear ✅)
```

### Large Payload (10 MB + 10k Items)

| Metric        | Target    | Notes                          |
|---------------|-----------|--------------------------------|
| Execution time| 1.1-1.3 ms| Dominated by memory allocation |
| Allocated     | ~10.2 MB  | Payload + overhead (~2%)       |
| Serialization | N/A       | Not tested (pure FlowT logic)  |

**Memory validation:**
```
1 MB:   ~10 MB allocated (~2% overhead)
10 MB:  ~10.2 MB allocated (~2% overhead)
100 MB: ~102 MB allocated (~2% overhead)
```

### Concurrent Execution (100 Parallel)

| Metric        | Target    | Notes                          |
|---------------|-----------|--------------------------------|
| Time/request  | 32-38 ns  | 10-20% overhead vs single      |
| Throughput    | 280-310M/s| Requests per second            |
| Contention    | < 20%     | Lock overhead                  |
| Memory/request| 144 B     | No shared state                |

**Concurrency validation:**
```
1 thread:   30 ns (baseline)
10 threads: 33 ns (+10% = excellent)
100 threads: 35 ns (+17% = excellent)
1000 threads: 42 ns (+40% = acceptable)
```

### Deep Nesting (10 Policies + Large Payload)

| Metric        | Target    | Notes                          |
|---------------|-----------|--------------------------------|
| Execution time| 1.4-1.6 ms| Combined overhead              |
| Allocated     | ~10.5 MB  | Policies + payload             |
| Stack depth   | 10 frames | No stack overflow              |

**Composition validation:**
```
10 policies alone:  ~270 ns
10 MB payload alone: ~1.2 ms
Combined: ~1.47 ms (sum = linear ✅)
```

## Detecting Regressions

### Red Flags (Investigate Immediately)

| Symptom                   | Threshold | Possible Cause                     |
|---------------------------|-----------|------------------------------------|
| 50% slower                | > 45 ns   | Major regression in core logic     |
| 2× memory allocation      | > 288 B   | Memory leak or extra allocations   |
| High variance (StdDev)    | > 20%     | Background noise or unstable code  |
| Non-linear scaling        | > 10%     | Algorithm change (O(n) → O(n²))    |

### Yellow Flags (Review Needed)

| Symptom                   | Threshold | Possible Cause                     |
|---------------------------|-----------|------------------------------------|
| 20-50% slower             | 36-45 ns  | Minor regression or CPU difference |
| 20-50% more memory        | 173-216 B | Added feature or string allocation |
| 10-20% variance           | 10-20%    | Background tasks or thermal throttle|

### Green Zone (Expected)

| Symptom                   | Threshold | Notes                              |
|---------------------------|-----------|------------------------------------|
| < 20% deviation           | 24-36 ns  | Normal variance across machines    |
| < 20% extra memory        | 144-173 B | Acceptable overhead                |
| < 10% variance            | < 10%     | Stable, reproducible results       |

### Example: Detecting Regression

**Baseline (v1.0.0):**
```
FlowT_SimpleHandler | 30.8 ns | 144 B
```

**New version (v1.1.0):**
```
FlowT_SimpleHandler | 52.3 ns | 144 B  ← ⚠️ 70% slower!
```

**Action:**
1. Verify hardware (same machine? same OS?)
2. Check build configuration (Release mode?)
3. Profile hot path (`dotnet trace collect`)
4. Review recent changes (`git diff v1.0.0..v1.1.0`)
5. Bisect commits to find regression

### Automation Example

```powershell
# Run baseline
.\scripts\run-standard-benchmarks.ps1 -Export
$baseline = Get-Content "docs/results/FlowT.Benchmarks.FlowContextBenchmarks-report.md"

# Apply changes
git commit -m "Optimize: FlowContext creation"

# Run new benchmark
.\scripts\run-standard-benchmarks.ps1 -Export
$new = Get-Content "docs/results/FlowT.Benchmarks.FlowContextBenchmarks-report.md"

# Compare (manual or CI script)
if ($new.Mean -gt $baseline.Mean * 1.2) {
    Write-Error "❌ Performance regression detected!"
    exit 1
}
```

## Best Practices

### ✅ 1. Run Baselines Before Optimizing

```powershell
git tag v1.0.0-baseline
.\scripts\run-standard-benchmarks.ps1 -Export
git add docs/results/*.md
git commit -m "Baseline: v1.0.0 benchmarks"
```

### ✅ 2. Compare Ratios, Not Absolute Numbers

```
❌ Bad:  "My machine gets 50 ns (yours is 30 ns, so I'm slower)"
✅ Good: "FlowT is 2.8× faster than DispatchR (consistent across machines)"
```

### ✅ 3. Track Trends Over Time

```
v1.0.0: 30.8 ns
v1.1.0: 29.2 ns (✅ 5% faster)
v1.2.0: 31.5 ns (⚠️ 2% slower than baseline)
v1.3.0: 28.1 ns (✅ 9% faster than baseline)
```

### ✅ 4. Test on Target Hardware

If deploying to:
- **Azure B2s VM** - Benchmark on B2s instance
- **AWS Lambda** - Test with similar CPU limits
- **On-premises** - Match production specs

### ✅ 5. Document Acceptable Trade-Offs

```markdown
## v1.2.0 Performance Change

**Change:** Added FlowContext event notification

**Impact:**
- Speed: 30.8 ns → 32.1 ns (+4.2%)
- Memory: 144 B → 168 B (+16.7%)

**Justification:**
Event notification enables observability (tracing, metrics).
4% slowdown is acceptable for enterprise features.

**Verdict:** ✅ Approved (benefits > costs)
```

## Summary Table

### Quick Reference

| Category         | Operation             | Target       | Allocated | Status |
|------------------|-----------------------|--------------|-----------|--------|
| Context          | Create context        | 25-35 ns     | 144 B     | ✅     |
| Context          | Set (default key)     | 10-15 ns     | 0 B       | ✅     |
| Context          | Set (named key)       | 15-20 ns     | 32 B      | ✅     |
| Pipeline         | Simple handler        | 30-40 ns     | 144 B     | ✅     |
| Pipeline         | 10 specs + 10 policies| 280-320 ns   | 700 B     | ✅     |
| HTTP             | GetUser()             | 10-15 ns     | 0 B       | ✅     |
| HTTP             | GetHeader(name)       | 15-20 ns     | 24 B      | ✅     |
| Comparison       | FlowT vs DispatchR    | 1.0× : 2.7×  | -         | ✅     |
| Comparison       | FlowT vs MediatR      | 1.0× : 9.0×  | -         | ✅     |
| Extreme          | 10+10+10 pipeline     | 280-320 ns   | 700 B     | ✅     |
| Extreme          | 10 MB payload         | 1.1-1.3 ms   | ~10.2 MB  | ✅     |
| Extreme          | 100 concurrent        | 32-38 ns/req | 144 B     | ✅     |

## Next Steps

- **Run benchmarks** → See [Quick-Start.md](Quick-Start.md)
- **Understand methodology** → See [Benchmark-Guide.md](Benchmark-Guide.md)
- **Compare with competitors** → See [DispatchR-Comparison.md](DispatchR-Comparison.md)
- **Test extreme loads** → See [Extreme-Benchmarks.md](Extreme-Benchmarks.md)
- **Add custom benchmarks** → See [Contributing.md](Contributing.md)

---

**Questions or issues?** Open an issue on [GitHub](https://github.com/vlasta81/FlowT)
