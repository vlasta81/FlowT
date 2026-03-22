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
            Context.GetFlowIdString(), action);

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
- Subsequent calls: returns the **same cached instance** (zero allocation)
- If `T` inherits `FlowPlugin`: automatically calls `Initialize(this)` to bind `Context`
- If `T` is not registered: throws `InvalidOperationException`

**Performance (BenchmarkDotNet results):**

| Scenario | Mean | Alloc |
|----------|------|-------|
| Cold (first call, DI resolution) | 204 ns | 464 B |
| Warm (cached lookup) | 23 ns | 0 B |
| 3 plugin types, all warm | 68.6 ns | 0 B |

> **8.7× speedup** after the first call. All repeated accesses cost only a locked dictionary lookup.

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
        _client.Track(Context.GetFlowIdString(), name, value);
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

`FlowContext` is **per-request** — it is not shared across concurrent requests. The plugin cache uses a lock only to prevent race conditions if the same `FlowContext` is somehow accessed concurrently (which should not happen in normal use).

---

## 🧪 Tests

Plugin system is covered by **21 tests** in [`PluginTests.cs`](../tests/FlowT.Tests/PluginTests.cs):

- **AddFlowPlugin registration** (4) — DI registration, Transient lifetime, TryAdd semantics
- **`Plugin<T>()` resolution & caching** (5) — return type, same instance, context isolation, error on missing type
- **`FlowPlugin` context binding** (5) — Context injected, correct instance, state read/write via Context
- **`FlowPlugin` accessibility** (4) — `Initialize` is `internal`, `Context` is `protected`
- **Pipeline integration** (3) — shared instance across policy+handler, isolation between executions

---

## 📚 Related Documentation

- **[FlowContext Guide](FLOWCONTEXT.md)** — `Plugin<T>()` in the full API reference
- **[Plugin Benchmarks](../benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.PluginBenchmarks.md)** — Cold/warm measurements
- **[API Reference: FlowPlugin](api/FlowPlugin.md)** — Auto-generated API docs
- **[API Reference: AddFlowPlugin](api/FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md)** — Extension method docs
