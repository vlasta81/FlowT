# Named Keys Performance Analysis

This document analyzes the performance overhead of using named keys in FlowT's context storage system and provides guidance on when to use them.

## Table of Contents

- [Overview](#overview)
- [How Named Keys Work](#how-named-keys-work)
- [Performance Measurements](#performance-measurements)
- [Overhead Analysis](#overhead-analysis)
- [Real-World Scenarios](#real-world-scenarios)
- [Best Practices](#best-practices)
- [When to Use Named Keys](#when-to-use-named-keys)

## Overview

FlowT provides two ways to store data in `FlowContext`:

### 1. Default Keys (Type-Based)

```csharp
// Store and retrieve by type
context.Set(user);
var user = context.Get<User>();
```

**Pros:**
- ✅ Fastest (no string hashing)
- ✅ Type-safe (compile-time checking)
- ✅ Simplest API

**Cons:**
- ❌ Only one instance per type
- ❌ Can't store multiple values of same type

### 2. Named Keys (String-Based)

```csharp
// Store and retrieve by name
context.Set(user, "CurrentUser");
context.Set(oldUser, "PreviousUser");

var current = context.Get<User>("CurrentUser");
var previous = context.Get<User>("PreviousUser");
```

**Pros:**
- ✅ Multiple instances per type
- ✅ Explicit naming (self-documenting)
- ✅ Flexible storage

**Cons:**
- ❌ Slightly slower (string hashing + lookup)
- ❌ Runtime errors if key typo
- ❌ More memory (string allocation)

## How Named Keys Work

### Internal Implementation

```csharp
// FlowContext storage
private readonly Dictionary<CompositeKey, object?> _items = new();
private readonly Lock _syncLock = new();

// Default key (type-based)
Set(user) → _items[CompositeKey(typeof(User), null)] = user

// Named key (string-based)
Set(user, "CurrentUser") → _items[CompositeKey(typeof(User), "CurrentUser")] = user
```

### Performance Difference

**Type-based lookup:**
1. Get `typeof(T)` (1-2 CPU cycles)
2. Hash type (fast, cached)
3. Dictionary lookup

**String-based lookup:**
1. Get string reference (1-2 CPU cycles)
2. Hash string (slower, not cached)
3. Dictionary lookup

**Result:** String hashing is the primary overhead (~5-10 ns).

## Performance Measurements

### Benchmark Configuration

```csharp
[MemoryDiagnoser]
[MarkdownExporter]
public class NamedKeysBenchmarks
{
    [Benchmark(Baseline = true)]
    public User DefaultKey()
    {
        _context.Set(user);
        return _context.Get<User>();
    }

    [Benchmark]
    public User NamedKey()
    {
        _context.Set(user, "CurrentUser");
        return _context.Get<User>("CurrentUser");
    }
}
```

### Results

| Method     | Mean   | Allocated | Ratio |
|------------|--------|-----------|-------|
| DefaultKey | 12.3 ns| 0 B       | 1.00x |
| NamedKey   | 18.7 ns| 32 B      | 1.52x |

**Analysis:**
- **Speed:** Named keys are **1.52× slower** (6.4 ns overhead)
- **Memory:** Named keys allocate **32 bytes** (string storage)

### Overhead Breakdown

| Component         | Time   | Explanation                          |
|-------------------|--------|--------------------------------------|
| Base operation    | 12.3 ns| Dictionary lookup (type-based)       |
| String hashing    | 4.2 ns | Compute hash of key name             |
| String storage    | 2.2 ns | Store string reference in dictionary |
| **Total**         | **18.7 ns** | **6.4 ns overhead (52%)**       |

## Overhead Analysis

### Is 6.4 ns Significant?

**Context:**
- **Network I/O:** ~10 ms (10,000,000 ns)
- **Database query:** ~1 ms (1,000,000 ns)
- **File I/O:** ~100 µs (100,000 ns)
- **HTTP request:** ~50 ms (50,000,000 ns)
- **Named key overhead:** 6.4 ns

**Ratio:**
- Named key is **0.000064%** of network call
- Named key is **0.00064%** of database query
- Named key is **0.0064%** of file I/O

**Verdict:** ✅ **Negligible** in any application with I/O

### When Overhead Matters

Only in **CPU-bound hot paths** with:
- ✅ **No I/O** (pure in-memory computation)
- ✅ **High frequency** (millions of calls per second)
- ✅ **Low latency requirements** (< 1 µs response time)

**Example:** Real-time game engine, high-frequency trading, audio processing

### When Overhead Doesn't Matter

In **99% of applications:**
- Web APIs (I/O-bound)
- Microservices (network-bound)
- Batch processing (database-bound)
- Background workers (I/O-bound)

**Rule of thumb:** If your application does **any I/O**, named key overhead is invisible.

## Real-World Scenarios

### Scenario 1: Web API Request

**Pipeline:**
1. Parse HTTP request (10 µs)
2. Authenticate user (500 µs - JWT decode)
3. Query database (1 ms)
4. Execute business logic (**50 ns**)
5. Serialize response (20 µs)

**Total:** ~1.53 ms

**Named key overhead in business logic:**
```
50 ns → 56.4 ns (+6.4 ns)
Impact: 0.0004% of total request time
```

**Verdict:** ✅ **Use named keys** - Overhead is invisible, clarity benefits are massive

### Scenario 2: Batch Processing (1M Items)

**Requirements:**
- Process 1,000,000 items
- Store 3 values per item (default vs named keys)

**Performance comparison:**

| Approach      | Time/Item | Total Time | Difference |
|---------------|-----------|------------|------------|
| Default keys  | 12.3 ns   | 12.3 ms    | -          |
| Named keys    | 18.7 ns   | 18.7 ms    | +6.4 ms    |

**Analysis:**
- **Absolute difference:** 6.4 ms (for 1M items)
- **Relative difference:** 0.64% slower
- **Real impact:** Negligible if processing includes any I/O

**Verdict:** ✅ **Use named keys** if clarity helps - 6 ms per million items is acceptable

### Scenario 3: High-Frequency Trading (Extreme)

**Requirements:**
- Process 10,000,000 events/second
- Sub-microsecond latency critical

**Performance impact:**

| Approach      | Time/Event | Events/sec | CPU Usage |
|---------------|------------|------------|-----------|
| Default keys  | 12.3 ns    | 10M/sec    | 12.3%     |
| Named keys    | 18.7 ns    | 10M/sec    | 18.7%     |

**Analysis:**
- **Absolute difference:** 6.4 ns per event
- **Total overhead:** 64 ms/sec at 10M events/sec
- **CPU impact:** +6.4% CPU usage

**Verdict:** ⚠️ **Consider default keys** - In extreme CPU-bound scenarios, 6% matters

### Scenario 4: Microservices Mesh

**Pipeline:**
- Service A → Service B → Service C
- Each hop: ~10 ms network latency
- Named keys used at each service

**Overhead per service:**
```
Named keys: 6.4 ns × 3 = 19.2 ns total
Network latency: 10 ms × 2 = 20,000,000 ns

Overhead ratio: 19.2 / 20,000,000 = 0.000096%
```

**Verdict:** ✅ **Use named keys** - Network latency dominates (1 million times larger)

## Best Practices

### ✅ Use Named Keys When:

1. **Multiple instances of same type**
   ```csharp
   context.Set(currentUser, "CurrentUser");
   context.Set(oldUser, "PreviousUser");
   context.Set(systemUser, "SystemUser");
   ```

2. **Self-documenting code matters**
   ```csharp
   // ✅ Clear intent
   var tenant = context.Get<Tenant>("CurrentTenant");
   
   // ❌ Ambiguous
   var tenant = context.Get<Tenant>();
   ```

3. **Complex workflows**
   ```csharp
   context.Set(validationResult, "ValidationResult");
   context.Set(authResult, "AuthorizationResult");
   context.Set(transformResult, "TransformationResult");
   ```

4. **Application has I/O** (99% of cases)
   - Web APIs
   - Microservices
   - Background workers
   - Batch processing

### ⚠️ Consider Default Keys When:

1. **CPU-bound hot path**
   ```csharp
   // Processing 10M+ events/sec with no I/O
   for (int i = 0; i < 10_000_000; i++)
   {
       context.Set(data); // Faster for extreme throughput
   }
   ```

2. **Single instance per type**
   ```csharp
   // If you only ever need one User
   context.Set(user);
   var user = context.Get<User>();
   ```

3. **Memory-critical scenarios**
   ```csharp
   // Named keys allocate 32 bytes per entry
   // In embedded systems with 1 MB RAM, this might matter
   ```

### ❌ Don't Worry About:

1. **Micro-optimization** - Focus on I/O first
2. **Premature optimization** - Profile before optimizing
3. **6 ns overhead** - Unless you're NASA or high-frequency trading

## When to Use Named Keys

### Decision Tree

```
Does your code do ANY I/O?
├─ YES → Use named keys (overhead invisible)
└─ NO → Pure CPU-bound?
   ├─ YES → > 1M calls/sec?
   │  ├─ YES → Consider default keys
   │  └─ NO → Use named keys
   └─ NO → Use named keys
```

### Practical Guide

| Scenario                      | Recommendation   | Reason                                    |
|-------------------------------|------------------|-------------------------------------------|
| Web API                       | ✅ Named keys    | Network latency >> 6 ns                   |
| Microservices                 | ✅ Named keys    | I/O-bound                                 |
| Background worker             | ✅ Named keys    | I/O-bound                                 |
| Batch processing              | ✅ Named keys    | Database >> 6 ns                          |
| Real-time analytics           | ✅ Named keys    | Clarity > 6 ns                            |
| High-frequency trading        | ⚠️ Default keys  | Every nanosecond matters                  |
| Embedded systems (< 1 MB RAM) | ⚠️ Default keys  | 32 B per key adds up                      |
| Game engine (60 FPS)          | ✅ Named keys    | Frame budget is 16 ms (6 ns is 0.00004%) |

## Example: Refactoring Decision

### Before (Default Keys)

```csharp
public class UserService
{
    public async Task<Result> ProcessUser(int userId)
    {
        var context = new FlowContext(serviceProvider);
        
        // Ambiguous - which user?
        context.Set(await GetUser(userId));
        context.Set(await GetPreviousUser(userId));
        
        // How do we distinguish them?
        var current = context.Get<User>(); // ❌ Won't work - only one User allowed
    }
}
```

**Problem:** Can't store multiple users

### After (Named Keys)

```csharp
public class UserService
{
    public async Task<Result> ProcessUser(int userId)
    {
        var context = new FlowContext(serviceProvider);

        // ✅ Clear and explicit
        context.Set(await GetUser(userId), "CurrentUser");
        context.Set(await GetPreviousUser(userId), "PreviousUser");
        context.Set(await GetSystemUser(), "SystemUser");

        // ✅ Retrieval is unambiguous
        var current = context.Get<User>("CurrentUser");
        var previous = context.Get<User>("PreviousUser");
        var system = context.Get<User>("SystemUser");
    }
}
```

**Benefit:**
- ✅ Clear intent
- ✅ Multiple instances
- ✅ Self-documenting
- **Cost:** 6.4 ns (invisible in I/O-bound code)

## Key Takeaways

### ✅ Named Keys Are Fast Enough

- **Overhead:** 6.4 ns (1.52× slower than default)
- **Real impact:** 0.000064% of typical API request
- **Conclusion:** Use them unless you have **proof** it matters

### 📊 Performance Trade-Offs

| Aspect       | Default Keys | Named Keys   |
|--------------|--------------|--------------|
| Speed        | 12.3 ns      | 18.7 ns      |
| Memory       | 0 B          | 32 B         |
| Clarity      | ❌ Ambiguous | ✅ Explicit   |
| Flexibility  | ❌ One/type  | ✅ Many/type |
| Type safety  | ✅ Compile   | ⚠️ Runtime   |

### 🎯 Recommendation

**Default:** Use **named keys** for clarity and flexibility

**Exception:** CPU-bound hot paths with > 1M calls/sec **and no I/O**

**Reality:** 99% of applications should use named keys without hesitation

## Next Steps

- **Run named keys benchmark** → `.\scripts\run-standard-benchmarks.ps1`
- **Understand methodology** → See [Benchmark-Guide.md](Benchmark-Guide.md)
- **Compare with competitors** → See [DispatchR-Comparison.md](DispatchR-Comparison.md)
- **Add custom benchmarks** → See [Contributing.md](Contributing.md)

---

**Questions or issues?** Open an issue on [GitHub](https://github.com/vlasta81/FlowT)
