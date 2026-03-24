# FlowT - High-Performance Orchestration Library for .NET

[![NuGet](https://img.shields.io/nuget/v/FlowT.svg)](https://www.nuget.org/packages/FlowT/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

**FlowT** is a high-performance orchestration library for .NET that implements the Chain of Responsibility pattern with a fluent API. Build maintainable, testable, and **ultra-fast** pipelines with specifications, policies, and handlers.

> 📚 **Full Documentation:** https://github.com/vlasta81/FlowT

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
- 🛡️ **26 Roslyn Analyzers** - Compile-time safety for threading & DI

### 💡 Advanced Features
- 🔍 **FlowInterrupt** - Type-safe error handling without exceptions
- 🔌 **Plugin System** - PerFlow services with 8.7× warm-path speedup
- 📊 **Named Keys** - Store multiple values of same type
- 🎭 **Scoped Services** - Safe `ctx.Service<T>()` in singleton handlers

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

### Specifications (`IFlowSpecification<TRequest>`)
Reusable validation and business rules that can interrupt flow with `FlowInterrupt`.

### Policies (`FlowPolicy<TRequest, TResponse>`)
Cross-cutting concerns as reusable middleware (logging, caching, transactions).

### Plugins (`Plugin<T>()`)
PerFlow services shared across all pipeline stages with automatic caching.

> 📖 **Complete Guide:** https://github.com/vlasta81/FlowT/blob/main/README.md#-core-concepts

---

## 🛡️ Compile-Time Safety

FlowT includes **26 Roslyn analyzers** that catch issues at compile-time:

- 🔴 **14 Errors** - Thread-safety, DI anti-patterns, data leaks
- ⚠️ **9 Warnings** - Async issues, locking problems
- ℹ️ **3 Info** - Cancellation, best practices

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

> 📖 **All Analyzer Rules:** https://github.com/vlasta81/FlowT/blob/main/src/FlowT.Analyzers/README.md

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

- **206+ unit tests** with full coverage
- xUnit test framework
- Thread-safety and concurrency tests
- Analyzer verification tests

> 📖 **Test Suite:** https://github.com/vlasta81/FlowT/tree/main/tests/FlowT.Tests

---

## 📦 Package Information

- **Package:** FlowT
- **Targets:** .NET 10.0, .NET Standard 2.0
- **Dependencies:** None (zero external dependencies)
- **License:** MIT

---

## 🤝 Contributing

Found a bug or have a feature request? Please open an issue:
https://github.com/vlasta81/FlowT/issues

---

## 📄 License

MIT License - see [LICENSE](https://github.com/vlasta81/FlowT/blob/main/LICENSE.txt) file for details.

---

**Made with ❤️ by [vlasta81](https://github.com/vlasta81)**
