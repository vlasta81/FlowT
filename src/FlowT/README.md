# FlowT - High-Performance Orchestration Library for .NET

**FlowT** is a high-performance orchestration library for .NET that implements the Chain of Responsibility pattern with a fluent API. Build maintainable, testable, and **ultra-fast** pipelines with specifications, policies, and handlers.

> 📚 **Full Documentation:** https://github.com/vlasta81/FlowT

---

## 📦 Package Information

| Property | Value |
|----------|-------|
| **Package** | FlowT |
| **NuGet** | https://www.nuget.org/packages/FlowT/ |
| **Targets** | .NET 10.0 (Primary), .NET Standard 2.0 (Compatibility) |
| **Dependencies** | None (zero external dependencies) |
| **Built-in plugins** | 10 (AuditPlugin, TenantPlugin, IdempotencyPlugin, PerformancePlugin, FlowScopePlugin, FeatureFlagPlugin, CorrelationPlugin, UserIdentityPlugin, RetryStatePlugin, TransactionPlugin) |
| **License** | MIT |

---

## ✨ Key Features

### 🚀 Performance
- ⚡ **2.7× faster than DispatchR** - The fastest .NET orchestration library
- ⚡ **9× faster than MediatR** - Singleton architecture with cached pipelines
- 💾 **84% less memory than MediatR** - Zero-allocation fast paths
- 🔥 **Thread-safe** - Lock-free operations with compile-time safety

### 🎯 Developer Experience
- 🧩 **Modular Architecture** - `IFlowModule` for clean feature organization
- 🔄 **Chain of Responsibility** - Composable pipeline with specs, policies, handlers
- 🎨 **Fluent API** - Intuitive pipeline configuration
- 📝 **Automatic Context** - No manual FlowContext creation needed
- 🛡️ **27 Roslyn Analyzers** - Compile-time safety for threading & DI

### 💡 Advanced Features
- 🔍 **FlowInterrupt** - Type-safe error handling without exceptions
- 🧩 **`FlowSpecification<TRequest>`** - Optional abstract base class: `Continue()`, `Fail()`, `Stop()` helpers (zero-allocation cached path)
- 🔌 **Plugin System** - PerFlow services with 8.7× warm-path speedup, 10 built-in plugins
- 📊 **Named Keys** - Store multiple values of same type
- 🎭 **Scoped Services** - Safe `ctx.Service<T>()` in singleton handlers
- 🏷️ **Feature Flags** - `FeatureFlagPlugin` wraps `IVariantFeatureManager` with per-flow cache
- 📝 **Audit Trail** - `AuditPlugin` accumulates structured events per flow execution
- ⏱️ **Performance Metrics** - `PerformancePlugin` measures named code sections with `Stopwatch`
- 🏢 **Multi-tenancy** - `TenantPlugin` resolves tenant from claim → header → route → default
- 🔑 **Idempotency** - `IdempotencyPlugin` reads `X-Idempotency-Key` header once per flow
- 🔭 **DI Scope Control** - `FlowScopePlugin` for explicit scopes in background/non-HTTP flows

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
public record CreateUserResponse(Guid Id, string Email);

// 2. Create handler
public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    public async ValueTask<CreateUserResponse> HandleAsync(
        CreateUserRequest request, FlowContext context)
    {
        var db = context.Service<AppDbContext>();
        var user = new User { Email = request.Email, Name = request.Name };
        db.Users.Add(user);
        await db.SaveChangesAsync(context.CancellationToken);
        return new CreateUserResponse(user.Id, user.Email);
    }
}

// 3. Define flow with pipeline
[FlowDefinition]
public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
    {
        flow
            .Check<ValidateEmailSpecification>()
            .Use<LoggingPolicy>()
            .OnInterrupt(interrupt => 
                new CreateUserResponse(Guid.Empty, interrupt.Message))
            .Handle<CreateUserHandler>();
    }
}

// 4. Register module
[FlowModule]
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", async (
            CreateUserRequest request,
            CreateUserFlow flow,
            HttpContext httpContext) =>
        {
            return await flow.ExecuteAsync(request, httpContext);
        });
    }
}

// 5. In Program.cs
builder.Services.AddFlowModules(typeof(Program).Assembly);
app.MapFlowModules();
```

---

## 📊 Performance

| Metric | Result |
|--------|--------|
| Speed vs DispatchR | **1.6-2.8× faster** |
| Speed vs MediatR | **9× faster** |
| Memory vs MediatR | **84% less** |

> 📊 **Detailed Benchmarks:** https://github.com/vlasta81/FlowT/tree/main/benchmarks/FlowT.Benchmarks

---

## 📚 Core Concepts

### Modules (`IFlowModule`)
Organize features into cohesive modules with `[FlowModule]` attribute for auto-discovery.

### Flows (`FlowDefinition<TRequest, TResponse>`)
Define reusable orchestration pipelines with `[FlowDefinition]` attribute.

### Context (`FlowContext`)
Per-request execution context with scoped service resolution, storage, and events.

### Specifications (`IFlowSpecification<TRequest>` / `FlowSpecification<TRequest>`)
Reusable validation and business rules that can interrupt flow with `FlowInterrupt`. Inherit from `FlowSpecification<TRequest>` for `Continue()`, `Fail()`, and `Stop()` helpers.

### Policies (`FlowPolicy<TRequest, TResponse>`)
Cross-cutting concerns as reusable middleware (logging, caching, transactions).

### Plugins (`Plugin<T>()`)
PerFlow services shared across all pipeline stages with automatic caching.
Register with `services.AddFlowPlugin<IMyPlugin, MyPlugin>()`, resolve with `context.Plugin<IMyPlugin>()`.

#### Built-in Plugins

| Plugin | Interface | Description |
|--------|-----------|-------------|
| `AuditPlugin` | `IAuditPlugin` | Accumulates structured `AuditEntry` records per flow execution |
| `TenantPlugin` | `ITenantPlugin` | Resolves tenant from claim `tid` → `X-Tenant-Id` header → route → `"default"` |
| `IdempotencyPlugin` | `IIdempotencyPlugin` | Reads `X-Idempotency-Key` header once per flow |
| `PerformancePlugin` | `IPerformancePlugin` | Measures named sections with `Stopwatch`; results in `Elapsed` dictionary |
| `FlowScopePlugin` | `IFlowScopePlugin` | Creates an explicit `IServiceScope` — useful in non-HTTP/background flows |
| `FeatureFlagPlugin` | `IFeatureFlagPlugin` | Evaluates feature flags via `IVariantFeatureManager` with per-flow caching |
| `CorrelationPlugin` | `ICorrelationPlugin` | Reads `X-Correlation-Id` header; falls back to `FlowId` |
| `UserIdentityPlugin` | `IUserIdentityPlugin` | Exposes `ClaimsPrincipal`, `UserId`, `Email`, `IsAuthenticated`, `IsInRole` |
| `RetryStatePlugin` | `IRetryStatePlugin` | Tracks retry attempt counter for retry policies |
| `TransactionPlugin` | `ITransactionPlugin` (abstract) | Base for custom transaction implementations (`BeginAsync`, `CommitAsync`, `RollbackAsync`) |

#### Example — FeatureFlagPlugin + AuditPlugin

```csharp
// appsettings.json
{
  "FeatureManagement": {
    "NewCheckout": true
  }
}

// Program.cs
builder.Services.AddFeatureManagement();
builder.Services.AddFlowPlugin<IFeatureFlagPlugin, FeatureFlagPlugin>();
builder.Services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();

// Handler
public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
{
    var ff = ctx.Plugin<IFeatureFlagPlugin>();
    if (!await ff.IsEnabledAsync("NewCheckout", ctx.CancellationToken))
        return new Response { Skipped = true };

    var audit = ctx.Plugin<IAuditPlugin>();
    audit.Record("CheckoutStarted", new { req.UserId });

    // ... business logic ...

    audit.Record("CheckoutCompleted", new { req.UserId, OrderId = order.Id });
    return new Response { OrderId = order.Id };
}
```

> 📖 **Complete Guide:** https://github.com/vlasta81/FlowT/blob/main/README.md#-core-concepts

---

## 🛡️ Compile-Time Safety

FlowT includes **27 Roslyn analyzers** that catch issues at compile-time:

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
- **FlowT026**: Thread.Sleep() in synchronous flow methods

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

> 📖 **All Analyzer Rules:** https://github.com/vlasta81/FlowT/blob/main/src/FlowT.Analyzers/README.md

**Example:**
```csharp
// ❌ FlowT003: Build fails!
public class BadHandler : IFlowHandler<Request, Response>
{
    private readonly AppDbContext _db; // Captive dependency!
    public BadHandler(AppDbContext db) { _db = db; }
}

// ✅ Correct pattern
public class GoodHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var db = ctx.Service<AppDbContext>(); // Safe!
        return await db.ProcessAsync(req);
    }
}
```

---

## 🔄 Migration Guides

### UserIdentityPlugin Removal (v1.2.0)
The `UserIdentityPlugin` and `IUserIdentityPlugin` interface have been removed. User identity functionality is now provided by built-in methods on `FlowContext`:

| Old Plugin Method | New Built-in Method |
|------------------|---------------------|
| `context.Plugin<IUserIdentityPlugin>().UserId` | `context.GetUserId()` |
| `context.Plugin<IUserIdentityPlugin>().Email` | `context.GetUser()?.Email` |
| `context.Plugin<IUserIdentityPlugin>().IsAuthenticated` | `context.IsAuthenticated()` |
| `context.Plugin<IUserIdentityPlugin>().IsInRole("Admin")` | `context.IsInRole("Admin")` |
| `context.Plugin<IUserIdentityPlugin>().Principal` | `context.GetUser()` |

> 📖 **Migration Guide:** https://github.com/vlasta81/FlowT/blob/main/docs/MIGRATION_UserIdentityPlugin.md

---

## 📖 Documentation

| Resource | Link |
|----------|------|
| **Full README** | https://github.com/vlasta81/FlowT |
| **API Reference** | https://github.com/vlasta81/FlowT/tree/main/docs/api |
| **Best Practices** | https://github.com/vlasta81/FlowT/blob/main/docs/BEST_PRACTICES.md |
| **FlowContext Guide** | https://github.com/vlasta81/FlowT/blob/main/docs/FLOWCONTEXT.md |
| **Plugin System** | https://github.com/vlasta81/FlowT/blob/main/docs/PLUGINS.md |
| **Benchmarks** | https://github.com/vlasta81/FlowT/tree/main/benchmarks/FlowT.Benchmarks |
| **Sample App** | https://github.com/vlasta81/FlowT/tree/main/samples/FlowT.SampleApp |
| **Analyzers** | https://github.com/vlasta81/FlowT/tree/main/src/FlowT.Analyzers |

---

## 🧪 Testing

- **277+ unit tests** with full coverage
- xUnit test framework
- Thread-safety and concurrency tests
- Analyzer verification tests

> 📖 **Test Suite:** https://github.com/vlasta81/FlowT/tree/main/tests/FlowT.Tests

---

## 🤝 Contributing

Found a bug or have a feature request? Please open an issue:
https://github.com/vlasta81/FlowT/issues

### Deprecation Policy
For future breaking changes, FlowT follows a deprecation-first approach:
1. Features are marked `[Obsolete]` with migration guidance in a minor version
2. Deprecated features are removed in the next major version
3. Migration guides are provided in `docs/MIGRATION_*.md`

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE.txt) file for details.

---

**Made with ❤️ by [vlasta81](https://github.com/vlasta81)**
