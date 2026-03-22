# FlowT.Analyzers

Roslyn analyzers for FlowT framework that detect common threading, dependency injection, and data leakage issues at compile-time.

## 🎯 Purpose

FlowT components (handlers, policies, specifications) are **registered as singletons** for performance. This means:
- ✅ Extremely fast (pipeline cached, zero allocations)
- ⚠️ **Shared between concurrent requests** - Critical for thread safety!
- ❌ Mutable state can cause race conditions
- ❌ Capturing scoped services causes "captive dependency" anti-pattern
- ❌ Storing request/response data leaks information between users
- ❌ Non-thread-safe collections cause data corruption

These analyzers **prevent common mistakes** before they reach production, with **26 diagnostic rules** that catch:
- 🛡️ **Thread safety violations** (mutable state, non-thread-safe collections, static fields)
- 🔐 **Data leakage** (request/response storage, context capture, singleton state)
- 💉 **DI anti-patterns** (captive dependencies, IServiceProvider storage)
- ⚡ **Async/await issues** (ConfigureAwait misuse, blocking calls, missing cancellation)
- 🐛 **Common bugs** (async void, empty catch blocks, manual thread creation)

---

## 📋 Diagnostic Rules

### 🔴 Errors (Build fails - must fix!)
- **FlowT002**: Non-thread-safe collections in fields
- **FlowT003**: Captive scoped dependencies (DbContext, HttpContext, etc.)
- **FlowT004**: Static mutable state
- **FlowT006**: FlowContext stored in field
- **FlowT007**: Request/Response objects in fields
- **FlowT010**: Thread.Sleep() in async methods (thread pool starvation)
- **FlowT011**: FlowDefinition Configure() missing .Handle<T>() ✨ NEW
- **FlowT012**: IServiceProvider stored in field
- **FlowT013**: CancellationTokenSource stored in field
- **FlowT015**: Mutable public/internal properties
- **FlowT018**: Lazy<T> without explicit thread-safety mode
- **FlowT019**: Potential state leak in singleton (StringBuilder, Stream, etc.)
- **FlowT021**: FlowPlugin stored in singleton field ✨ NEW
- **FlowT022**: Multiple .Handle<T>() calls in Configure() ✨ NEW

### ⚠️ Warnings (Should fix)
- **FlowT001**: Mutable instance fields (race condition risk)
- **FlowT005**: Async void methods (unhandled exceptions)
- **FlowT008**: Lock on `this` or `typeof(T)` (deadlock risk)
- **FlowT010**: Synchronous blocking (.Result, .Wait(), .GetResult())
- **FlowT016**: Task/ValueTask stored in field
- **FlowT017**: Manual Thread creation (new Thread())
- **FlowT020**: ConfigureAwait(false) can lose context
- **FlowT023**: new HttpClient() in flow component ✨ NEW
- **FlowT024**: Synchronous file I/O in async flow method ✨ NEW

### ℹ️ Info (Suggestions)
- **FlowT009**: Missing CancellationToken propagation
- **FlowT014**: Empty catch blocks
- **FlowT025**: Direct IServiceProvider access (prefer context.Service<T>()) ✨ NEW

---

## 📖 Detailed Rules

### FlowT001: Mutable Field in Singleton Component ⚠️ Warning

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private int _counter = 0; // ❌ Race condition!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _counter++; // Shared between requests!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly ILogger _logger; // ✅ Readonly - OK

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        int counter = ctx.Get<int>("counter") + 1; // ✅ Per-request data
        ctx.Set(counter, "counter");
        return new Response();
    }
}
```

---

### FlowT002: Non-Thread-Safe Collection 🔴 Error

**Problem:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    private List<string> _cache = new(); // ❌ Not thread-safe!
    private Dictionary<int, User> _users = new(); // ❌ Race conditions!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _cache.Add(req.Data); // ❌ Concurrent modification!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    private readonly ConcurrentBag<string> _cache = new(); // ✅ Thread-safe
    private readonly ConcurrentDictionary<int, User> _users = new(); // ✅ Thread-safe

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _cache.Add(req.Data); // ✅ Safe
        return new Response();
    }
}
```

**Detected types:**
- `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`
- `Queue<T>`, `Stack<T>`, `LinkedList<T>`
- `SortedList<T>`, `SortedSet<T>`, `SortedDictionary<K,V>`

---

### FlowT003: Captive Scoped Dependency 🔴 Error

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly DbContext _db; // ❌ DbContext is scoped!
    
    public UserHandler(DbContext db) // ❌ Will be disposed after first request
    {
        _db = db;
    }
    
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        await _db.Users.ToListAsync(); // ❌ ObjectDisposedException!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Resolve scoped service per-request
        var db = ctx.Service<DbContext>();
        await db.Users.ToListAsync(); // ✅ Works correctly!
        return new Response();
    }
}
```

**Detected scoped types:**
- `DbContext` (and all derived types)
- `IHttpContextAccessor`
- `HttpContext`, `HttpRequest`, `HttpResponse`
- `SignInManager`, `UserManager`

---

### FlowT004: Static Mutable Field 🔴 Error

**Problem:**
```csharp
public class CounterHandler : IFlowHandler<Request, Response>
{
    private static int _globalCounter = 0; // ❌ Shared across ALL instances!
    private static List<User> _allUsers = new(); // ❌ Global mutable state!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _globalCounter++; // ❌ Race condition across entire app!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class CounterHandler : IFlowHandler<Request, Response>
{
    // ✅ Static readonly is safe
    private static readonly ImmutableList<User> _defaults = ImmutableList<User>.Empty;

    // ✅ Thread-safe static mutable (if really needed)
    private static readonly ConcurrentDictionary<int, User> _cache = new();

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _cache.TryAdd(req.Id, req.User); // ✅ Thread-safe
        return new Response();
    }
}
```

---

### FlowT006: FlowContext Stored in Field 🔴 Error

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private FlowContext? _context; // ❌ Will leak between requests!
    
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _context = ctx; // ❌ Next request sees previous context!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Use context only as method parameter
        ctx.Set("key", "value");
        return new Response();
    }
}
```

---

### FlowT005: Async Void Method ⚠️ Warning

**Problem:**
```csharp
public class NotificationHandler : IFlowHandler<Request, Response>
{
    // ❌ Exceptions cannot be caught, app will crash!
    public async void SendEmail(string to, string body)
    {
        await _emailService.SendAsync(to, body);
        throw new Exception("Boom!"); // ❌ Unhandled, crashes app!
    }

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        SendEmail(req.Email, req.Body); // ❌ Fire-and-forget, no error handling
        return new Response();
    }
}
```

**Solution:**
```csharp
public class NotificationHandler : IFlowHandler<Request, Response>
{
    // ✅ Return Task so exceptions can be handled
    public async Task SendEmailAsync(string to, string body)
    {
        await _emailService.SendAsync(to, body);
    }

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Option 1: Await it
        await SendEmailAsync(req.Email, req.Body);

        // ✅ Option 2: Background with proper error handling
        context.PublishInBackground(new SendEmailEvent(req.Email, req.Body));

        return new Response();
    }
}
```

---

### FlowT007: Request/Response Object in Field 🔴 Error

**Problem:**
```csharp
public class UserHandler : IFlowHandler<CreateUserRequest, UserResponse>
{
    private readonly CreateUserRequest? _lastRequest; // ❌ Request leaks between calls!
    private UserResponse? _cachedResponse; // ❌ Response shared across requests!

    public async ValueTask<UserResponse> HandleAsync(CreateUserRequest req, FlowContext ctx)
    {
        _lastRequest = req; // ❌ Next request sees this data!
        return new UserResponse(req.Name);
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<CreateUserRequest, UserResponse>
{
    // ✅ Keep request/response as local variables or parameters only
    public async ValueTask<UserResponse> HandleAsync(CreateUserRequest req, FlowContext ctx)
    {
        // ✅ Store in per-request context if needed
        ctx.Set("lastRequest", req);

        var response = new UserResponse(req.Name);
        return response; // ✅ No field storage
    }
}
```

---

### FlowT008: Lock on This or Type ⚠️ Warning

**Problem:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    private int _counter = 0;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        lock (this) // ❌ External code can lock on this too → deadlock!
        {
            _counter++;
        }

        lock (typeof(CacheHandler)) // ❌ Any code can lock this type → deadlock!
        {
            _counter--;
        }

        return new Response();
    }
}
```

**Solution:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    private readonly object _lock = new(); // ✅ Private lock object
    private int _counter = 0;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        lock (_lock) // ✅ Only this class can lock on _lock
        {
            _counter++;
        }

        return new Response();
    }
}
```

---

### FlowT009: Missing CancellationToken Propagation ℹ️ Info

**Problem:**
```csharp
public class DataHandler : IFlowHandler<Request, Response>
{
    private readonly HttpClient _http;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ❌ Method has CT parameter but we don't pass it
        var data = await _http.GetStringAsync(req.Url);

        // ❌ Operation won't be cancelled when request is aborted
        await Task.Delay(1000);

        return new Response(data);
    }
}
```

**Solution:**
```csharp
public class DataHandler : IFlowHandler<Request, Response>
{
    private readonly HttpClient _http;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Pass cancellation token
        var data = await _http.GetStringAsync(req.Url, ctx.CancellationToken);

        // ✅ Respect cancellation
        await Task.Delay(1000, ctx.CancellationToken);

        return new Response(data);
    }
}
```

---

### FlowT010: Synchronous Blocking in Async Method ⚠️ Warning

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ❌ All of these block the thread → deadlock risk!
        var user = GetUserAsync().Result; // ❌ Blocks thread

        SaveUserAsync(user).Wait(); // ❌ Blocks thread

        var id = CreateUserAsync(user).GetAwaiter().GetResult(); // ❌ Blocks thread

        return new Response();
    }

    private async Task<User> GetUserAsync() => await Task.FromResult(new User());
    private async Task SaveUserAsync(User u) => await Task.CompletedTask;
    private async Task<int> CreateUserAsync(User u) => await Task.FromResult(1);
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Use await instead of blocking
        var user = await GetUserAsync();

        await SaveUserAsync(user);

        var id = await CreateUserAsync(user);

        return new Response();
    }

    private async Task<User> GetUserAsync() => await Task.FromResult(new User());
    private async Task SaveUserAsync(User u) => await Task.CompletedTask;
    private async Task<int> CreateUserAsync(User u) => await Task.FromResult(1);
}
```

---

### FlowT014: Empty Catch Block ℹ️ Info

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        try
        {
            await SomeOperationAsync();
        }
        catch
        {
            // ❌ Silently swallows ALL exceptions!
            // Bug might go unnoticed in production
        }

        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly ILogger _logger;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        try
        {
            await SomeOperationAsync();
        }
        catch (Exception ex)
        {
            // ✅ Log the error
            _logger.LogError(ex, "Operation failed");

            // ✅ Or re-throw
            throw;

            // ✅ Or handle gracefully with fallback
            return new Response { Success = false, Error = ex.Message };
        }

        return new Response { Success = true };
    }
}
```

---

### FlowT012: IServiceProvider Stored in Field 🔴 Error

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly IServiceProvider _services; // ❌ Will capture root provider!

    public UserHandler(IServiceProvider services)
    {
        _services = services; // ❌ Not scoped per-request!
    }

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var db = _services.GetRequiredService<DbContext>(); // ❌ Wrong scope!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Get scoped provider per-request
        var db = ctx.Service<DbContext>();
        return new Response();
    }
}
```

---

## 🔧 Configuration

Create `.editorconfig` in your project root:

```ini
# FlowT analyzer configuration

# FlowT001: Mutable fields (Warning by default)
dotnet_diagnostic.FlowT001.severity = warning

# FlowT002: Non-thread-safe collections (Error - must fix!)
dotnet_diagnostic.FlowT002.severity = error

# FlowT003: Captive scoped dependencies (Error - must fix!)
dotnet_diagnostic.FlowT003.severity = error

# FlowT004: Static mutable state (Error - must fix!)
dotnet_diagnostic.FlowT004.severity = error

# FlowT005: Async void methods (Warning)
dotnet_diagnostic.FlowT005.severity = warning

# FlowT006: FlowContext in fields (Error - must fix!)
dotnet_diagnostic.FlowT006.severity = error

# FlowT007: Request/Response in fields (Error - must fix!)
dotnet_diagnostic.FlowT007.severity = error

# FlowT008: Lock on this/type (Warning)
dotnet_diagnostic.FlowT008.severity = warning

# FlowT009: Missing CancellationToken (Info - suggestion)
dotnet_diagnostic.FlowT009.severity = suggestion

# FlowT010: Synchronous blocking (Warning)
dotnet_diagnostic.FlowT010.severity = warning

# FlowT011: Missing .Handle<T>() in Configure() (Error - must fix!) ✨ NEW
dotnet_diagnostic.FlowT011.severity = error

# FlowT012: IServiceProvider in fields (Error - must fix!)
dotnet_diagnostic.FlowT012.severity = error

# FlowT013: CancellationTokenSource in fields (Error - must fix!)
dotnet_diagnostic.FlowT013.severity = error

# FlowT014: Empty catch blocks (Info - suggestion)
dotnet_diagnostic.FlowT014.severity = suggestion

# FlowT015: Mutable public/internal properties (Error - must fix!)
dotnet_diagnostic.FlowT015.severity = error

# FlowT016: Task/ValueTask storage (Warning)
dotnet_diagnostic.FlowT016.severity = warning

# FlowT017: Manual thread creation (Warning)
dotnet_diagnostic.FlowT017.severity = warning

# FlowT018: Lazy without thread-safety mode (Error - must fix!)
dotnet_diagnostic.FlowT018.severity = error

# FlowT019: Potential state leak in singleton (Error - must fix!)
dotnet_diagnostic.FlowT019.severity = error

# FlowT020: ConfigureAwait(false) can lose context (Warning)
dotnet_diagnostic.FlowT020.severity = warning

# FlowT021: FlowPlugin stored in singleton field (Error - must fix!) ✨ NEW
dotnet_diagnostic.FlowT021.severity = error

# FlowT022: Multiple .Handle<T>() calls in Configure() (Error - must fix!) ✨ NEW
dotnet_diagnostic.FlowT022.severity = error

# FlowT023: new HttpClient() in flow component (Warning) ✨ NEW
dotnet_diagnostic.FlowT023.severity = warning

# FlowT024: Synchronous file I/O in async method (Warning) ✨ NEW
dotnet_diagnostic.FlowT024.severity = warning

# FlowT025: Direct IServiceProvider access (Info - suggestion) ✨ NEW
dotnet_diagnostic.FlowT025.severity = suggestion

# Exclude test projects
[**/*Tests/**/*.cs]
dotnet_diagnostic.FlowT001.severity = none
dotnet_diagnostic.FlowT002.severity = none
dotnet_diagnostic.FlowT003.severity = none
dotnet_diagnostic.FlowT004.severity = none
dotnet_diagnostic.FlowT006.severity = none
dotnet_diagnostic.FlowT007.severity = none
dotnet_diagnostic.FlowT012.severity = none
dotnet_diagnostic.FlowT013.severity = none
dotnet_diagnostic.FlowT015.severity = none
dotnet_diagnostic.FlowT018.severity = none
dotnet_diagnostic.FlowT019.severity = none
dotnet_diagnostic.FlowT021.severity = none
```

---

## 📦 Installation

FlowT.Analyzers is automatically included when you reference FlowT:

```xml
<ItemGroup>
  <PackageReference Include="FlowT" Version="1.0.0" />
  <!-- FlowT.Analyzers is included automatically -->
</ItemGroup>
```

Or install separately:

```xml
<ItemGroup>
  <PackageReference Include="FlowT.Analyzers" Version="1.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

---

### FlowT013: CancellationTokenSource Stored in Field 🔴 Error

**Problem:**
```csharp
public class OperationHandler : IFlowHandler<Request, Response>
{
    private readonly CancellationTokenSource _cts = new(); // ❌ Shared between requests!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ❌ First request calls Cancel() → ALL requests get cancelled!
        await Task.Delay(1000, _cts.Token);
        _cts.Cancel(); // ❌ Affects ALL concurrent requests!

        return new Response();
    }
}
```

**Solution:**
```csharp
public class OperationHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Create per-request CancellationTokenSource
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        await Task.Delay(1000, cts.Token); // ✅ Isolated per request

        return new Response();
    }
}
```

---

### FlowT015: Mutable Public/Internal Property 🔴 Error

**Problem:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    public List<User> Users { get; set; } = new(); // ❌ External code can modify!
    internal int Counter { get; set; } // ❌ Race condition!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        Counter++; // ❌ Not thread-safe
        Users.Add(req.User); // ❌ Not thread-safe
        return new Response();
    }
}

// External code:
var handler = serviceProvider.GetRequiredService<CacheHandler>();
handler.Users.Clear(); // ❌ Modifies shared state!
```

**Solution:**
```csharp
public class CacheHandler : IFlowHandler<Request, Response>
{
    // ✅ Option 1: Private mutable with thread-safe collection
    private readonly ConcurrentBag<User> _users = new();

    // ✅ Option 2: Public readonly (immutable)
    public IReadOnlyList<User> Users => _users.ToArray();

    // ✅ Option 3: Init-only setter (set once during construction)
    public int MaxSize { get; init; } = 100;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _users.Add(req.User); // ✅ Thread-safe
        return new Response();
    }
}
```

---

### FlowT016: Task/ValueTask Stored in Field ⚠️ Warning

**Problem:**
```csharp
public class DataHandler : IFlowHandler<Request, Response>
{
    private Task<User>? _cachedUserTask; // ⚠️ Shared between requests!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ⚠️ First request starts task, second request awaits same task!
        _cachedUserTask ??= GetUserAsync(req.UserId);

        var user = await _cachedUserTask; // ⚠️ Wrong user for second request!
        return new Response(user);
    }
}
```

**Solution:**
```csharp
public class DataHandler : IFlowHandler<Request, Response>
{
    // ✅ Option 1: Create task per-request
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var userTask = GetUserAsync(req.UserId); // ✅ New task per request
        var user = await userTask;
        return new Response(user);
    }

    // ✅ Option 2: Cache result, not Task
    private readonly ConcurrentDictionary<int, User> _userCache = new();

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var user = await _userCache.GetOrAddAsync(req.UserId, async id =>
        {
            return await GetUserAsync(id);
        });

        return new Response(user);
    }
}
```

---

### FlowT017: Manual Thread Creation ⚠️ Warning

**Problem:**
```csharp
public class BackgroundHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ⚠️ Creates new OS thread (~1MB stack per thread!)
        var thread = new Thread(() =>
        {
            DoBackgroundWork(); // ⚠️ Not cancellable, no error handling
        });
        thread.Start();

        return new Response();
    }
}
```

**Solution:**
```csharp
public class BackgroundHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Use thread pool (efficient, managed)
        _ = Task.Run(async () =>
        {
            await DoBackgroundWorkAsync(ctx.CancellationToken);
        }, ctx.CancellationToken);

        // ✅ Or use FlowT's background events
        ctx.PublishInBackground(new BackgroundWorkEvent());

        return new Response();
    }
}
```

---

### FlowT018: Lazy<T> Without Thread-Safety Mode 🔴 Error

**Problem:**
```csharp
public class ConfigHandler : IFlowHandler<Request, Response>
{
    // ❌ No explicit thread-safety mode!
    private readonly Lazy<Config> _config = new Lazy<Config>(() => LoadConfig());

    // ⚠️ What if someone accidentally uses LazyThreadSafetyMode.None?
    private readonly Lazy<Settings> _settings = new Lazy<Settings>(() => LoadSettings(), 
        LazyThreadSafetyMode.None); // ❌ Not thread-safe!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var config = _config.Value; // ❌ Implicit thread-safety - unclear intent
        return new Response();
    }
}
```

**Solution:**
```csharp
public class ConfigHandler : IFlowHandler<Request, Response>
{
    // ✅ Explicit thread-safe mode
    private readonly Lazy<Config> _config = new Lazy<Config>(
        () => LoadConfig(), 
        LazyThreadSafetyMode.ExecutionAndPublication); // ✅ Thread-safe, clear intent

    // ✅ Or use PublicationOnly if multiple executions are acceptable
    private readonly Lazy<Settings> _settings = new Lazy<Settings>(
        () => LoadSettings(), 
        LazyThreadSafetyMode.PublicationOnly); // ✅ Thread-safe

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var config = _config.Value; // ✅ Safe, clear thread-safety guarantee
        return new Response();
    }
}
```

**Modes explained:**
- `ExecutionAndPublication` - Only one thread executes factory, others wait (default, but should be explicit)
- `PublicationOnly` - Multiple threads may execute factory, first published value wins
- `None` - ❌ **NOT thread-safe** - never use in singleton components!

---

### FlowT019: Potential State Leak in Singleton 🔴 Error ✨ NEW

**Problem:**
```csharp
public class LogHandler : IFlowHandler<Request, Response>
{
    private readonly StringBuilder _log = new(); // ❌ Shared between ALL requests!
    private MemoryStream? _currentStream; // ❌ Leaks data between users!
    private Stopwatch _timer = Stopwatch.StartNew(); // ❌ Shared timer!

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _log.AppendLine($"User {req.UserId}"); // ❌ User A sees User B's logs!

        _currentStream = new MemoryStream(); // ❌ Request 1 overwrites Request 2's stream!

        var elapsed = _timer.Elapsed; // ❌ Wrong timing - includes ALL requests!

        return new Response();
    }
}
```

**Solution:**
```csharp
public class LogHandler : IFlowHandler<Request, Response>
{
    // ✅ Option 1: Create per-request instances
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var log = new StringBuilder(); // ✅ Local variable
        log.AppendLine($"User {req.UserId}");

        using var stream = new MemoryStream(); // ✅ Per-request stream

        var timer = Stopwatch.StartNew(); // ✅ Per-request timer
        // ... work ...
        timer.Stop();

        return new Response();
    }

    // ✅ Option 2: Use thread-safe alternatives for caching
    private readonly ConcurrentDictionary<int, string> _logCache = new();

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _logCache.TryAdd(req.UserId, $"User {req.UserId}"); // ✅ Thread-safe cache
        return new Response();
    }

    // ✅ Option 3: Store in FlowContext for per-request state
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var log = ctx.GetOrAdd("log", () => new StringBuilder());
        log.AppendLine($"User {req.UserId}"); // ✅ Isolated per request
        return new Response();
    }
}
```

**Detected types:**
- `StringBuilder` - Mutable string builder (not thread-safe)
- `MemoryStream`, `StreamWriter`, `StreamReader` - I/O streams (stateful)
- `Stopwatch` - Timer with mutable state
- `Random` - Not thread-safe, produces correlated sequences
- `Timer` - Callback timer (stateful)
- Arrays (`T[]`) - Always mutable
- `HttpClient` with per-request config - Should be static readonly

---

### FlowT011: FlowDefinition Configure() Missing .Handle<T>() 🔴 Error ✨ NEW

**Problem:**
```csharp
[FlowDefinition]
public class CreateUserFlow : FlowDefinition<CreateUserRequest, UserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, UserResponse> flow)
    {
        flow
            .Check<ValidateEmailSpec>()  // ❌ Only specs and policies — no handler!
            .Use<LoggingPolicy>();
        // ❌ Missing .Handle<T>() — flow has no business logic!
    }
}
```

**Solution:**
```csharp
[FlowDefinition]
public class CreateUserFlow : FlowDefinition<CreateUserRequest, UserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, UserResponse> flow)
    {
        flow
            .Check<ValidateEmailSpec>()
            .Use<LoggingPolicy>()
            .Handle<CreateUserHandler>(); // ✅ Terminal handler required
    }
}
```

---

### FlowT020: ConfigureAwait(false) Can Lose Context ⚠️ Warning

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ❌ Lose SynchronizationContext → HttpContext becomes null!
        await Task.Delay(100).ConfigureAwait(false);

        var httpContext = _httpContextAccessor.HttpContext; // ❌ NULL!
        var userId = ctx.Get<int>("userId"); // ❌ May throw - wrong context!

        // ❌ External API call loses context
        await _apiClient.GetAsync("https://api.example.com/data")
            .ConfigureAwait(false);

        var user = ctx.Service<IUserService>(); // ❌ May be wrong scope!

        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ✅ Default behavior captures context
        await Task.Delay(100); // ConfigureAwait(true) is default

        var httpContext = _httpContextAccessor.HttpContext; // ✅ Available
        var userId = ctx.Get<int>("userId"); // ✅ Correct context

        // ✅ Explicit ConfigureAwait(true) for clarity
        await _apiClient.GetAsync("https://api.example.com/data")
            .ConfigureAwait(true);

        var user = ctx.Service<IUserService>(); // ✅ Correct scope

        return new Response();
    }
}
```

**Why this matters:**
- **ASP.NET Core** uses `SynchronizationContext` to flow `HttpContext`, `FlowContext`, and other ambient state
- **ConfigureAwait(false)** prevents capturing context → you lose access to request-scoped data
- **ConfigureAwait(true)** (default) preserves context → everything works correctly
- **Library code** can use `ConfigureAwait(false)`, but **application code** (like FlowT handlers) should use `true`

**When ConfigureAwait(false) is safe:**
```csharp
// ✅ In pure computational code that doesn't need context
private async Task<int> CalculateSumAsync(int[] numbers)
{
    int sum = 0;
    foreach (var num in numbers)
    {
        await Task.Yield().ConfigureAwait(false); // ✅ No context needed
        sum += num;
    }
    return sum;
}
```

---

### FlowT021: FlowPlugin Stored in Singleton Field 🔴 Error ✨ NEW

**Problem:**
```csharp
public class AuditHandler : IFlowHandler<Request, Response>
{
    private readonly AuditPlugin _audit; // ❌ PerFlow plugin captured in singleton!

    public AuditHandler(AuditPlugin audit) { _audit = audit; }

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        _audit.Record("started"); // ❌ Wrong Context — plugin holds previous request's context!
        return new Response();
    }
}
```

**Solution:**
```csharp
public class AuditHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        ctx.Plugin<IAuditPlugin>().Record("started"); // ✅ PerFlow instance, correct context
        return new Response();
    }
}
```

---

### FlowT022: Multiple .Handle<T>() Calls in Configure() 🔴 Error ✨ NEW

**Problem:**
```csharp
[FlowDefinition]
public class BadFlow : FlowDefinition<Request, Response>
{
    protected override void Configure(IFlowBuilder<Request, Response> flow)
    {
        flow
            .Handle<FirstHandler>()   // ✅ First handler
            .Handle<SecondHandler>(); // ❌ FlowT022: only one handler allowed!
    }
}
```

**Solution:**
```csharp
// ✅ Split into separate flows
[FlowDefinition]
public class FirstFlow : FlowDefinition<Request, Response>
{
    protected override void Configure(IFlowBuilder<Request, Response> flow)
        => flow.Handle<FirstHandler>();
}

[FlowDefinition]
public class SecondFlow : FlowDefinition<Request, Response>
{
    protected override void Configure(IFlowBuilder<Request, Response> flow)
        => flow.Handle<SecondHandler>();
}
```

---

### FlowT023: HttpClient Instantiated Directly in Flow Component ⚠️ Warning ✨ NEW

**Problem:**
```csharp
public class ApiHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        using var client = new HttpClient(); // ❌ Socket exhaustion risk!
        var data = await client.GetStringAsync(req.Url);
        return new Response(data);
    }
}
```

**Solution:**
```csharp
public class ApiHandler : IFlowHandler<Request, Response>
{
    private readonly IHttpClientFactory _factory;

    public ApiHandler(IHttpClientFactory factory) => _factory = factory;

    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        using var client = _factory.CreateClient(); // ✅ Pooled handler
        var data = await client.GetStringAsync(req.Url);
        return new Response(data);
    }
}
```

---

### FlowT024: Synchronous File I/O in Async Flow Method ⚠️ Warning ✨ NEW

**Problem:**
```csharp
public class ReportHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var content = File.ReadAllText(req.Path);      // ❌ Blocks thread pool!
        File.WriteAllText(req.OutputPath, content);   // ❌ Blocks thread pool!
        return new Response(content);
    }
}
```

**Solution:**
```csharp
public class ReportHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var content = await File.ReadAllTextAsync(req.Path, ctx.CancellationToken);     // ✅
        await File.WriteAllTextAsync(req.OutputPath, content, ctx.CancellationToken);  // ✅
        return new Response(content);
    }
}
```

**Detected methods:** `ReadAllText`, `ReadAllBytes`, `ReadAllLines`, `WriteAllText`, `WriteAllBytes`, `WriteAllLines`, `AppendAllText`, `AppendAllLines`

---

### FlowT025: Direct IServiceProvider Access in Flow Component ℹ️ Info ✨ NEW

**Problem:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        // ℹ️ Service locator pattern — harder to test and read
        var db = ctx.Services.GetRequiredService<AppDbContext>();
        var repo = ctx.Services.GetService<IUserRepository>();
        return new Response();
    }
}
```

**Solution:**
```csharp
public class UserHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var db = ctx.Service<AppDbContext>();          // ✅ Clean, testable
        var repo = ctx.TryService<IUserRepository>(); // ✅ Returns null if not registered
        return new Response();
    }
}
```

---

## 📊 Complete Analyzer Summary

### Singleton Safety (Prevent Data Leaks)
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT001 | ⚠️ Warning | Mutable instance fields (race condition risk) |
| FlowT002 | 🔴 Error | Non-thread-safe collections (List, Dictionary, etc.) |
| FlowT004 | 🔴 Error | Static mutable state (global data corruption) |
| FlowT006 | 🔴 Error | FlowContext stored in field (context leak) |
| FlowT007 | 🔴 Error | Request/Response objects in fields (user data leak) |
| FlowT015 | 🔴 Error | Mutable public/internal properties (external modification) |
| FlowT019 | 🔴 Error | Potential state leak (StringBuilder, Stream, Stopwatch, etc.) |
| FlowT021 | 🔴 Error | **NEW** - FlowPlugin stored in singleton field (PerFlow lifetime violation) |

### Flow Configuration
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT011 | 🔴 Error | **NEW** - Configure() missing .Handle<T>() (no business logic) |
| FlowT022 | 🔴 Error | **NEW** - Multiple .Handle<T>() calls (only one handler allowed) |

### Dependency Injection (Prevent Captive Dependencies)
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT003 | 🔴 Error | Captive scoped dependencies (DbContext, HttpContext) |
| FlowT012 | 🔴 Error | IServiceProvider stored in field (wrong scope) |
| FlowT013 | 🔴 Error | CancellationTokenSource stored in field (shared cancellation) |
| FlowT025 | ℹ️ Info | **NEW** - Direct IServiceProvider access (prefer context.Service<T>()) |

### Async/Await Patterns
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT005 | ⚠️ Warning | Async void methods (unhandled exceptions) |
| FlowT009 | ℹ️ Info | Missing CancellationToken propagation (can't cancel) |
| FlowT010 | ⚠️ Warning | Synchronous blocking (.Result, .Wait(), .GetResult()) |
| FlowT016 | ⚠️ Warning | Task/ValueTask stored in field (shared task) |
| FlowT020 | ⚠️ Warning | ConfigureAwait(false) loses context (HttpContext null) |
| FlowT024 | ⚠️ Warning | **NEW** - Synchronous file I/O in async method (thread pool block) |

### Threading & Concurrency
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT008 | ⚠️ Warning | Lock on this/typeof (external deadlock risk) |
| FlowT017 | ⚠️ Warning | Manual Thread creation (use Task.Run instead) |
| FlowT018 | 🔴 Error | Lazy<T> without thread-safety mode (implicit behavior) |

### Best Practices
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT023 | ⚠️ Warning | **NEW** - new HttpClient() in flow component (socket exhaustion) |

### Code Quality
| Rule | Severity | Description |
|------|----------|-------------|
| FlowT014 | ℹ️ Info | Empty catch blocks (silent failures) |

**Summary: 26 rules** - 14 Errors, 9 Warnings, 3 Info

---

## 🚀 Build Experience

**In Visual Studio:**
```
UserHandler.cs(12,17): error FlowT003: Flow component 'UserHandler' captures scoped service 'DbContext' in constructor. Use 'context.Service<DbContext>()' instead
    
💡 Quick Fix: Replace with context.Service<T>()
```

**In CLI:**
```sh
$ dotnet build
...
error FlowT003: Flow component 'UserHandler' captures scoped service 'DbContext' [MyProject.csproj]

Build FAILED.
    1 Error(s)
```

---

## 🛡️ Why These Rules?

### Thread Safety (Rules: FlowT001, FlowT002, FlowT004, FlowT008, FlowT017, FlowT018, FlowT019)
Singleton components are **shared between concurrent requests**. Any mutable state = race conditions and data corruption.

**Real-world impact:**
- User A's shopping cart shows User B's items
- Counter shows wrong value (lost increments)
- Cache returns stale/corrupted data
- **Production bug: Hard to reproduce, only under load**

### Memory & Data Safety (Rules: FlowT006, FlowT007, FlowT015)
FlowContext and Request/Response are **per-request**. Storing them in fields = data leaks between users.

**Real-world impact:**
- User A sees User B's personal information
- Previous request's authentication leaks to next request
- GDPR violation - sensitive data visible to wrong users
- **Security issue: Exploitable data leak**

### Dependency Injection (Rules: FlowT003, FlowT012, FlowT013)
Scoped services are **disposed after each request**. Capturing them in singletons = `ObjectDisposedException`.

**Real-world impact:**
- DbContext is disposed after first request → crash on second request
- HttpContext is null after first request → NullReferenceException
- CancellationTokenSource cancelled once → all requests fail
- **Production crash: "Cannot access a disposed object"**

### Async/Await Correctness (Rules: FlowT005, FlowT009, FlowT010, FlowT016, FlowT020)
Incorrect async patterns cause deadlocks, thread pool starvation, and context loss.

**Real-world impact:**
- ConfigureAwait(false) → HttpContext becomes null
- .Wait() or .Result → deadlock in ASP.NET Core
- Async void → unhandled exceptions crash application
- Missing CancellationToken → operations continue after user cancels
- **Production deadlock: All requests hang, CPU at 100%**

---

## 📚 Learn More

- [FlowT Documentation](https://github.com/vlasta81/FlowT)
- [Best Practices](../docs/BEST_PRACTICES.md)
- [Thread Safety Guide](../docs/THREAD_SAFETY.md)
