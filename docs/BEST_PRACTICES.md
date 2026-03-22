# FlowT Best Practices

This guide explains best practices for building reliable, thread-safe FlowT applications.

## 🏗️ Architecture

### Singleton Components = Stateless Components

FlowT handlers, policies, and specifications are **registered as singletons** for performance:

```csharp
// ✅ Explicit flow registration (recommended)
services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
```

**Why singleton?**
- ✅ Pipeline is compiled once and cached
- ✅ Zero allocations per request
- ✅ 9-10x faster than MediatR

**Consequence:**
- Components are **shared between concurrent requests**
- ⚠️ Mutable state = race conditions
- ⚠️ Scoped dependencies = captive dependency anti-pattern

> 📖 **Migrating from AddFlows()?** See [Migration Guide](MIGRATION_AddFlows.md)

---

## ✅ DO: Use Readonly Fields

```csharp
public class UserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly IUserRepository _repository; // ✅ OK - injected once
    private readonly ILogger _logger; // ✅ OK - thread-safe

    public UserHandler(IUserRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async ValueTask<CreateUserResponse> HandleAsync(
        CreateUserRequest request, FlowContext context)
    {
        // ✅ All per-request data is in parameters
        _logger.LogInformation("Creating user: {Email}", request.Email);
        var user = await _repository.SaveAsync(request.ToUser());
        return user.ToResponse();
    }
}
```

**✅ Safe patterns:**
- Readonly dependencies injected in constructor
- All per-request data passed as parameters
- Use `FlowContext` for shared per-request state

---

## ❌ DON'T: Use Mutable Fields

```csharp
// ❌ WRONG: Mutable field causes race conditions
public class BadHandler : IFlowHandler<Request, Response>
{
    private int _requestCounter = 0; // ❌ Shared between requests!
    private List<string> _cache = new(); // ❌ Not thread-safe!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _requestCounter++; // ❌ Race condition!
        _cache.Add(req.Data); // ❌ Race condition!
        
        // Two concurrent requests:
        // Request A: _requestCounter = 5
        // Request B: _requestCounter = 5 (reads same value!)
        // Both increment to 6 → one increment lost!
        
        return new Response();
    }
}
```

**❌ Problems:**
- Race conditions (lost updates, corrupted data)
- Data leaks between users
- Hard-to-debug intermittent failures

**✅ Solution: Use FlowContext for per-request data**

```csharp
public class GoodHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Per-request counter stored in context
        int counter = ctx.TryGet<int>(out var c) ? c + 1 : 1;
        ctx.Set(counter);
        
        // ✅ Each request has its own context
        return new Response(counter);
    }
}
```

---

## ❌ DON'T: Capture Scoped Services

```csharp
// ❌ WRONG: Captive dependency anti-pattern
public class BadHandler : IFlowHandler<Request, Response>
{
    private readonly DbContext _db; // ❌ DbContext is scoped!
    
    public BadHandler(DbContext db) // ❌ Will be disposed after first request
    {
        _db = db;
    }
    
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ❌ ObjectDisposedException after first request!
        await _db.Users.ToListAsync();
        return new Response();
    }
}
```

**❌ Problems:**
- `DbContext` is disposed after first request
- All subsequent requests throw `ObjectDisposedException`
- Memory leaks (scoped services never released)

**✅ Solution: Resolve scoped services per-request**

```csharp
public class GoodHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Resolve DbContext per-request using new convenience method
        var db = ctx.Service<DbContext>();
        
        // ✅ Each request gets fresh scoped DbContext
        await db.Users.ToListAsync();
        return new Response();
    }
}
```

**Common scoped services:**
- `DbContext` (Entity Framework)
- `IHttpContextAccessor`
- `HttpContext`, `HttpRequest`, `HttpResponse`
- `SignInManager`, `UserManager` (Identity)

---

## ❌ DON'T: Store FlowContext in Fields

```csharp
// ❌ WRONG: Capturing context causes data leaks
public class BadHandler : IFlowHandler<Request, Response>
{
    private FlowContext? _context; // ❌ Will leak between requests!
    
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _context = ctx; // ❌ Next request sees previous user's context!
        
        // Request A stores sensitive data in context
        // Request B sees Request A's context → data leak!
        
        return new Response();
    }
}
```

**❌ Problems:**
- Data leaks between users (security risk!)
- Context contains request-specific state
- Violates isolation guarantees

**✅ Solution: Use context only as parameter**

```csharp
public class GoodHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Context used only within method scope
        ctx.Set(req.Data, key: "data");
        var data = ctx.Get<string>(key: "data");
        return new Response(data);
    }
}
```

---

## 🛡️ Compile-Time Safety with Analyzers

FlowT includes Roslyn analyzers that **prevent these mistakes at compile-time**:

### FlowT001: Mutable Field (Warning)

```csharp
private int _counter = 0; // ⚠️ FlowT001: Mutable field can cause race conditions
```

**Quick fix:** Make readonly or use FlowContext

### FlowT003: Captive Dependency (Error)

```csharp
public Handler(DbContext db) // 🔴 FlowT003: Cannot capture scoped service
```

**Quick fix:** Use `context.Service<DbContext>()`

**Build fails** until fixed!

### FlowT006: Context Capturing (Error)

```csharp
private FlowContext? _ctx; // 🔴 FlowT006: Cannot store context in field
```

**Build fails** until fixed!

---

## 🎯 Summary Checklist

**✅ DO:**
- Use readonly fields for dependencies
- Store per-request data in `FlowContext` or method parameters
- Resolve scoped services with `context.Service<T>()`
- Keep handlers/policies/specifications stateless
- Use `ConcurrentDictionary` if you must cache data

**❌ DON'T:**
- Use mutable fields (race conditions)
- Capture scoped services in constructor (captive dependency)
- Store `FlowContext` in fields (data leaks)
- Use `static` mutable fields (global state)
- Use `List<T>`, `Dictionary<T>` without locking

---

## 📚 Related

- [FlowT.Analyzers Documentation](../src/FlowT.Analyzers/README.md)
- [Thread Safety in C#](https://learn.microsoft.com/dotnet/standard/threading/managed-threading-best-practices)
- [Dependency Injection Lifetimes](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
