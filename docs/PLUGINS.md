# Plugin System Guide

## 📋 What is the Plugin System?

The **FlowT Plugin System** provides a way to attach **PerFlow services** to a `FlowContext` — components that are created once per flow execution and shared across all pipeline stages (specifications, policies, handler).

Plugins are resolved lazily via `context.Plugin<T>()` and cached for the lifetime of that flow execution. This makes them ideal for cross-cutting services that need access to the current `FlowContext` or that accumulate state across pipeline stages.

---

## 🎯 When to Use Plugins

| Use Case | Plugin | Context.Service<T>() |
|----------|--------|-----------------------|
| Shared state across pipeline stages | ✅ Ideal | ❌ Each resolve = new transient |
| Needs access to `FlowContext` | ✅ Via `FlowPlugin` base | ❌ Not available |
| Cross-cutting audit / tracing | ✅ Accumulates per-flow | ❌ Stateless |
| Simple one-shot service | ❌ Overkill | ✅ Simpler |
| Scoped service (DbContext) | ❌ | ✅ Use `ctx.Service<T>()` |

---

## 🚀 Quick Start

### 1. Define a Plugin Interface

```csharp
public interface IAuditPlugin
{
    void Record(string action);
    IReadOnlyList<AuditEntry> Entries { get; }
}
```

### 2. Implement the Plugin

```csharp
// Option A: Plain implementation (no FlowContext access needed)
public class AuditPlugin : IAuditPlugin
{
    private readonly List<AuditEntry> _entries = new();
    public IReadOnlyList<AuditEntry> Entries => _entries;

    public void Record(string action)
        => _entries.Add(new AuditEntry(action, DateTime.UtcNow));
}

// Option B: Inherit FlowPlugin to access FlowContext
public class AuditPlugin : FlowPlugin, IAuditPlugin
{
    private readonly ILogger<AuditPlugin> _logger;
    private readonly List<AuditEntry> _entries = new();
    public IReadOnlyList<AuditEntry> Entries => _entries;

    public AuditPlugin(ILogger<AuditPlugin> logger) => _logger = logger;

    public void Record(string action)
    {
        _entries.Add(new AuditEntry(action, DateTime.UtcNow));

        // ✅ Context is available here — injected by the framework
        _logger.LogInformation("[{FlowId}] Audit: {Action}",
            Context.FlowIdString, action);

        // ✅ Write to flow state
        Context.Set(_entries.Count, key: "auditCount");
    }
}
```

### 3. Register the Plugin

```csharp
// In your module's Register() method
public void Register(IServiceCollection services)
{
    services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();
}

// Or directly in Program.cs
builder.Services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();
```

### 4. Use in the Pipeline

```csharp
// In a policy
public class AuditPolicy : FlowPolicy<CreateOrderRequest, CreateOrderResponse>
{
    public override async ValueTask<CreateOrderResponse> HandleAsync(
        CreateOrderRequest request, FlowContext context)
    {
        var audit = context.Plugin<IAuditPlugin>();
        audit.Record("OrderCreation.Started");

        var response = await Next!.HandleAsync(request, context);

        audit.Record("OrderCreation.Completed");
        return response;
    }
}

// In the handler — same plugin instance!
public class CreateOrderHandler : IFlowHandler<CreateOrderRequest, CreateOrderResponse>
{
    public async ValueTask<CreateOrderResponse> HandleAsync(
        CreateOrderRequest request, FlowContext context)
    {
        var audit = context.Plugin<IAuditPlugin>();
        audit.Record("OrderPersisted");

        // audit.Entries contains all entries from AuditPolicy too
        // They share the same plugin instance within this flow execution

        return new CreateOrderResponse(orderId);
    }
}
```

---

## 🔑 `context.Plugin<T>()` — API Reference

```csharp
public T Plugin<T>() where T : notnull
```

**Behaviour:**
- First call: resolves `T` from DI, caches in the context, returns the instance
- Subsequent calls: returns the **same cached instance** (zero allocation, lockless fast path)
- If `T` inherits `FlowPlugin`: automatically calls `Initialize(this)` to bind `Context`
- If `T` is not registered: throws `InvalidOperationException`

**Performance (BenchmarkDotNet results):**

| Scenario | Mean | Alloc |
|----------|------|-------|
| Cold (first call, DI resolution) | 204 ns | 464 B |
| Warm (cached lookup) | 23 ns | 0 B |
| 3 plugin types, all warm | 68.6 ns | 0 B |

> **8.7× speedup** after the first call. All repeated accesses use a lockless fast path with zero allocation.

📖 **[Full Benchmark Results →](../benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.PluginBenchmarks.md)**

---

## 🔌 `FlowPlugin` — Abstract Base Class

`FlowPlugin` is an optional abstract base class for plugins that need access to the current `FlowContext`.

```csharp
public abstract class FlowPlugin
{
    // ✅ Available after Plugin<T>() resolves this instance
    protected FlowContext Context { get; private set; }

    // ✅ Called once by the framework immediately after DI resolution
    internal void Initialize(FlowContext context) => Context = context;
}
```

**Rules:**
- `Context` is `protected` — only accessible inside the plugin class itself
- `Initialize` is `internal` — called only by the framework, never by user code
- DI constructor injection works normally alongside `FlowPlugin`

### Example — Metric Collection Plugin

```csharp
public interface IMetricPlugin
{
    void Record(string name, double value);
}

public class MetricPlugin : FlowPlugin, IMetricPlugin
{
    private readonly IMetricsClient _client;
    private readonly Dictionary<string, double> _metrics = new();

    public MetricPlugin(IMetricsClient client) => _client = client;

    public void Record(string name, double value)
    {
        _metrics[name] = value;

        // Use FlowId for distributed tracing correlation
        _client.Track(Context.FlowIdString, name, value);
    }
}
```

### Example — Per-Flow Counter

```csharp
public interface ICounterPlugin
{
    void Increment(string key);
    int Get(string key);
}

public class CounterPlugin : FlowPlugin, ICounterPlugin
{
    private readonly Dictionary<string, int> _counters = new();

    public void Increment(string key)
    {
        _counters.TryGetValue(key, out var current);
        _counters[key] = current + 1;

        // Expose total count in FlowContext for downstream access
        Context.Set(_counters.Values.Sum(), key: "totalCount");
    }

    public int Get(string key)
    {
        _counters.TryGetValue(key, out var count);
        return count;
    }
}
```

---

## 📐 PerFlow Lifetime

Plugins have **PerFlow lifetime** — a single instance per `FlowContext` execution:

```
FlowContext (one per request)
    └── Plugin cache (Dictionary<Type, object>)
            ├── IAuditPlugin  → AuditPlugin instance A
            ├── IMetricPlugin → MetricPlugin instance B
            └── ICounterPlugin → CounterPlugin instance C
```

| Property | Value |
|----------|-------|
| **Created** | On first `context.Plugin<T>()` call |
| **Lifetime** | Duration of the flow execution |
| **Shared** | Across all stages: specs → policies → handler |
| **Isolated** | Between different flow executions / FlowContext instances |
| **DI registration** | Transient (new instance per DI resolution = per FlowContext) |

---

## ⚠️ Important Notes

### Registration Uses `TryAdd` (Idempotent)

```csharp
// ✅ Safe to call multiple times — only first registration is kept
services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();
services.AddFlowPlugin<IAuditPlugin, AlternativePlugin>(); // Ignored
```

### Plugins Are Not Scoped Services

Do not confuse plugins with scoped DI services:

```csharp
// ✅ Plugin — PerFlow, cached in FlowContext, can hold mutable state
var audit = context.Plugin<IAuditPlugin>();

// ✅ Scoped service — resolved from DI scope per-request
var db = context.Service<AppDbContext>();
```

### Thread Safety

`FlowContext` is **per-request** — it is not shared across concurrent requests. The plugin resolution uses a lockless fast path for repeated lookups, with a lock only on the initial write, making warm-path access thread-safe and allocation-free.

---

## 🧪 Tests

Plugin system is covered by **30+ tests** in [`PluginTests.cs`](../tests/FlowT.Tests/PluginTests.cs):

- **AddFlowPlugin registration** (4) — DI registration, Transient lifetime, TryAdd semantics
- **`Plugin<T>()` resolution & caching** (5) — return type, same instance, context isolation, error on missing type
- **`FlowPlugin` context binding** (5) — Context injected, correct instance, state read/write via Context
- **`FlowPlugin` accessibility** (4) — `Initialize` is `internal`, `Context` is `protected`
- **Pipeline integration** (3) — shared instance across policy+handler, isolation between executions

---

## 📦 Built-in Plugins (`FlowT.Plugins`)

FlowT ships **9 ready-to-use plugins** for common cross-cutting concerns. Register only the ones you need:

| Plugin | Interface | Purpose |
|--------|-----------|--------|
| **Correlation** | `ICorrelationPlugin` | Stable correlation ID from header or FlowId |
| **RetryState** | `IRetryStatePlugin` | Thread-safe attempt counter for retry policies |
| **Transaction** | `ITransactionPlugin` | Coordinate DB transactions across pipeline stages |
| **Audit** | `IAuditPlugin` | Accumulate structured audit entries per flow |
| **FeatureFlag** | `IFeatureFlagPlugin` | Per-flow feature flag evaluation with result cache |
| **FlowScope** | `IFlowScopePlugin` | Dedicated DI scope for non-HTTP scenarios |
| **Idempotency** | `IIdempotencyPlugin` | Idempotency key from header (`Idempotency-Key`) |
| **Performance** | `IPerformancePlugin` | Named `Stopwatch`-based section timing |
| **Tenant** | `ITenantPlugin` | Tenant ID from claim / header / route / default |

### `IAuditPlugin` — Structured Audit Trail

Accumulates audit entries in memory for the duration of the flow. Flush to DB or a logging sink after the flow completes.

```csharp
services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();

// Usage
var audit = context.Plugin<IAuditPlugin>();
audit.Record("OrderCreated", new { orderId, userId });
audit.Record("PaymentCharged");

foreach (var entry in audit.Entries)
    logger.LogInformation("[{Ts}] {Action} {Data}", entry.Timestamp, entry.Action, entry.Data);
```

### `IFeatureFlagPlugin` — Feature Flags (Microsoft.FeatureManagement)

Evaluates feature flags once per flow and caches the result. Requires the `Microsoft.FeatureManagement.AspNetCore` NuGet package.

```csharp
// Package: dotnet add package Microsoft.FeatureManagement.AspNetCore
builder.Services.AddFeatureManagement();             // MS FeatureManagement setup
services.AddFlowPlugin<IFeatureFlagPlugin, FeatureFlagPlugin>();

// Usage
var flags = context.Plugin<IFeatureFlagPlugin>();
if (await flags.IsEnabledAsync("NewCheckout", ct))
    return await NewCheckoutFlow(request, context);
```

### `IFlowScopePlugin` — Dedicated DI Scope

Creates a dedicated `IServiceScope` for the flow. Useful in background jobs / hosted services where ASP.NET Core does not manage a per-request scope automatically.

```csharp
services.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>();

// Usage — dispose the scope when the flow is finished
var scopePlugin = context.Plugin<IFlowScopePlugin>();
var db = scopePlugin.ScopedServices.GetRequiredService<AppDbContext>();
// ... use db ...
scopePlugin.Dispose();
```

> ⚠️ The plugin is **not** disposed automatically — the host (hosted service, pipeline) must call `Dispose()` after the flow.

### `IIdempotencyPlugin` — Idempotency Key

Reads the `Idempotency-Key` HTTP request header and exposes it for idempotent request handling.

```csharp
services.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>();

// Usage in a specification
var idempotency = context.Plugin<IIdempotencyPlugin>();
if (idempotency.HasKey && await store.ExistsAsync(idempotency.Key!, ct))
    return FlowInterrupt<object?>.Stop(cachedResponse, 200);
```

### `IPerformancePlugin` — Named Section Timing

Measures elapsed time for named code sections using `Stopwatch`. Results are available after each `Dispose()`.

```csharp
services.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>();

// Usage — IDisposable scope pattern
var perf = context.Plugin<IPerformancePlugin>();
using (perf.Measure("db-query"))
    result = await db.Orders.FindAsync(id);

using (perf.Measure("mapping"))
    response = mapper.Map(result);

foreach (var (name, elapsed) in perf.Elapsed)
    logger.LogInformation("{Section}: {Ms} ms", name, elapsed.TotalMilliseconds);
```

### `ITenantPlugin` — Multi-Tenant ID Resolution

Resolves the tenant identifier with the following priority order: `tid` claim → `X-Tenant-Id` header → `tenantId` route value → `"default"` fallback.

```csharp
services.AddFlowPlugin<ITenantPlugin, TenantPlugin>();

// Usage
var tenantId = context.Plugin<ITenantPlugin>().TenantId;
var db = context.Service<ITenantDbFactory>().GetDatabase(tenantId);
```

---

## 📚 Related Documentation

- **[FlowContext Guide](FLOWCONTEXT.md)** — `Plugin<T>()` in the full API reference
- **[Plugin Benchmarks](../benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.PluginBenchmarks.md)** — Cold/warm measurements
- **[API Reference: FlowPlugin](api/FlowPlugin.md)** — Auto-generated API docs
- **[API Reference: AddFlowPlugin](api/FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md)** — Extension method docs
