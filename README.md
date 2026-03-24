# FlowT - High-Performance Orchestration Library for .NET

[![NuGet](https://img.shields.io/nuget/v/FlowT.svg)](https://www.nuget.org/packages/FlowT/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](https://github.com/vlasta81/FlowT)
[![Tests](https://img.shields.io/badge/Tests-206+%20Passing-brightgreen.svg)](tests/FlowT.Tests/)
[![Analyzers](https://img.shields.io/badge/Analyzers-26%20Rules-blue.svg)](src/FlowT.Analyzers/README.md)

**FlowT** is a high-performance orchestration library for .NET that implements the Chain of Responsibility pattern with a fluent API. Build maintainable, testable, and **ultra-fast** pipelines with specifications, policies, and handlers.

---

## ✨ Key Features

### 🚀 Performance & Efficiency
- ⚡ **2.7× faster than DispatchR** - The fastest MediatR alternative
- ⚡ **9× faster than MediatR** - Singleton architecture with cached pipelines
- 💾 **22% more memory than DispatchR** - Trade-off for 2.7× speed gain
- 💾 **84% less memory than MediatR** - Zero-allocation fast paths with ValueTask
- 🔥 **Thread-safe** - Lock-free operations with compile-time safety
- 📦 **Singleton handlers** - Reusable components with automatic dependency resolution

### 🎯 Developer Experience
- 🧩 **Modular Architecture** - `IFlowModule` for clean feature organization
- 🔄 **Chain of Responsibility** - Composable pipeline with specifications, policies, handlers
- 🎨 **Fluent API** - Intuitive and expressive pipeline configuration
- 📝 **Automatic Context Creation** - `ExecuteAsync(request, httpContext)` for web, `ExecuteAsync(request, services, ct)` for non-web - no manual FlowContext
- 🛡️ **26 Roslyn Analyzers** - Compile-time safety preventing threading issues

### 💡 Advanced Capabilities
- 🔍 **FlowInterrupt** - Type-safe error handling from specifications without exceptions
  - Validation failures with status codes (400, 401, 409, etc.)
  - Early returns for business logic shortcuts
  - No try-catch overhead, clean error flow
- 🔌 **Plugin System** - PerFlow services shared across all pipeline stages
  - `AddFlowPlugin<TPlugin, TImpl>()` — explicit, idempotent registration
  - `context.Plugin<T>()` — 23 ns warm path, zero allocations
  - `FlowPlugin` abstract base for automatic `FlowContext` binding
- 📊 **Named Keys** - Store multiple values of same type with optional string keys
- 🎭 **Scoped Services** - Safe `ctx.Service<DbContext>()` usage in singleton handlers
- 🧪 **100% Tested** - 206+ unit tests with full coverage
- 🏗️ **CQRS Ready** - Commands, Queries, Events with minimal boilerplate

---

## 📊 Performance

**FlowT is the fastest .NET orchestration library** — optimized for high-throughput scenarios with singleton architecture and cached pipelines.

### Key Metrics

| Metric | Result |
|--------|--------|
| ⚡ **Speed vs DispatchR** | **1.6-2.8× faster** |
| ⚡ **Speed vs MediatR** | **9× faster** |
| 💾 **Memory vs MediatR** | **84% less allocation** |
| 🧵 **Thread Safety** | Lock-free, compile-time verified |

### Performance Highlights

- 🚀 **Simple handler**: ~30 ns execution time
- 🔄 **Pipeline scaling**: Linear (10× components = 10× time)
- 📦 **Large payloads**: Zero overhead for 10 MB+ data
- 🔁 **Concurrent requests**: Perfect scaling, no contention

> 📊 **Detailed benchmarks**: [FlowT.Benchmarks/README.md](benchmarks/FlowT.Benchmarks/README.md)  
> 🔥 **Extreme load tests**: [FlowT.Benchmarks/docs/Extreme-Benchmarks.md](benchmarks/FlowT.Benchmarks/docs/Extreme-Benchmarks.md)  
> 📁 **Raw results**: [FlowT.Benchmarks/docs/results/](benchmarks/FlowT.Benchmarks/docs/results/)

### Quick Comparison

| Framework | Relative Speed | Memory Efficiency | Best For |
|-----------|---------------|-------------------|----------|
| **FlowT** 🥇 | **Fastest** | Excellent | High-performance orchestration |
| **DispatchR** 🥈 | 2.7× slower | **Best** | Memory-constrained scenarios |
| MediatR 🥉 | 9× slower | Baseline | Legacy compatibility |

> 💡 **Trade-off**: FlowT uses ~28% more memory than DispatchR for 2.7× speed gain + advanced features (analyzers, modules, type-safe interrupts).

> 📖 **Complete framework comparison**: [DispatchR Deep Dive](benchmarks/FlowT.Benchmarks/docs/DispatchR-Comparison.md)

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package FlowT
```

### Basic Example

```csharp
// 1. Define request/response
public record CreateUserRequest(string Email, string Name);
public record CreateUserResponse(Guid Id, string Email, string? Error = null);

// 2. Create handler
public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(ILogger<CreateUserHandler> logger) => _logger = logger;

    public async ValueTask<CreateUserResponse> HandleAsync(
        CreateUserRequest request, FlowContext context)
    {
        // ✅ Resolve scoped services per-request
        var db = context.Service<AppDbContext>();

        var user = new User { Email = request.Email, Name = request.Name };
        db.Users.Add(user);
        await db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("User {Id} created", user.Id);

        return new CreateUserResponse(user.Id, user.Email);
    }
}

// 3. Define flow with pipeline (mark with [FlowDefinition] attribute)
[FlowDefinition]  // ✅ Required for auto-discovery
public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
    {
        flow
            .Check<ValidateEmailSpecification>()  // ✅ Validation
            .Use<LoggingPolicy>()                 // ✅ Logging
            .Use<TransactionPolicy>()             // ✅ Transaction
            .OnInterrupt(interrupt =>             // ✅ Map Fail() to typed error response
                new CreateUserResponse(Guid.Empty, "", interrupt.Message))
            .Handle<CreateUserHandler>();         // ✅ Business logic
    }
}

// 4. Register using modules (mark with [FlowModule] attribute)
[FlowModule]  // ✅ Required for auto-discovery
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        // ✅ Explicit per-flow registration (recommended)
        services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();

        // Register external dependencies (handlers/specs/policies are auto-created)
        services.AddScoped<IUserRepository, UserRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", async (
            CreateUserRequest request,
            CreateUserFlow flow,                    // ✅ Injected directly!
            HttpContext httpContext) =>
        {
            // ✅ FlowContext created automatically from HttpContext!
            return await flow.ExecuteAsync(request, httpContext);
        });
    }
}

// 5. In Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFlowModules(typeof(Program).Assembly);

var app = builder.Build();
app.MapFlowModules(); // ✅ Auto-map all module endpoints
app.Run();
```

---

## 📚 Core Concepts

### 🧩 Modules (`IFlowModule`)
Organize features into cohesive modules with automatic discovery (requires `[FlowModule]` attribute):

```csharp
[FlowModule]  // ✅ Required for AddFlowModules() auto-discovery
public class OrderModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        // ✅ Explicit per-flow registration
        services.AddFlow<CreateOrderFlow, CreateOrderRequest, CreateOrderResponse>();
        services.AddFlow<GetOrderFlow, GetOrderRequest, GetOrderResponse>();
        services.AddFlow<UpdateOrderFlow, UpdateOrderRequest, UpdateOrderResponse>();

        // Register external dependencies
        services.AddScoped<IOrderRepository, OrderRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", /* ... */);
        app.MapGet("/api/orders/{id}", /* ... */);
    }
}
```

**Note:** The `[FlowModule]` attribute is **required** for automatic registration. It provides:
- ✅ **Vertical slice architecture** - Each module is a self-contained feature
- ✅ **Clear boundaries** - Explicit marking prevents accidental module registration
- ✅ **Convention over configuration** - `AddFlowModules()` scans and calls `Register()` automatically
- ✅ **No duplicate registrations** - Each flow is explicitly registered once per module

### 🔄 Flows (`FlowDefinition<TRequest, TResponse>`)
Define reusable orchestration pipelines (requires `[FlowDefinition]` attribute):

```csharp
[FlowDefinition]  // ✅ Required for compile-time validation
public class ProcessOrderFlow : FlowDefinition<OrderRequest, OrderResponse>
{
    protected override void Configure(IFlowBuilder<OrderRequest, OrderResponse> flow)
    {
        flow
            .Check<ValidateOrderSpec>()      // Validation
            .Check<InventoryCheckSpec>()     // Business rule
            .Use<CachingPolicy>()            // Cross-cutting concern
            .Use<RetryPolicy>()              // Resilience
            .OnInterrupt(interrupt =>        // Required: map Fail() to typed response
                new OrderResponse(interrupt.Message, interrupt.StatusCode))
            .Handle<ProcessOrderHandler>();  // Core logic
    }
}
```

**Registration:**
```csharp
// In module
services.AddFlow<ProcessOrderFlow, OrderRequest, OrderResponse>();

// Or standalone in Program.cs
builder.Services.AddFlow<ProcessOrderFlow, OrderRequest, OrderResponse>();
```

**Note:** The `[FlowDefinition]` attribute is **required** for registration. It provides:
- ✅ **Explicit opt-in** - Prevents accidental registration of base classes or test fixtures
- ✅ **Clear intent** - Makes it obvious which flows are part of the public API
- ✅ **Better tooling** - IDEs can find all flows by searching for the attribute
- ✅ **Compile-time validation** - Ensures flow is properly decorated before registration

### 🎭 Context (`FlowContext`)
Per-request execution context with scoped service resolution:

```csharp
public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
{
    // ✅ Scoped services (DbContext, HttpContext, etc.)
    var db = ctx.Service<AppDbContext>();
    var user = ctx.Service<IHttpContextAccessor>().HttpContext.User;

    // ✅ Per-request storage
    ctx.Set(user.FindFirst("sub")?.Value, key: "userId");
    ctx.Set(DateTime.UtcNow, key: "requestTime");

    // ✅ Named keys for multiple values of same type
    ctx.Set(cacheData, key: "primary-cache");
    ctx.Set(fallbackData, key: "secondary-cache");

    // ✅ Cancellation support
    await db.SaveChangesAsync(ctx.CancellationToken);

    // ✅ Events
    ctx.PublishInBackground(new OrderCreatedEvent(orderId));
}
```

**FlowContext provides:**
- 🔌 **Dependency Injection** - `Service<T>()`, `TryService<T>()`
- 🧩 **Plugin Resolution** - `Plugin<T>()` — PerFlow cached instances
- 💾 **Shared State** - `Set()`, `TryGet()`, `GetOrAdd()`, `Push()`
- 🚫 **Cancellation** - `CancellationToken` for timeout/client disconnect
- 🌐 **HTTP Context** - Access to request/response (nullable)
- 📢 **Events** - `PublishAsync()`, `PublishInBackground()`
- ⏱️ **Timing** - `StartTimer()` returns `TimerDisposable` — measure via `using`
- 🆔 **Flow ID** - `FlowId`, `GetFlowIdString()` for correlation/logging

📚 **[Complete FlowContext Guide →](docs/FLOWCONTEXT.md)**

### 🔍 Specifications (`IFlowSpecification<TRequest>`)
Reusable validation and business rules that can interrupt flow execution:

```csharp
public class ValidateEmailSpecification : IFlowSpecification<CreateUserRequest>
{
    // ⚠️ Specs always return FlowInterrupt<object?> — the response type is erased at the interface level
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateUserRequest request, FlowContext context)
    {
        if (!IsValidEmail(request.Email))
        {
            // ✅ FlowInterrupt captures validation failure without exceptions
            // Returns 400 Bad Request with error message
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail(
                    "Email format is invalid",
                    StatusCodes.Status400BadRequest
                )
            );
        }

        // ✅ null = validation passed, continue pipeline
        return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
    }
}
```

**How FlowInterrupt works:**
- **`Fail(message, statusCode)`** - Validation/business rule failure (400, 401, 409, etc.)
- **`Stop(response, statusCode)`** - Early return with successful result (200, 201, 204)
- **`null`** - Validation passed, continue to next step
- **No exceptions** - Clean error flow without try-catch overhead
- **⚠️ `Fail()` requires `.OnInterrupt()`** — without it, failure silently returns `default!` (null), causing `NullReferenceException`

### 🔀 `.OnInterrupt()` — Mapping Specification Results to Typed Responses

Registers a mapper for `FlowInterrupt` values returned by **specifications only** (`.Check<>()`). Exceptions thrown by policies or the handler propagate normally — this mapper does **not** catch them.

> ⚠️ **Without `.OnInterrupt()`, a `Fail()` interrupt silently returns `default!` (null for reference types)** — this causes `NullReferenceException` when the endpoint accesses the result.

The mapper receives a `FlowInterrupt<object?>` with `.Message`, `.StatusCode`, `.IsFailure`, and `.IsEarlyReturn` properties and must return a `TResponse`.

**Pattern 1 — Map to response (when response type can carry error info):**
```csharp
[FlowDefinition]
public class CreateOrderFlow : FlowDefinition<CreateOrderRequest, CreateOrderResponse>
{
    protected override void Configure(IFlowBuilder<CreateOrderRequest, CreateOrderResponse> flow)
    {
        flow
            .Check<ValidateUserSpec>()
            .Check<ValidateItemsSpec>()
            .OnInterrupt(interrupt =>                         // ✅ Spec Fail()/Stop() → typed response
                new CreateOrderResponse(
                    Guid.Empty, 0m, Status.Failed,
                    interrupt.Message ?? "Request failed"))
            .Handle<CreateOrderHandler>();
    }
}
```

**Pattern 2 — Throw (when response is a clean DTO with no error fields):**
```csharp
[FlowDefinition]
public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
    {
        flow
            .Check<ValidateEmailSpec>()
            .OnInterrupt(interrupt =>                         // ✅ Throw — caught by exception-handler middleware
                throw new ValidationException(
                    interrupt.Message ?? "Request failed"))  // use your preferred exception type
            .Handle<CreateUserHandler>();
    }
}
```

**Rules:**
- ✅ **Specifications only** — captures `FlowInterrupt` from `.Check<>()` calls; policy/handler exceptions propagate normally
- ✅ **Required for `Fail()`** — `Stop()` works without it (casts `Response` to `TResponse` directly)
- ✅ **Call exactly once** — throws `InvalidOperationException` if called more than once
- ✅ **May throw** — the mapper can throw instead of returning when the response type has no error fields

### 🛡️ Policies (`FlowPolicy<TRequest, TResponse>`)
Cross-cutting concerns as reusable middleware:

```csharp
public class TransactionPolicy<TRequest, TResponse> : FlowPolicy<TRequest, TResponse>
{
    public override async ValueTask<TResponse> HandleAsync(
        TRequest request, FlowContext context)
    {
        var db = context.Service<AppDbContext>();

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var response = await Next!.HandleAsync(request, context);
            await transaction.CommitAsync();
            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### 🔌 Plugin System (`Plugin<T>()`)
PerFlow services that are created once per flow execution and shared across all pipeline stages:

```csharp
// 1. Define and implement a plugin
public interface IAuditPlugin
{
    void Record(string action);
}

public class AuditPlugin : FlowPlugin, IAuditPlugin  // FlowPlugin → gets Context injected
{
    private readonly ILogger<AuditPlugin> _logger;
    public AuditPlugin(ILogger<AuditPlugin> logger) => _logger = logger;

    public void Record(string action)
    {
        // ✅ FlowContext is automatically bound — no constructor injection needed
        _logger.LogInformation("[{FlowId}] {Action}", Context.GetFlowIdString(), action);
    }
}

// 2. Register
services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();

// 3. Use in any pipeline stage — same instance is shared
public class AuditPolicy : FlowPolicy<Request, Response>
{
    public override async ValueTask<Response> HandleAsync(Request request, FlowContext context)
    {
        context.Plugin<IAuditPlugin>().Record("Started");        // cold: 204 ns
        var response = await Next!.HandleAsync(request, context);
        context.Plugin<IAuditPlugin>().Record("Completed");      // warm: 23 ns / 0 B
        return response;
    }
}
```

**Key properties:**
- ✅ **PerFlow lifetime** — one instance per `FlowContext`, shared across specs → policies → handler
- ✅ **8.7× warm-path speedup** — 204 ns cold → 23 ns warm, zero allocations
- ✅ **`FlowPlugin` base** — automatic `Context` binding without exposing it publicly
- ✅ **Idempotent registration** — `AddFlowPlugin` uses `TryAdd`, safe to call multiple times

📚 **[Complete Plugin System Guide →](docs/PLUGINS.md)**

### 🧩 Built-in Plugins (`FlowT.Plugins`)
FlowT ships four ready-to-use plugins for common cross-cutting concerns. Register only the ones you need:

| Plugin | Interface | Implementation | Purpose |
|--------|-----------|----------------|---------|
| User Identity | `IUserIdentityPlugin` | `UserIdentityPlugin` | Parse `ClaimsPrincipal` once per flow |
| Correlation | `ICorrelationPlugin` | `CorrelationPlugin` | Stable correlation ID from header or FlowId |
| Retry State | `IRetryStatePlugin` | `RetryStatePlugin` | Thread-safe attempt counter for retry policies |
| Transaction | `ITransactionPlugin` | `FlowTransactionPlugin` (abstract) | Coordinate DB transactions across pipeline stages |

```csharp
// Register in Program.cs or module
services.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>();
services.AddFlowPlugin<ICorrelationPlugin,  CorrelationPlugin>();
services.AddFlowPlugin<IRetryStatePlugin,   RetryStatePlugin>();
services.AddFlowPlugin<ITransactionPlugin,  MyEfCoreTransactionPlugin>(); // your concrete impl
```

**`IUserIdentityPlugin`** — claims parsed once, cached for the flow:
```csharp
var identity = context.Plugin<IUserIdentityPlugin>();
if (!identity.IsAuthenticated)
    return FlowInterrupt<object?>.Fail("Unauthorized", 401);

var userId  = identity.UserId;           // Guid?   from NameIdentifier claim
var email   = identity.Email;            // string? from Email claim
var isAdmin = identity.IsInRole("Admin");
```

**`ICorrelationPlugin`** — reads `X-Correlation-Id` header, falls back to `FlowId`:
```csharp
var id = context.Plugin<ICorrelationPlugin>().CorrelationId;
logger.LogInformation("[{CorrelationId}] Processing", id);
```

**`IRetryStatePlugin`** — shared counter across policy → handler:
```csharp
var retry = context.Plugin<IRetryStatePlugin>();
while (retry.ShouldRetry(maxAttempts: 3))
{
    retry.RegisterAttempt();
    try   { return await Next!.HandleAsync(request, context); }
    catch { if (!retry.ShouldRetry(3)) throw; }
}
```

**`ITransactionPlugin`** — implement `FlowTransactionPlugin` for your DB provider:
```csharp
public class EfCoreTransactionPlugin : FlowTransactionPlugin
{
    private IDbContextTransaction? _tx;

    public override async ValueTask BeginAsync(CancellationToken ct = default)
    {
        _tx = await Context.Service<AppDbContext>().Database.BeginTransactionAsync(ct);
        IsActive = true;
    }

    public override async ValueTask CommitAsync(CancellationToken ct = default)
    {
        await _tx!.CommitAsync(ct);
        IsActive = false;
    }

    public override async ValueTask RollbackAsync(CancellationToken ct = default)
    {
        await _tx!.RollbackAsync(ct);
        IsActive = false;
    }
}
```

---

## 🛡️ Compile-Time Safety (Roslyn Analyzers)

FlowT includes **26 Roslyn analyzers** that catch threading and safety issues at compile-time:

### 🔴 Errors (Build fails - must fix!)
- **FlowT002**: Non-thread-safe collections (List, Dictionary, etc.)
- **FlowT003**: Captive scoped dependencies (DbContext in constructor)
- **FlowT004**: Static mutable state
- **FlowT006**: FlowContext stored in field
- **FlowT007**: Request/Response objects in fields
- **FlowT010**: Thread.Sleep() in async methods
- **FlowT011**: Missing .Handle<T>() in FlowDefinition.Configure()
- **FlowT012**: IServiceProvider stored in field
- **FlowT013**: CancellationTokenSource stored in field
- **FlowT015**: Mutable public/internal properties
- **FlowT018**: Lazy<T> without thread-safety mode
- **FlowT019**: State leak types (StringBuilder, Stream, Stopwatch, arrays)
- **FlowT021**: FlowPlugin stored in singleton field
- **FlowT022**: Multiple .Handle<T>() calls in Configure()

### ⚠️ Warnings
- **FlowT001**: Mutable instance fields
- **FlowT005**: Async void methods
- **FlowT008**: Lock on `this` or `typeof(T)`
- **FlowT010**: Synchronous blocking (.Result, .Wait())
- **FlowT016**: Task/ValueTask storage
- **FlowT017**: Manual Thread creation
- **FlowT020**: ConfigureAwait(false) loses HttpContext/FlowContext
- **FlowT023**: new HttpClient() in flow component (socket exhaustion)
- **FlowT024**: Synchronous file I/O in async flow method

### ℹ️ Info (Suggestions)
- **FlowT009**: Missing CancellationToken propagation
- **FlowT014**: Empty catch blocks
- **FlowT025**: Direct IServiceProvider access (prefer context.Service<T>())

> 📖 Full analyzer documentation: [src/FlowT.Analyzers/README.md](src/FlowT.Analyzers/README.md)

**Example:**
```csharp
public class BadHandler : IFlowHandler<Request, Response>
{
    private readonly AppDbContext _db; // ❌ FlowT003: Build fails!

    public BadHandler(AppDbContext db) { _db = db; }
}

// Build error:
// error FlowT003: Flow component 'BadHandler' captures scoped service 'AppDbContext' 
// in constructor. Use 'context.Service<AppDbContext>()' instead
```

---

## 📖 Documentation

### 📘 Getting Started
- **[Quick Start](#-quick-start)** - 5-minute installation and basic example
- **[Core Concepts](#-core-concepts)** - Modules, Flows, Context, Specifications, Policies
- **[Migration Guide: AddFlows() → AddFlow<>()](docs/MIGRATION_AddFlows.md)** - ⚠️ Migrate from deprecated AddFlows()

### 📚 API Reference
- **[API Documentation](docs/api/index.md)** - Complete API reference (auto-generated from XML docs)
- **[FlowContext Complete Guide](docs/FLOWCONTEXT.md)** - ⭐ Full API reference, patterns & named keys
- **[Plugin System Guide](docs/PLUGINS.md)** - PerFlow plugins, `FlowPlugin` base class, `AddFlowPlugin`
- **[FlowT.links](docs/FlowT.links)** - Direct links to all API types and members

### 📊 Performance & Benchmarks
- **[Benchmark Suite](benchmarks/FlowT.Benchmarks/README.md)** - Comprehensive performance testing
  - **Core Benchmarks** - FlowContext, Pipeline, Allocations, Named Keys
  - **Framework Comparisons** - FlowT vs DispatchR/MediatR/Wolverine/Brighter/Mediator.Net
  - **Extreme Load Tests** - Stress tests with 10k items, 100 concurrent requests, deep nesting
  - **Streaming Benchmarks** - IAsyncEnumerable performance (25× overhead, 96% memory reduction)
  - **Plugin System Benchmarks** - PerFlow caching, cold/warm paths, pipeline overhead
- **[Benchmark Results](benchmarks/FlowT.Benchmarks/docs/results/)** - Detailed result breakdowns
  - [Streaming Guide](benchmarks/FlowT.Benchmarks/docs/Streaming-Benchmarks.md) - Complete streaming analysis
  - [Streaming Results](benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.StreamingBenchmarks.md)
  - [Streaming Comparison](benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.StreamingComparisonBenchmarks.md)
  - [Plugin Results](benchmarks/FlowT.Benchmarks/docs/results/FlowT.Benchmarks.PluginBenchmarks.md)
- **[Run Benchmarks](benchmarks/FlowT.Benchmarks/scripts/)** - PowerShell scripts for local testing

### 🛡️ Code Quality & Safety
- **[FlowT.Analyzers](src/FlowT.Analyzers/README.md)** - 26 Roslyn diagnostic rules
  - **14 Errors** - Build fails (thread-safety, DI anti-patterns, data leaks, flow configuration)
  - **9 Warnings** - Should fix (async issues, locking problems, HttpClient, file I/O)
  - **3 Info** - Suggestions (cancellation, empty catch blocks, service provider access)
- **[Thread Safety Guide](docs/BEST_PRACTICES.md#-architecture)** - Singleton safety patterns
- **Compile-time protection** - Catch bugs before production

### 🧪 Testing & Examples
- **[FlowT.Tests](tests/FlowT.Tests/)** - 206+ unit tests with full coverage
  - Flow definition tests
  - Pipeline execution tests
  - FlowContext operations
  - Module registration
  - Plugin system tests
  - Analyzer verification
- **[Sample Application](samples/FlowT.SampleApp/)** - Complete working example
  - 📖 **[Read the guide](samples/FlowT.SampleApp/README.md)** - Comprehensive documentation with examples
  - **Features:** User CRUD (5 flows), Product catalog (3 flows), Order processing (complex validation)
  - **Demonstrates:** Modules, FlowInterrupt, named keys, policies, specifications, best practices
  - **Includes:** Unit tests, Scalar UI documentation, curl examples

---

## 🎯 Why FlowT?

FlowT is designed for **high-performance orchestration** with compile-time safety and developer-friendly APIs.

### Key Differentiators

| Feature | Benefit |
|---------|--------|
| ⚡ **Singleton Architecture** | Cached pipelines, 2.7× faster than DispatchR |
| 🛡️ **26 Roslyn Analyzers** | Catch threading/DI issues at compile-time |
| 🧩 **IFlowModule System** | Clean vertical slice organization |
| 🔍 **FlowInterrupt** | Type-safe error handling without exceptions |
| 🔌 **Plugin System** | PerFlow services with 8.7× warm-path speedup |
| 💾 **Named Keys** | Store multiple values of same type in FlowContext |

### Framework Comparison

**Choose FlowT when you need:**
- Maximum performance for high-throughput scenarios
- Compile-time safety for thread-safe singleton components
- Clean modular architecture with auto-discovery

**Consider alternatives when:**
- Memory is the absolute priority → [DispatchR](https://github.com/samvasta/DispatchR) (28% less memory)
- You need legacy MediatR compatibility → [MediatR](https://github.com/jbogard/MediatR)
- You need full messaging infrastructure → [Brighter](https://github.com/BrighterCommand/Brighter)

> 📊 **Detailed comparisons**: [Benchmark Suite](benchmarks/FlowT.Benchmarks/README.md)  
> 🔍 **DispatchR deep dive**: [DispatchR-Comparison.md](benchmarks/FlowT.Benchmarks/docs/DispatchR-Comparison.md)

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    FlowModule                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Register   │  │ MapEndpoints │  │  Auto-scan   │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    FlowDefinition<TRequest, TResponse>      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Configure  │→ │    Pipeline  │→ │  Singleton   │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pipeline Execution                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │Specification │→ │    Policy    │→ │   Handler    │       │
│  │  (Validate)  │  │ (Middleware) │  │   (Logic)    │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    FlowContext                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Services   │  │    Storage   │  │    Events    │       │
│  │ (Per-request)│  │  (Named keys)│  │ (Background) │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Project Structure

```
FlowT/

├── CHANGELOG.md                        # Version history
├── src/
│   ├── FlowT/                          # Core library (.NET 10 + .NET Standard 2.0)
│   │   ├── Abstractions/               # Base classes (FlowDefinition, StreamableResponse, etc.)
│   │   ├── Contracts/                  # Interfaces (IFlow, IFlowHandler, IFlowPolicy, etc.)
│   │   ├── Extensions/                 # DI & endpoint mapping extensions
│   │   └── FlowContext.cs              # Per-request execution context
│   └── FlowT.Analyzers/                # Roslyn analyzers (26 diagnostic rules)
│       ├── README.md                   # Analyzer documentation
│       └── *Analyzer.cs                # Individual analyzer implementations
├── tests/
│   └── FlowT.Tests/                    # 206+ unit tests (xUnit)
│       ├── FlowDefinitionTests.cs      # Flow pipeline tests
│       ├── FlowContextTests.cs         # Context operations tests
│       ├── PluginTests.cs              # Plugin system tests
│       └── AnalyzerTests/              # Roslyn analyzer tests
├── benchmarks/
│   └── FlowT.Benchmarks/               # BenchmarkDotNet suite
│       ├── README.md                   # Benchmark guide
│       ├── scripts/                    # PowerShell scripts for running benchmarks
│       ├── docs/                       # Detailed analysis & comparisons
│       └── *Benchmarks.cs              # Individual benchmark classes
├── samples/
│   └── FlowT.SampleApp/                # Complete example application
│       ├── README.md                   # 📖 Comprehensive guide with examples
│       └── Features/
│           ├── Users/                  # User CRUD module (5 flows)
│           ├── Products/               # Product catalog module (3 flows)
│           └── Orders/                 # Order processing module (complex validation)
├── docs/
│   ├── api/                            # Auto-generated API reference
│   ├── BEST_PRACTICES.md               # Thread-safety & performance guide
│   ├── FLOWCONTEXT.md                  # Complete FlowContext guide
│   ├── PLUGINS.md                      # Plugin system guide
│   └── FlowT.links                     # Direct links to API members
└── GenerateDefaultDocumentation.ps1    # Script to rebuild API docs
```

---

### 🐛 Reporting Issues
- Use [GitHub Issue Templates](https://github.com/vlasta81/FlowT/issues/new/choose): **Bug Report** or **Feature Request**
- Include: .NET version, FlowT version, code sample, expected vs actual behavior

---

## 📄 License

MIT License - see [LICENSE.txt](LICENSE.txt) file for details.

---

## 🙏 Acknowledgments

- Inspired by [MediatR](https://github.com/jbogard/MediatR) and [DispatchR](https://github.com/samvasta/DispatchR)
- Built with [Roslyn Analyzers](https://github.com/dotnet/roslyn-analyzers)
- Benchmarked with [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
- Documentation generated with [DefaultDocumentation](https://github.com/Doraku/DefaultDocumentation)

---

**Made with ❤️ by [vlasta81](https://github.com/vlasta81)**

