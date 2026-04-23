# FlowContext - Execution Context Guide

## 📋 What is FlowContext?

**FlowContext** is a shared execution context for a flow and all its sub-flows. A single instance is created when the flow starts and passes through the entire pipeline (specifications → policies → handler).

### 🎯 What does it do?

FlowContext provides:

1. ✅ **Dependency Injection** - access to scoped services (DbContext, HttpContext, etc.)
2. ✅ **Shared State** - storing data between pipeline steps (Set/Get/GetOrAdd)
3. ✅ **Cancellation** - observing request cancellation (timeout, client disconnect)
4. ✅ **HTTP Context** - access to request/response (when HTTP request)
5. ✅ **Events** - publishing events (PublishAsync, PublishInBackground)
6. ✅ **Timing** - performance measurement (`StartTimer` — returns `TimerDisposable`, stop via `using`)
7. ✅ **Flow ID** - unique identifier for logging/correlation

---

## 🔧 API Reference

### **1. Services (Dependency Injection)**

#### `Service<T>()` - Resolve Required Service

Requests a service from the DI container. Throws an exception if the service is not registered.

```csharp
public T Service<T>() where T : notnull
```

**Usage:**
```csharp
// ✅ Scoped services (DbContext, HttpContext accessor)
var db = context.Service<AppDbContext>();
var httpContextAccessor = context.Service<IHttpContextAccessor>();

// ✅ Singleton services
var logger = context.Service<ILogger<MyHandler>>();
var config = context.Service<IConfiguration>();

// ✅ Transient services
var emailSender = context.Service<IEmailSender>();
```

**⚠️ Important:**
- Use for **scoped services** in singleton handlers
- FlowContext is per-request → each request has its own scoped services
- Safe even for singleton handlers (no captive dependencies)

---

#### `TryService<T>()` - Resolve Optional Service

Attempts to request a service. Returns `null` if not registered.

```csharp
public T? TryService<T>() where T : class
```

**Usage:**
```csharp
// ✅ Optional services
var cache = context.TryService<IMemoryCache>();
if (cache != null)
{
    cache.Set("key", value);
}

// ✅ Feature flags
var featureToggle = context.TryService<IFeatureToggle>();
var isEnabled = featureToggle?.IsEnabled("NewFeature") ?? false;

// ✅ Fallback pattern
var logger = context.TryService<ILogger<MyHandler>>() ?? NullLogger.Instance;
```

---

#### `Plugin<T>()` - PerFlow Plugin Resolution

Resolves a plugin that is cached for the lifetime of this flow execution. First call resolves via DI and caches the instance; subsequent calls return the same instance with zero allocation.

```csharp
public T Plugin<T>() where T : notnull
```

**Usage:**
```csharp
// ✅ Resolve a plugin (PerFlow lifetime — cached after first call)
var audit = context.Plugin<IAuditPlugin>();
audit.Record("Validation.Passed");

// ✅ Same instance across pipeline stages
// Policy:
var counter = context.Plugin<ICounterPlugin>();
counter.Increment("policy");

// Handler (same context → same instance):
var counter = context.Plugin<ICounterPlugin>(); // Returns the same instance
counter.Increment("handler");
```

**Registration:**
```csharp
// In IFlowModule.Register() or Program.cs
services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();
```

**Performance:**
- Cold (first call): 204 ns / 464 B (DI resolution + cache store)
- Warm (cached): **23 ns / 0 B** (lockless fast path, zero allocation)
- **8.7× speedup** after first call

**⚠️ Important:**
- Throws `InvalidOperationException` if `T` is not registered via `AddFlowPlugin`
- Plugins are isolated between `FlowContext` instances — different requests get different plugin instances
- Plugins that inherit from `FlowPlugin` get `Context` automatically injected

📚 **[Complete Plugin System Guide →](PLUGINS.md)**

---

### **2. Shared State Storage**

#### `Set<T>(value, key)` - Store Value

Stores a value in the context. Thread-safe.

```csharp
public void Set<T>(T value, string? key = null)
```

**Usage:**
```csharp
// ✅ Default key (single value of given type)
context.Set(user);
context.Set(DateTime.UtcNow);

// ✅ Named keys (multiple values of same type)
context.Set(adminUser, key: "admin");
context.Set(guestUser, key: "guest");

// ✅ Complex types
context.Set(new OrderContext { OrderId = 123 }, key: "order");

// ✅ Primitive types
context.Set(42, key: "retryCount");
context.Set("en-US", key: "culture");
```

**Keying:**
- **Default key** (`null`): Type-only key → only one value of given type
- **Named key**: (Type + String) composite key → multiple values of same type

---

#### `TryGet<T>(out value, key)` - Retrieve Value

Attempts to retrieve a value from the context. Returns `true` if found.

```csharp
public bool TryGet<T>(out T value, string? key = null)
```

**Usage:**
```csharp
// ✅ Default key
if (context.TryGet<User>(out var user))
{
    Console.WriteLine($"User: {user.Name}");
}

// ✅ Named keys
if (context.TryGet<User>(out var admin, key: "admin"))
{
    // Process with admin privileges
}

if (context.TryGet<int>(out var retryCount, key: "retryCount"))
{
    if (retryCount > 3)
    {
        // Too many retries
    }
}

// ✅ Check existence without using value
if (context.TryGet<CacheData>(out _, key: "cached"))
{
    // Cache exists
}
```

**Performance:**
- Fast path: Lock-free read (common case)
- Double-check lock: Minimal contention

---

#### `GetOrAdd<T>(factory, key)` - Get or Create Value

Gets an existing value or creates a new one using the factory. Thread-safe.

```csharp
public T GetOrAdd<T>(Func<T> factory, string? key = null)
```

**Usage:**
```csharp
// ✅ Lazy initialization
var cache = context.GetOrAdd(() => new Dictionary<int, User>(), key: "userCache");

// ✅ Expensive computation (computed only once)
var config = context.GetOrAdd(() => LoadConfiguration(), key: "config");

// ✅ Per-request singleton
var processor = context.GetOrAdd(() => new DataProcessor());

// ✅ Multiple caches
var userCache = context.GetOrAdd(() => new Dictionary<int, User>(), key: "users");
var productCache = context.GetOrAdd(() => new Dictionary<int, Product>(), key: "products");
```

**Guarantees:**
- Factory is called **only once** (even with concurrent access)
- Returned value is **the same instance** for the same key

---

#### `GetOrAdd<T, TArg>(arg, factory, key)` - Get or Create with Argument

Optimized version of `GetOrAdd` with argument - **no closure allocation**.

```csharp
public T GetOrAdd<T, TArg>(TArg arg, Func<TArg, T> factory, string? key = null)
```

**Usage:**
```csharp
// ✅ Avoid closure allocation
var cache = context.GetOrAdd(
    arg: 100, 
    factory: capacity => new Dictionary<int, User>(capacity),
    key: "userCache"
);

// ✅ Pass service as argument
var service = context.Service<IUserService>();
var users = context.GetOrAdd(
    arg: service,
    factory: svc => svc.GetAllUsers(),
    key: "allUsers"
);

// ✅ Configuration-based initialization
var config = context.Service<IConfiguration>();
var connection = context.GetOrAdd(
    arg: config,
    factory: cfg => new SqlConnection(cfg["ConnectionString"]),
    key: "dbConnection"
);
```

**Performance:**
- **No closure** → zero extra allocations
- Use when factory needs external state

---

#### `Push<T>(value, key)` - Scoped Value Override

Temporarily overrides a value. Returns `IDisposable` for automatic restoration of the original value.

```csharp
public IDisposable Push<T>(T value, string? key = null)
```

**Usage:**
```csharp
// ✅ Temporary override
context.Set("Production", key: "environment");

using (context.Push("Test", key: "environment"))
{
    // Inside this scope: environment = "Test"
    await DoSomething(context);
    
} // Automatically restored to "Production"

// ✅ Nested scopes
context.Set(LogLevel.Info, key: "logLevel");

using (context.Push(LogLevel.Debug, key: "logLevel"))
{
    // Debug logging enabled
    
    using (context.Push(LogLevel.Trace, key: "logLevel"))
    {
        // Trace logging enabled
    }
    
    // Back to Debug
}

// Back to Info

// ✅ Testing scenarios
context.Set(realUser);

using (context.Push(testUser))
{
    // Use test user for this block
    var result = await handler.HandleAsync(request, context);
    Assert.Equal(testUser.Id, result.UserId);
}
```

**Use cases:**
- Testing (override dependencies)
- Scoped configuration
- Temporary privilege elevation

---

### **2.1. Named Keys - Advanced Feature**

FlowContext supports **named keys** - storing multiple values of the same type under different named keys. This feature enables more flexible state management without the need to create wrapper types.

#### 🎯 Motivation

**Before Named Keys:**
```csharp
// You had to create wrapper types
public record AdminUser(string Name);
public record GuestUser(string Name);

context.Set(new AdminUser("John"));
context.Set(new GuestUser("Jane"));
```

**After Named Keys:**
```csharp
// Simple named values of the same type
context.Set("John", key: "admin");
context.Set("Jane", key: "guest");
```

#### 🚀 API

All FlowContext methods support an optional `string? key` parameter:

```csharp
// Set
context.Set(value);                   // Default key
context.Set(value, key: "mykey");     // Named key

// TryGet
context.TryGet<T>(out var value);              // Default key
context.TryGet<T>(out var value, key: "mykey"); // Named key

// GetOrAdd
context.GetOrAdd(() => new T());              // Default key
context.GetOrAdd(() => new T(), key: "mykey"); // Named key

// GetOrAdd with arg
context.GetOrAdd(arg, factory);              // Default key
context.GetOrAdd(arg, factory, key: "mykey"); // Named key

// Push (scope)
context.Push(value);                   // Default key
context.Push(value, key: "mykey");     // Named key
```

#### 💡 Practical Examples

**1. Multiple Users:**
```csharp
public class CreateOrderHandler : IFlowHandler<CreateOrderRequest, CreateOrderResponse>
{
    public async ValueTask<CreateOrderResponse> HandleAsync(
        CreateOrderRequest request, 
        FlowContext context)
    {
        // Store different users under different keys
        var admin = await GetUser(request.AdminId);
        var customer = await GetUser(request.CustomerId);

        context.Set(admin, key: "admin");
        context.Set(customer, key: "customer");

        // Later in pipeline...
        if (context.TryGet<User>(out var reviewer, key: "admin"))
        {
            // Process with admin privileges
        }

        return new CreateOrderResponse();
    }
}
```

**2. Multiple Caches:**
```csharp
public class CachingPolicy : FlowPolicy<TRequest, TResponse>
{
    public override async ValueTask<TResponse> HandleAsync(
        TRequest request, 
        FlowContext context)
    {
        // Different caches for different purposes
        var userCache = context.GetOrAdd(() => new Dictionary<int, User>(), key: "users");
        var productCache = context.GetOrAdd(() => new Dictionary<int, Product>(), key: "products");
        var orderCache = context.GetOrAdd(() => new Dictionary<int, Order>(), key: "orders");

        // Use caches...
        var result = await Next.HandleAsync(request, context);
        return result;
    }
}
```

**3. Multiple Configurations:**
```csharp
public class ConfigurationPolicy : FlowPolicy<Request, Response>
{
    public override async ValueTask<Response> HandleAsync(Request request, FlowContext context)
    {
        // Store different configurations
        context.Set(new RetryConfig { MaxRetries = 3 }, key: "retry");
        context.Set(new TimeoutConfig { Seconds = 30 }, key: "timeout");
        context.Set(new LoggingConfig { Level = LogLevel.Debug }, key: "logging");

        return await Next.HandleAsync(request, context);
    }
}

public class MyHandler : IFlowHandler<Request, Response>
{
    public ValueTask<Response> HandleAsync(Request request, FlowContext context)
    {
        if (context.TryGet<RetryConfig>(out var retryConfig, key: "retry"))
        {
            // Use retry configuration
        }

        if (context.TryGet<TimeoutConfig>(out var timeoutConfig, key: "timeout"))
        {
            // Use timeout configuration
        }

        return ValueTask.FromResult(new Response());
    }
}
```

**4. Scoped Values with Named Keys:**
```csharp
public class ScopedPolicy : FlowPolicy<Request, Response>
{
    public override async ValueTask<Response> HandleAsync(Request request, FlowContext context)
    {
        // Temporarily override a named value
        context.Set("Production Mode", key: "environment");

        using (context.Push("Test Mode", key: "environment"))
        {
            // Inside this scope, environment is "Test Mode"
            var result = await Next.HandleAsync(request, context);

            // Test-specific processing...
            return result;
        }

        // Outside scope, environment is back to "Production Mode"
    }
}
```

#### 🎨 Use Cases

**✅ When to Use Named Keys:**

1. **Multiple instances of same type**
   - Different users (admin, customer, guest)
   - Different configurations (retry, timeout, logging)
   - Different cache/storage objects

2. **Avoiding wrapper types**
   - You don't want to create `AdminUser`, `GuestUser` classes
   - Simpler API without type pollution

3. **Dynamic keys**
   - Keys known only at runtime
   - User-defined scopes
   - Multi-tenant scenarios

**❌ When NOT to Use Named Keys:**

1. **Type safety is critical**
   - Better to use distinct types (`AdminUser`, `GuestUser`)
   - Compile-time safety > runtime flexibility

2. **Single value per type**
   - If you're storing only one value of a given type
   - Default key (null) is simpler

3. **Public API**
   - Named keys are string-based = possible typos
   - For public API consider strongly-typed keys

#### ⚠️ Important Notes

**1. Default Key vs Named Key:**
```csharp
context.Set("Value1");                   // Default key (null)
context.Set("Value2", key: "mykey");     // Named key

context.TryGet<string>(out var v1);             // Returns "Value1"
context.TryGet<string>(out var v2, key: "mykey"); // Returns "Value2"
```
**Default and named keys are independent!**

**2. Type + Key = Composite Key:**
```csharp
// Same key name, different types = different values
context.Set("String", key: "key");
context.Set(42, key: "key");
context.Set(true, key: "key");

context.TryGet<string>(out var s, key: "key");  // "String"
context.TryGet<int>(out var i, key: "key");     // 42
context.TryGet<bool>(out var b, key: "key");    // true
```
**Composite key = (Type, StringKey)**

**3. Thread Safety:**

Named keys are **fully thread-safe**:
```csharp
Parallel.For(0, 100, i =>
{
    context.Set($"Value{i}", key: $"key{i}");
    context.TryGet<string>(out var value, key: $"key{i}");
});
```

**4. Performance:**

Named keys have **minimal overhead**:
- CompositeKey is a struct (no allocation)
- HashCode is efficient (`HashCode.Combine`)
- Same double-check lock pattern

```csharp
// Benchmark results
Set<T>(value):           ~21ns
Set<T>(value, "key"):    ~23ns  (+2ns overhead)
```

#### 📚 Best Practices

**✅ DO:**
```csharp
// Use descriptive key names
context.Set(user, key: "currentUser");
context.Set(admin, key: "systemAdmin");

// Use constants for common keys
public static class ContextKeys
{
    public const string CurrentUser = "currentUser";
    public const string TenantId = "tenantId";
}

context.Set(user, key: ContextKeys.CurrentUser);

// Document your keys
/// <summary>
/// Stores user under "currentUser" key.
/// </summary>
public void StoreCurrentUser(User user, FlowContext context)
{
    context.Set(user, key: "currentUser");
}
```

**❌ DON'T:**
```csharp
// Don't use magic strings everywhere
context.Set(user, key: "usr");  // What is "usr"?

// Don't use numbered keys without reason
context.Set(value1, key: "1");
context.Set(value2, key: "2");  // Use typed keys or arrays instead

// Don't use named keys when type is sufficient
context.Set(config, key: "theOnlyConfig");  // Just use default key
context.Set(config);  // Better
```

---

### **3. Cancellation**

#### `CancellationToken` - Observe Request Cancellation

Token for observing request cancellation (client disconnect, timeout).

```csharp
public required CancellationToken CancellationToken { get; init; }
```

**Usage:**
```csharp
// ✅ Pass to async operations
await db.SaveChangesAsync(context.CancellationToken);
await httpClient.GetAsync(url, context.CancellationToken);

// ✅ Check if cancelled
if (context.CancellationToken.IsCancellationRequested)
{
    return new Response { Status = "Cancelled" };
}

// ✅ Throw if cancelled
context.CancellationToken.ThrowIfCancellationRequested();

// ✅ Long-running operations
for (int i = 0; i < 1000; i++)
{
    context.CancellationToken.ThrowIfCancellationRequested();
    await ProcessItem(items[i]);
}

// ✅ Combine with timeout
using var cts = CancellationTokenSource.CreateLinkedTokenSource(
    context.CancellationToken,
    new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token
);

await SlowOperation(cts.Token);
```

**Sources:**
- HTTP request cancellation (client disconnect)
- Timeout policies
- Manual cancellation (testing)

---

### **4. HTTP Context**

#### `HttpContext` - HTTP Request/Response Access

Access to HTTP context (only for HTTP requests).

```csharp
public HttpContext? HttpContext { get; init; }
```

**Usage:**
```csharp
// ✅ Read request data
var userId = context.HttpContext?.User.FindFirst("sub")?.Value;
var userAgent = context.HttpContext?.Request.Headers["User-Agent"];
var ip = context.HttpContext?.Connection.RemoteIpAddress;
var path = context.HttpContext?.Request.Path;

// ✅ Set response metadata
if (context.HttpContext != null)
{
    context.HttpContext.Response.StatusCode = 201;
    context.HttpContext.Response.Headers["Location"] = $"/api/users/{newUserId}";
    context.HttpContext.Response.Cookies.Append("session", sessionId);
}

// ✅ Access authenticated user
var user = context.HttpContext?.User;
if (user?.Identity?.IsAuthenticated == true)
{
    var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
}

// ✅ Read route values
var id = context.HttpContext?.GetRouteValue("id")?.ToString();

// ✅ Per-request items
context.HttpContext?.Items["RequestId"] = Guid.NewGuid();
```

**⚠️ Warnings:**
- **Never write to Response.Body** - FlowT handles serialization
- **Null check required** - not available in non-HTTP scenarios
- **Available in:** HTTP requests (ASP.NET Core)
- **Null in:** Background jobs, console apps, unit tests, Blazor WASM

---

### **5. Events**

#### `PublishAsync<TEvent>(event)` - Publish Event Synchronously

Publishes an event and waits for all event handlers.

```csharp
public async ValueTask PublishAsync<TEvent>(TEvent @event) where TEvent : notnull
```

**Usage:**
```csharp
// ✅ Publish and wait
await context.PublishAsync(new UserCreatedEvent(userId));

// ✅ Critical events (must complete)
await context.PublishAsync(new OrderPlacedEvent(orderId));

// Waits for all handlers to complete before continuing
```

**When to use:**
- Critical events (must complete)
- Transactional consistency
- Error handling required

---

#### `PublishInBackground<TEvent>(event)` - Fire and Forget

Publishes an event in the background without waiting.

```csharp
public void PublishInBackground<TEvent>(TEvent @event) where TEvent : notnull
```

**Usage:**
```csharp
// ✅ Fire and forget
context.PublishInBackground(new EmailSentEvent(emailId));
context.PublishInBackground(new AuditLogEvent(userId, action));

// ✅ Non-critical notifications
context.PublishInBackground(new UserLoggedInEvent(userId));

// Returns immediately, event processed asynchronously
```

**When to use:**
- Non-critical events
- Performance-sensitive paths
- Notifications, logging, analytics

---

### **6. Timing & Performance**

#### `StartTimer(key)` - Measure Elapsed Time

Starts a high-precision timer. Returns a `TimerDisposable` that records the elapsed time when disposed.
The elapsed time (in raw `Stopwatch` ticks) is stored in the context under the given key.

```csharp
public TimerDisposable StartTimer(string key)
```

> ⚠️ There is **no `StopTimer` method**. The timer stops automatically when the returned `TimerDisposable` is disposed — use `using`.

**Usage:**
```csharp
// ✅ Timer stops and stores elapsed time when using block exits
using (context.StartTimer("database-query"))
{
    users = await db.Users.ToListAsync();
}

// ✅ Equivalent with using var
using var timer = context.StartTimer("database-query");
var users = await db.Users.ToListAsync();
// elapsed recorded automatically when timer is disposed

// ✅ Multiple timers
using (context.StartTimer("validation"))
    ValidateRequest(request);

using (context.StartTimer("processing"))
    result = await ProcessData(request);

using (context.StartTimer("persistence"))
    await db.SaveChangesAsync();

// ✅ Nested timers
using (context.StartTimer("total-operation"))
{
    using (context.StartTimer("step1"))
        await Step1();

    using (context.StartTimer("step2"))
        await Step2();
}
```

**Performance:**
- Uses `Stopwatch.GetTimestamp()` internally
- Minimal overhead (~10 ns)
- Thread-safe

---

### **7. Flow Identification**

#### `FlowId` - Unique Flow Execution ID

Unique identifier for flow execution (shared across main flow and sub-flows).

```csharp
public Guid FlowId { get; init; }
```

#### `FlowIdString` - Flow ID as String

Returns `FlowId` formatted as a 32-character hexadecimal string without hyphens. This is a property (no method call overhead).

```csharp
public string FlowIdString { get; }
```

**Usage:**
```csharp
// ✅ Logging
logger.LogInformation("Flow {FlowId}: Processing request", context.FlowIdString);

// ✅ Correlation
var correlationId = context.FlowIdString;
httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

// ✅ Tracing
Activity.Current?.AddTag("flow.id", context.FlowIdString);

// ✅ Database auditing
var audit = new AuditLog
{
    FlowId = context.FlowId,
    UserId = userId,
    Action = "CreateOrder"
};
```

> ℹ️ **Migration note:** `GetFlowIdString()` was removed in v1.3.0. Replace all calls with the `FlowIdString` property.

---

#### `StartedAt` - Flow Start Timestamp

UTC timestamp when the flow execution started.

```csharp
public DateTimeOffset StartedAt { get; init; }
```

**Usage:**
```csharp
// ✅ Calculate total execution time
var duration = DateTimeOffset.UtcNow - context.StartedAt;
logger.LogInformation("Flow completed in {Duration}ms", duration.TotalMilliseconds);

// ✅ Timeout detection
var elapsed = DateTimeOffset.UtcNow - context.StartedAt;
if (elapsed.TotalSeconds > 30)
{
    return FlowInterrupt<Response>.Fail("Request timeout", 408);
}

// ✅ Performance monitoring
var executionTime = DateTimeOffset.UtcNow - context.StartedAt;
metrics.RecordExecutionTime("flow.execution", executionTime.TotalMilliseconds);
```

---

## 🎯 Common Patterns

### **Pattern 1: Lazy Expensive Computation**

```csharp
// Computed only once per request
var config = context.GetOrAdd(() => 
{
    // Expensive operation
    var data = LoadFromDatabase();
    var processed = ProcessData(data);
    return new Configuration(processed);
});

// Subsequent calls return cached instance
var config2 = context.GetOrAdd(() => ...); // Returns same instance
```

---

### **Pattern 2: Multi-Tenant Context**

```csharp
// Store tenant-specific data
var tenantId = context.HttpContext?.User.FindFirst("tenant_id")?.Value;
context.Set(tenantId, key: "tenantId");

// Tenant-specific cache
var tenantCache = context.GetOrAdd(
    arg: tenantId,
    factory: id => new Dictionary<string, object>(),
    key: $"cache:{id}"
);
```

---

### **Pattern 3: Conditional Event Publishing**

```csharp
// Critical events: wait for completion
if (isCriticalOrder)
{
    await context.PublishAsync(new OrderPlacedEvent(orderId));
}
else
{
    // Non-critical: fire and forget
    context.PublishInBackground(new OrderPlacedEvent(orderId));
}
```

---

### **Pattern 4: Scoped User Context**

```csharp
// Store current user
var user = await GetUser(userId);
context.Set(user);

// Temporarily impersonate admin
var admin = await GetAdmin();
using (context.Push(admin))
{
    // Operations run as admin
    await PerformAdminOperation(context);
}

// Back to original user
```

---

### **Pattern 5: Performance Monitoring**

```csharp
public class PerformanceMonitoringPolicy<TRequest, TResponse> : FlowPolicy<TRequest, TResponse>
{
    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        var start = Stopwatch.GetTimestamp();
        using var _ = context.StartTimer("total");

        var response = await Next!.HandleAsync(request, context);

        var elapsed = Stopwatch.GetElapsedTime(start);

        if (elapsed.TotalMilliseconds > 1000)
        {
            // Log slow request
            logger.LogWarning("Slow request: {FlowId} took {Duration}ms",
                context.FlowIdString, elapsed.TotalMilliseconds);
        }

        return response;
    }
}
```

---

## ⚠️ Best Practices

### ✅ DO:

```csharp
// ✅ Use context only as method parameter
public ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
{
    var db = ctx.Service<DbContext>();
    ctx.Set(data);
    return ...;
}

// ✅ Pass to async operations
await db.SaveChangesAsync(context.CancellationToken);

// ✅ Use named keys for multiple values
context.Set(admin, key: "admin");
context.Set(guest, key: "guest");

// ✅ Check HttpContext for null
if (context.HttpContext != null)
{
    var user = context.HttpContext.User;
}

// ✅ Use GetOrAdd for expensive operations
var cache = context.GetOrAdd(() => new ExpensiveCache());
```

---

### ❌ DON'T:

```csharp
// ❌ Store context in fields (causes data leaks!)
public class BadHandler : IFlowHandler<Request, Response>
{
    private FlowContext? _context; // ❌ NEVER DO THIS!
    
    public ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _context = ctx; // ❌ Data leak between requests!
        return ...;
    }
}

// ❌ Write to Response.Body
await context.HttpContext.Response.WriteAsync("data"); // ❌ Breaks FlowT

// ❌ Ignore cancellation token
await LongOperation(); // ❌ Use cancellation token!

// ❌ Use magic string keys
context.Set(user, "usr"); // ❌ Use descriptive keys or constants

// ❌ Assume HttpContext is always available
var user = context.HttpContext.User; // ❌ Null in non-HTTP scenarios
```

---

## 🧪 Testing

```csharp
// ✅ Create test context
var services = new ServiceCollection()
    .AddScoped<AppDbContext>()
    .BuildServiceProvider();

var context = new FlowContext
{
    Services = services.CreateScope().ServiceProvider,
    CancellationToken = CancellationToken.None,
    HttpContext = null // or mock HttpContext
};

// ✅ Test with data
context.Set(testUser);
var result = await handler.HandleAsync(request, context);
Assert.True(context.TryGet<User>(out var storedUser));

// ✅ Test cancellation
var cts = new CancellationTokenSource();
var testContext = new FlowContext
{
    Services = services,
    CancellationToken = cts.Token
};

cts.Cancel();
await Assert.ThrowsAsync<OperationCanceledException>(() =>
    handler.HandleAsync(request, testContext).AsTask()
);
```

---

## 📊 Performance Characteristics

| Operation | Time | Allocations |
|-----------|------|-------------|
| `Service<T>()` | ~20ns | 0 bytes |
| `Set<T>(value)` | ~20ns | 0 bytes* |
| `TryGet<T>(out value)` | ~15ns | 0 bytes |
| `GetOrAdd<T>(factory)` | ~25ns | 0 bytes** |
| `GetOrAdd<T, TArg>(arg, factory)` | ~23ns | 0 bytes |
| `StartTimer(key)` | ~10ns | 0 bytes**** |

*First call may allocate dictionary entry  
**Excluding factory allocations  
***First timer allocates dictionary  

---

## 🔗 Related Documentation

- [Best Practices](BEST_PRACTICES.md) - FlowT best practices
- [API Reference](api/index.md) - Complete API documentation

---

## 📝 Summary

**FlowContext** is the central object in FlowT, providing:

1. **DI Resolution** - `Service<T>()`, `TryService<T>()`
2. **Shared State** - `Set()`, `TryGet()`, `GetOrAdd()`, `Push()` (with optional named keys)
3. **Cancellation** - `CancellationToken`
4. **HTTP Access** - `HttpContext` (nullable)
5. **Events** - `PublishAsync()`, `PublishInBackground()`
6. **Timing** - `StartTimer()` (returns `TimerDisposable`, use with `using`)
7. **Identification** - `FlowId`, `StartedAt`

**Thread-safe**, **performant**, **zero-allocation** on fast path. Single instance per request, shared across the entire pipeline.
