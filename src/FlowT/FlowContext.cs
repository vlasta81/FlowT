using FlowT.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT
{
    /// <summary>
    /// Represents the shared execution context for a flow and all its sub-flows.
    /// Provides access to dependency injection, cancellation, shared state storage, event publishing, and timing utilities.
    /// One instance is created per flow execution and passed through the entire pipeline chain.
    /// </summary>
    /// <remarks>
    /// The context uses a type-keyed dictionary for storing arbitrary data.
    /// All operations on <see cref="Set{T}"/>, <see cref="TryGet{T}"/>,
    /// and <see cref="GetOrAdd{T}(Func{T},string)"/> are thread-safe and protected by an internal lock.
    /// Timer values recorded via <see cref="StartTimer"/> are stored in a dedicated dictionary separate from general-purpose storage.
    /// </remarks>
    public sealed class FlowContext
    {
        /// <summary>
        /// Gets the service provider for resolving dependencies.
        /// </summary>
        public required IServiceProvider Services { get; init; }

        /// <summary>
        /// Gets the cancellation token for this flow execution.
        /// Use this to observe cancellation requests from the client or timeout policies.
        /// </summary>
        public required CancellationToken CancellationToken { get; init; }

        /// <summary>
        /// Gets the HTTP context for this flow execution, if available.
        /// This is <c>null</c> for non-HTTP scenarios (background jobs, console apps, Blazor WebAssembly, message queue handlers, unit tests).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Having access to <see cref="HttpContext"/> allows handlers to:
        /// </para>
        /// <list type="bullet">
        /// <item>Read HTTP request metadata (headers, query params, cookies, path, body)</item>
        /// <item>Access authenticated user (<see cref="HttpContext.User"/>, claims, identity)</item>
        /// <item>Set HTTP response metadata (status codes, headers, cookies)</item>
        /// <item>Access connection info (IP addresses, ports, certificates)</item>
        /// <item>Use per-request storage (<see cref="HttpContext.Items"/>)</item>
        /// <item>Access low-level features (<see cref="HttpContext.Features"/>)</item>
        /// </list>
        /// 
        /// <para><strong>⚠️ WARNING - Response Body Access:</strong></para>
        /// <para>
        /// <strong>DO NOT write directly to <see cref="HttpContext.Response.Body"/>!</strong>
        /// FlowT flows return typed responses (<typeparamref name="TResponse"/>) which are automatically serialized by ASP.NET Core.
        /// Writing to <see cref="HttpContext.Response.Body"/> will cause:
        /// </para>
        /// <list type="bullet">
        /// <item><strong>Double serialization</strong> - Both your write and FlowT's response will be sent</item>
        /// <item><strong>Malformed responses</strong> - Client receives corrupted/invalid data</item>
        /// <item><strong>Header conflicts</strong> - Cannot modify headers after body is written</item>
        /// </list>
        /// 
        /// <para><strong>✅ SAFE operations:</strong></para>
        /// <code>
        /// // ✅ Read from request
        /// var user = context.HttpContext.User;
        /// var header = context.HttpContext.Request.Headers["X-Custom"];
        /// var ip = context.HttpContext.Connection.RemoteIpAddress;
        /// 
        /// // ✅ Set response metadata (before returning)
        /// context.HttpContext.Response.StatusCode = 201;
        /// context.HttpContext.Response.Headers["Location"] = "/api/resource/123";
        /// context.HttpContext.Response.Cookies.Append("session", "xyz");
        /// </code>
        /// 
        /// <para><strong>❌ UNSAFE operations:</strong></para>
        /// <code>
        /// // ❌ DO NOT write to Response.Body
        /// await context.HttpContext.Response.Body.WriteAsync(data);
        /// await context.HttpContext.Response.WriteAsync("text");
        /// await context.HttpContext.Response.WriteAsJsonAsync(obj);
        /// </code>
        /// 
        /// <para><strong>Alternative for custom response handling:</strong></para>
        /// <para>
        /// If you need full control over the response (streaming, custom serialization, etc.), 
        /// use a custom middleware or endpoint instead of FlowT, or return an appropriate typed response 
        /// (e.g., <see cref="Microsoft.AspNetCore.Mvc.FileStreamResult"/>, <see cref="Microsoft.AspNetCore.Mvc.IResult"/>).
        /// </para>
        /// 
        /// <para>
        /// This does <strong>NOT</strong> violate singleton pattern as <see cref="HttpContext"/> is per-request scoped.
        /// </para>
        /// </remarks>
        public HttpContext? HttpContext { get; init; }

        /// <summary>
        /// Gets the unique identifier for this flow execution.
        /// This ID is shared across the entire flow execution tree (main flow and all sub-flows) for correlation.
        /// </summary>
        public Guid FlowId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets the flow ID as a string in format "N" (32 digits without hyphens).
        /// Useful for logging and correlation.
        /// </summary>
        public string FlowIdString => FlowId.ToString("N");

        /// <summary>
        /// Gets the UTC timestamp when this flow execution started.
        /// </summary>
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Resolves a required service from the dependency injection container.
        /// This is a convenience method for <see cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the service is not registered.</exception>
        /// <remarks>
        /// Use this method to resolve scoped services (e.g., DbContext) in singleton handlers.
        /// This ensures each request gets its own scoped service instance.
        /// Example: <c>var db = context.Service&lt;DbContext&gt;();</c>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Service<T>() where T : notnull => Services.GetRequiredService<T>();

        /// <summary>
        /// Attempts to resolve an optional service from the dependency injection container.
        /// Returns <c>null</c> if the service is not registered.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The resolved service instance, or <c>null</c> if not registered.</returns>
        /// <remarks>
        /// Use this method when a service is optional and you want to handle its absence gracefully.
        /// Example: <c>var cache = context.TryService&lt;ICache&gt;() ?? new NullCache();</c>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? TryService<T>() where T : class => Services.GetService<T>();

        /// <summary>
        /// Resolves a plugin registered for this flow context, creating and caching it on first access.
        /// The plugin instance is scoped to this <see cref="FlowContext"/> — one instance per flow execution.
        /// </summary>
        /// <typeparam name="T">The plugin interface type. Must be registered via <see cref="Extensions.FlowServiceCollectionExtensions.AddFlowPlugin{TPlugin,TImpl}"/>.</typeparam>
        /// <returns>The plugin instance for this flow execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the plugin type is not registered.</exception>
        /// <remarks>
        /// Plugins are always <strong>PerFlow</strong> — one instance shared across all pipeline stages
        /// (specifications, policies, handler) within the same flow execution.
        /// This enables plugins to accumulate state (metrics, trace spans, audit entries) across the entire pipeline.
        /// Use <see cref="Extensions.FlowServiceCollectionExtensions.AddFlowPlugin{TPlugin,TImpl}"/> in Program.cs or module registration.
        /// <para>
        /// If the plugin inherits from <see cref="Abstractions.FlowPlugin"/>, the <see cref="FlowContext"/> is bound
        /// automatically after creation via an internal call, giving the plugin full access to this context
        /// through its <c>protected Context</c> property.
        /// </para>
        /// <para>
        /// <strong>Thread safety:</strong> After the plugin is created, subsequent calls use a lockless read path for performance.
        /// The first-time creation is protected by a lock to prevent duplicate initialization.
        /// </para>
        /// <code>
        /// var metrics = context.Plugin&lt;IRequestMetrics&gt;();
        /// metrics.RecordDbQuery(elapsed);
        /// </code>
        /// </remarks>
        public T Plugin<T>() where T : class
        {
            if (_plugins is { } dict && dict.TryGetValue(typeof(T), out object? cached) && cached is T existing)
            {
                return existing;
            }
            lock (_syncLock)
            {
                _plugins ??= new Dictionary<Type, object?>();
                ref object? entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_plugins, typeof(T), out bool exists);
                if (!exists)
                {
                    T plugin = Services.GetRequiredService<T>();
                    if (plugin is Abstractions.FlowPlugin fp)
                        fp.Initialize(this);
                    entry = plugin;
                }
                return (T)entry!;
            }
        }

        private readonly Dictionary<CompositeKey, object?> _items = new();
        private Dictionary<Type, object?>? _plugins;
        private readonly Lock _syncLock = new();
        private Dictionary<string, long>? _timers;

        private static readonly CookieOptions _defaultCookieOptions = new();

        /// <summary>
        /// Stores a value in the context's shared state, keyed by its type and optional string key.
        /// This method is thread-safe and optimized using <see cref="CollectionsMarshal"/> for minimal allocations.
        /// </summary>
        /// <typeparam name="T">The type of value to store (used as part of the composite key).</typeparam>
        /// <param name="value">The value to store.</param>
        /// <param name="key">Optional string key to store multiple values of the same type under different keys.</param>
        /// <remarks>
        /// When <paramref name="key"/> is null, only one value of type <typeparamref name="T"/> can be stored.
        /// Use named keys to store multiple instances: Set(user1, "admin"), Set(user2, "guest").
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(T value, string? key = null)
        {
            CompositeKey composite = new CompositeKey(typeof(T), key);
            lock (_syncLock)
            {
                ref object? entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_items, composite, out _);
                entry = value;
            }
        }

        /// <summary>
        /// Attempts to retrieve a value from the context's shared state.
        /// This method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The type of value to retrieve.</typeparam>
        /// <param name="value">When this method returns, contains the value if found; otherwise, the default value for <typeparamref name="T"/>.</param>
        /// <param name="key">Optional string key to retrieve a specific named value of type <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// When <paramref name="key"/> is null, retrieves the default value stored for type <typeparamref name="T"/>.
        /// Use named keys to retrieve specific instances: TryGet(out user, "admin").
        /// </remarks>
        public bool TryGet<T>(out T value, string? key = null)
        {
            CompositeKey composite = new CompositeKey(typeof(T), key);
            lock (_syncLock)
            {
                if (_items.TryGetValue(composite, out var obj) && obj is T typed)
                {
                    value = typed;
                    return true;
                }
            }
            value = default!;
            return false;
        }

        /// <summary>
        /// Gets an existing value from the context or adds a new one using the provided factory.
        /// This method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The type of value to get or add.</typeparam>
        /// <param name="factory">A function to create the value if it doesn't exist.</param>
        /// <param name="key">Optional string key for storing multiple values of the same type.</param>
        /// <returns>The existing or newly created value.</returns>
        /// <remarks>
        /// When <paramref name="key"/> is null, uses the default key for type <typeparamref name="T"/>.
        /// Use named keys for multiple instances: GetOrAdd(() => new Cache(), "users").
        /// </remarks>
        public T GetOrAdd<T>(Func<T> factory, string? key = null)
        {
            CompositeKey composite = new CompositeKey(typeof(T), key);
            lock (_syncLock)
            {
                ref object? entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_items, composite, out bool exists);
                if (!exists)
                {
                    entry = factory();
                }
                return (T)entry!;
            }
        }

        /// <summary>
        /// Gets an existing value from the context or adds a new one using the provided factory with an argument.
        /// This overload avoids closure allocation when the factory needs a parameter.
        /// </summary>
        /// <typeparam name="T">The type of value to get or add.</typeparam>
        /// <typeparam name="TArg">The type of argument to pass to the factory.</typeparam>
        /// <param name="arg">The argument to pass to the factory if a new value needs to be created.</param>
        /// <param name="factory">A function to create the value if it doesn't exist.</param>
        /// <param name="key">Optional string key for storing multiple values of the same type.</param>
        /// <returns>The existing or newly created value.</returns>
        public T GetOrAdd<T, TArg>(TArg arg, Func<TArg, T> factory, string? key = null)
        {
            CompositeKey composite = new CompositeKey(typeof(T), key);
            lock (_syncLock)
            {
                ref object? entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_items, composite, out bool exists);
                if (!exists)
                {
                    entry = factory(arg);
                }
                return (T)entry!;
            }
        }

        /// <summary>
        /// Temporarily replaces a value in the context's shared state.
        /// Returns a disposable that restores the previous value (or removes the entry) when disposed.
        /// Useful for scoped overrides in nested flows or policies.
        /// </summary>
        /// <typeparam name="T">The type of value to push.</typeparam>
        /// <param name="value">The value to temporarily store.</param>
        /// <param name="key">Optional string key for storing multiple values of the same type.</param>
        /// <returns>A <see cref="ScopeReverter"/> that restores the original state when disposed.</returns>
        public ScopeReverter Push<T>(T value, string? key = null)
        {
            CompositeKey composite = new CompositeKey(typeof(T), key);
            lock (_syncLock)
            {
                ref object? entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_items, composite, out bool exists);
                object? old = entry;
                entry = value;
                return new ScopeReverter(this, composite, exists, old);
            }
        }

        /// <summary>
        /// Starts a high-precision timer for measuring elapsed time of an operation.
        /// Returns a disposable that records the elapsed time when disposed.
        /// Timers are stored in the context and can be retrieved for performance analysis.
        /// </summary>
        /// <param name="key">A unique string key to identify this timer.</param>
        /// <returns>A <see cref="TimerDisposable"/> that records elapsed time when disposed.</returns>
        public TimerDisposable StartTimer(string key) => new(this, key);

        /// <summary>
        /// Publishes a domain event asynchronously to all registered handlers.
        /// Handlers are resolved from the service provider and executed sequentially.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish.</typeparam>
        /// <param name="eventData">The event data to pass to handlers.</param>
        /// <param name="cancellationToken">A cancellation token to observe.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken)
        {
            foreach (IEventHandler<TEvent> handler in Services.GetServices<IEventHandler<TEvent>>())
            {
                await handler.HandleAsync(eventData, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Publishes a domain event in the background (fire-and-forget) without blocking the current flow.
        /// Use this for non-critical side effects that shouldn't delay the response.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish.</typeparam>
        /// <param name="eventData">The event data to pass to handlers.</param>
        /// <param name="cancellationToken">A cancellation token to observe.</param>
        /// <returns>A task representing the background operation. The returned task is typically not awaited by callers.</returns>
        /// <remarks>
        /// Any exception thrown by an event handler is caught and logged via <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>
        /// resolved from <see cref="Services"/>. If no logger factory is registered the exception is silently discarded.
        /// The background work is dispatched via <see cref="System.Threading.Tasks.Task.Run(System.Func{System.Threading.Tasks.Task?}, System.Threading.CancellationToken)"/>
        /// and respects the provided <paramref name="cancellationToken"/>.
        /// </remarks>
        public Task PublishInBackground<TEvent>(TEvent eventData, CancellationToken cancellationToken)
        {
            ILogger? logger = Services.GetService<ILoggerFactory>()
                ?.CreateLogger(nameof(FlowContext));
            Task work = Task.Run(() => PublishAsync(eventData, cancellationToken), cancellationToken);
            _ = work.ContinueWith(
                t => logger?.LogError(t.Exception, "Unhandled exception in background event handler for {EventType}.", typeof(TEvent).Name),
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            return work;
        }

        /// <summary>
        /// Throws an <see cref="OperationCanceledException"/> if cancellation has been requested.
        /// Convenience method for checking <see cref="CancellationToken"/>.
        /// </summary>
        public void ThrowIfCancellationRequested() => CancellationToken.ThrowIfCancellationRequested();

        /// <summary>
        /// Gets the authenticated user principal from the HTTP context.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios).
        /// </summary>
        /// <returns>The <see cref="ClaimsPrincipal"/> representing the authenticated user, or <c>null</c> if not available.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to <c>context.HttpContext?.User</c>.
        /// Use this to access user identity, claims, and role information in handlers.
        /// Example: <c>var userId = context.GetUser()?.FindFirst(ClaimTypes.NameIdentifier)?.Value;</c>
        /// </remarks>
        public ClaimsPrincipal? GetUser() => HttpContext?.User;

        /// <summary>
        /// Determines whether the current user is authenticated.
        /// Returns <c>false</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios).
        /// </summary>
        /// <returns><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to <c>context.HttpContext?.User?.Identity?.IsAuthenticated == true</c>.
        /// Use this to check if a user has successfully authenticated before accessing protected resources.
        /// Example: <c>if (!context.IsAuthenticated()) return FlowInterrupt.Abort(new UnauthorizedResponse());</c>
        /// </remarks>
        public bool IsAuthenticated() => HttpContext?.User?.Identity?.IsAuthenticated == true;

        /// <summary>
        /// Determines whether the current user belongs to the specified role.
        /// Returns <c>false</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios) or user is not in the role.
        /// </summary>
        /// <param name="role">The name of the role to check.</param>
        /// <returns><c>true</c> if the user is in the specified role; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to <c>context.HttpContext?.User?.IsInRole(role) == true</c>.
        /// Use this for role-based authorization in handlers.
        /// Example: <c>if (!context.IsInRole("Admin")) return FlowInterrupt.Abort(new ForbiddenResponse());</c>
        /// </remarks>
        public bool IsInRole(string role) => HttpContext?.User?.IsInRole(role) == true;

        /// <summary>
        /// Gets the authenticated user's identifier from claims.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available, user is not authenticated, or the claim is not present.
        /// </summary>
        /// <returns>The user identifier (typically from <see cref="ClaimTypes.NameIdentifier"/> claim), or <c>null</c> if not available.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to <c>context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value</c>.
        /// The identifier is typically the primary key from your user database (e.g., ASP.NET Identity's UserId).
        /// Example: <c>var userId = context.GetUserId() ?? throw new UnauthorizedException();</c>
        /// </remarks>
        public string? GetUserId() => HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        /// <summary>
        /// Gets the value of a specific HTTP request header.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios) or the header is not present.
        /// </summary>
        /// <param name="name">The name of the header to retrieve (case-insensitive).</param>
        /// <returns>The first value of the specified header, or <c>null</c> if not found.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method equivalent to <c>(string?)context.HttpContext?.Request.Headers[name]</c>.
        /// Use this to access custom headers, authentication tokens, content negotiation, etc.
        /// </para>
        /// <para>
        /// If the header has multiple values, only the first value is returned.
        /// Use <c>context.HttpContext?.Request.Headers[name]</c> directly to access all values.
        /// </para>
        /// <para><strong>⚠️ SECURITY WARNING - USER INPUT:</strong></para>
        /// <para>
        /// Request headers can be controlled by clients and must be treated as <strong>untrusted user input</strong>.
        /// Always validate header values, especially for authentication, authorization, and routing decisions.
        /// </para>
        /// <example>
        /// <strong>✅ SAFE - Validated header:</strong>
        /// <code>
        /// var apiKey = context.GetHeader("X-API-Key");
        /// if (string.IsNullOrWhiteSpace(apiKey) || !IsValidApiKey(apiKey))
        ///     return FlowInterrupt.Fail("Invalid API key", 401);
        ///     
        /// var contentType = context.GetHeader("Content-Type");
        /// if (contentType != "application/json")
        ///     return FlowInterrupt.Fail("Unsupported content type", 415);
        /// </code>
        /// 
        /// <strong>❌ UNSAFE - Direct usage without validation:</strong>
        /// <code>
        /// // ❌ TRUST CLIENT HEADER FOR AUTHENTICATION - VULNERABLE!
        /// var userId = context.GetHeader("X-User-Id"); // Client can set ANY value!
        /// var user = await db.Users.FindAsync(userId); // SECURITY BREACH!
        /// 
        /// // ❌ HEADER INJECTION RISK
        /// var customHeader = context.GetHeader("X-Custom");
        /// context.SetResponseHeader("X-Echo", customHeader); // Can inject CRLF characters!
        /// </code>
        /// 
        /// <strong>✅ SECURE patterns:</strong>
        /// <code>
        /// // ✅ Authentication via middleware (secure)
        /// var user = context.GetUser(); // Already validated by ASP.NET Core authentication
        /// 
        /// // ✅ Authorization header with proper validation
        /// var auth = context.GetHeader("Authorization");
        /// if (auth?.StartsWith("Bearer ") == true)
        /// {
        ///     var token = auth.Substring(7);
        ///     if (await _tokenValidator.ValidateAsync(token))
        ///         // Proceed
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public string? GetHeader(string name) => (string?)HttpContext?.Request.Headers[name];

        /// <summary>
        /// Gets the value of a specific query string parameter.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios) or the parameter is not present.
        /// </summary>
        /// <param name="name">The name of the query parameter to retrieve (case-sensitive).</param>
        /// <returns>The first value of the specified query parameter, or <c>null</c> if not found.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method equivalent to <c>(string?)context.HttpContext?.Request.Query[name]</c>.
        /// Use this to access query string parameters from the URL.
        /// </para>
        /// <para>
        /// If the parameter has multiple values, only the first value is returned.
        /// Use <c>context.HttpContext?.Request.Query[name]</c> directly to access all values.
        /// </para>
        /// <para><strong>⚠️ SECURITY WARNING - USER INPUT:</strong></para>
        /// <para>
        /// Query parameters are <strong>untrusted user input</strong> and must be validated/sanitized before use.
        /// Always validate format, range, and content to prevent injection attacks and ensure data integrity.
        /// </para>
        /// <example>
        /// <strong>✅ SAFE - Validated input:</strong>
        /// <code>
        /// var pageStr = context.GetQueryParam("page");
        /// if (!int.TryParse(pageStr, out var page) || page &lt; 0 || page &gt; 1000)
        ///     return FlowInterrupt.Fail("Invalid page number");
        ///     
        /// var status = context.GetQueryParam("status");
        /// if (!Enum.TryParse&lt;OrderStatus&gt;(status, out var orderStatus))
        ///     return FlowInterrupt.Fail("Invalid status value");
        /// </code>
        /// 
        /// <strong>❌ UNSAFE - Direct usage without validation:</strong>
        /// <code>
        /// // ❌ SQL INJECTION RISK!
        /// var name = context.GetQueryParam("name");
        /// var sql = $"SELECT * FROM Users WHERE Name = '{name}'"; // VULNERABLE!
        /// 
        /// // ❌ XSS RISK!
        /// var message = context.GetQueryParam("msg");
        /// return new Response { Html = $"&lt;div&gt;{message}&lt;/div&gt;" }; // VULNERABLE!
        /// 
        /// // ❌ PATH TRAVERSAL RISK!
        /// var file = context.GetQueryParam("file");
        /// var path = Path.Combine("uploads", file); // VULNERABLE if file = "../../etc/passwd"
        /// </code>
        /// </example>
        /// </remarks>
        public string? GetQueryParam(string name) => (string?)HttpContext?.Request.Query[name];

        /// <summary>
        /// Gets the value of a specific route parameter.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios) or the route parameter is not present.
        /// </summary>
        /// <param name="name">The name of the route parameter to retrieve (case-insensitive).</param>
        /// <returns>The string representation of the route parameter value, or <c>null</c> if not found.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method equivalent to <c>context.HttpContext?.GetRouteValue(name)?.ToString()</c>.
        /// Use this to access route parameters defined in your endpoint patterns.
        /// Example: <c>var userId = context.GetRouteValue("id");</c> for route pattern <c>"/users/{id}"</c>.
        /// </para>
        /// <para><strong>⚠️ SECURITY WARNING - USER INPUT:</strong></para>
        /// <para>
        /// Route parameters are <strong>untrusted user input</strong> derived from URL segments.
        /// Always validate format, type, and authorization before using them to access resources.
        /// </para>
        /// <example>
        /// <strong>✅ SAFE - Validated route parameter:</strong>
        /// <code>
        /// var idStr = context.GetRouteValue("id");
        /// if (!int.TryParse(idStr, out var id) || id &lt;= 0)
        ///     return FlowInterrupt.Fail("Invalid ID", 400);
        ///     
        /// // ✅ Check authorization
        /// var userId = context.GetUserId();
        /// var order = await db.Orders.FindAsync(id);
        /// if (order?.UserId != userId)
        ///     return FlowInterrupt.Fail("Forbidden", 403);
        /// </code>
        /// 
        /// <strong>❌ UNSAFE - Direct usage without validation:</strong>
        /// <code>
        /// // ❌ IDOR (Insecure Direct Object Reference) vulnerability
        /// var id = context.GetRouteValue("userId");
        /// var user = await db.Users.FindAsync(id); // No authorization check!
        /// return new UserResponse { Email = user.Email }; // LEAKED!
        /// 
        /// // ❌ Path traversal risk
        /// var filename = context.GetRouteValue("file");
        /// var path = Path.Combine("uploads", filename); // VULNERABLE if filename = "../../etc/passwd"
        /// </code>
        /// </example>
        /// </remarks>
        public string? GetRouteValue(string name) => HttpContext?.GetRouteValue(name)?.ToString();

        /// <summary>
        /// Gets the client's IP address from the HTTP connection.
        /// Returns <c>null</c> if <see cref="HttpContext"/> is not available (non-HTTP scenarios) or the connection info is not available.
        /// </summary>
        /// <returns>The string representation of the client's IP address, or <c>null</c> if not available.</returns>
        /// <remarks>
        /// This is a convenience method equivalent to <c>context.HttpContext?.Connection?.RemoteIpAddress?.ToString()</c>.
        /// Use this for logging, rate limiting, geo-location, or security purposes.
        /// Example: <c>var clientIp = context.GetClientIpAddress();</c>
        /// Note: Be aware of proxies and load balancers - consider checking <c>X-Forwarded-For</c> header for the real client IP.
        /// </remarks>
        public string? GetClientIpAddress() => HttpContext?.Connection?.RemoteIpAddress?.ToString();

        /// <summary>
        /// Sets the HTTP response status code.
        /// Does nothing if <see cref="HttpContext"/> is not available (non-HTTP scenarios).
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set (e.g., 200, 201, 400, 404, 500).</param>
        /// <remarks>
        /// This is a convenience method for setting <c>context.HttpContext.Response.StatusCode</c>.
        /// Use this to return custom status codes based on flow logic.
        /// Example: <c>context.SetStatusCode(201);</c> for created resources.
        /// <para><strong>⚠️ WARNING:</strong> Set the status code <strong>before</strong> returning the response. Do not write to Response.Body.</para>
        /// Common status codes:
        /// <list type="bullet">
        /// <item>200 (OK) - Default for successful requests</item>
        /// <item>201 (Created) - Resource successfully created</item>
        /// <item>204 (No Content) - Success with no response body</item>
        /// <item>400 (Bad Request) - Invalid input</item>
        /// <item>404 (Not Found) - Resource not found</item>
        /// <item>500 (Internal Server Error) - Unhandled exception</item>
        /// </list>
        /// </remarks>
        public void SetStatusCode(int statusCode)
        {
            if (HttpContext is not null)
            {
                HttpContext.Response.StatusCode = statusCode;
            }
        }

        /// <summary>
        /// Sets a custom HTTP response header.
        /// Does nothing if <see cref="HttpContext"/> is not available (non-HTTP scenarios).
        /// </summary>
        /// <param name="name">The name of the header to set.</param>
        /// <param name="value">The value of the header.</param>
        /// <remarks>
        /// This is a convenience method for setting <c>context.HttpContext.Response.Headers[name]</c>.
        /// Use this to add custom headers to the HTTP response.
        /// Example: <c>context.SetResponseHeader("Location", "/api/users/123");</c> for 201 Created responses.
        /// <para><strong>⚠️ WARNING:</strong> Set headers <strong>before</strong> returning the response. Do not write to Response.Body.</para>
        /// Common response headers:
        /// <list type="bullet">
        /// <item>Location - URL of newly created resource (with 201 Created)</item>
        /// <item>Cache-Control - Caching directives</item>
        /// <item>ETag - Resource version identifier</item>
        /// <item>X-Custom-Header - Custom application headers</item>
        /// </list>
        /// </remarks>
        public void SetResponseHeader(string name, string value)
        {
            if (HttpContext is not null)
            {
                HttpContext.Response.Headers[name] = value;
            }
        }

        /// <summary>
        /// Appends a cookie to the HTTP response.
        /// Does nothing if <see cref="HttpContext"/> is not available (non-HTTP scenarios).
        /// </summary>
        /// <param name="key">The name of the cookie.</param>
        /// <param name="value">The value of the cookie.</param>
        /// <param name="options">Optional cookie options (expiration, path, domain, security flags). Defaults to empty options if <c>null</c>.</param>
        /// <remarks>
        /// This is a convenience method for <c>context.HttpContext.Response.Cookies.Append(key, value, options)</c>.
        /// Use this to set cookies for session management, preferences, tracking, etc.
        /// Example: <c>context.SetCookie("session", sessionId, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });</c>
        /// <para><strong>⚠️ WARNING:</strong> Set cookies <strong>before</strong> returning the response. Do not write to Response.Body.</para>
        /// <para><strong>Security best practices:</strong></para>
        /// <list type="bullet">
        /// <item><strong>HttpOnly = true</strong> - Prevents JavaScript access (XSS protection)</item>
        /// <item><strong>Secure = true</strong> - Only send over HTTPS</item>
        /// <item><strong>SameSite = Strict/Lax</strong> - CSRF protection</item>
        /// <item><strong>Expires</strong> - Set explicit expiration for persistent cookies</item>
        /// </list>
        /// </remarks>
        public void SetCookie(string key, string value, CookieOptions? options = null)
        {
            HttpContext?.Response.Cookies.Append(key, value, options ?? _defaultCookieOptions);
        }

        /// <summary>
        /// Composite key structure that combines a Type and an optional string name.
        /// Enables storing multiple values of the same type under different named keys.
        /// </summary>
        /// <remarks>
        /// This struct is used as the dictionary key for FlowContext storage.
        /// It provides efficient equality comparison and hash code generation.
        /// Structural equality ensures (typeof(User), "admin") == (typeof(User), "admin").
        /// </remarks>
        internal readonly struct CompositeKey : IEquatable<CompositeKey>
        {
            private readonly Type _type;
            private readonly string? _name;

            public CompositeKey(Type type, string? name)
            {
                _type = type;
                _name = name;
            }

            public bool Equals(CompositeKey other) => _type == other._type && _name == other._name;

            public override bool Equals(object? obj) => obj is CompositeKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(_type, _name);
        }

        /// <summary>
        /// Disposable struct that restores a previous value in the context when disposed.
        /// Returned by <see cref="Push{T}"/>.
        /// </summary>
        public readonly struct ScopeReverter : IDisposable
        {
            private readonly FlowContext _ctx;
            private readonly CompositeKey _key;
            private readonly bool _hadOld;
            private readonly object? _oldValue;

            internal ScopeReverter(FlowContext ctx, CompositeKey key, bool hadOld, object? oldValue)
            {
                _ctx = ctx;
                _key = key;
                _hadOld = hadOld;
                _oldValue = oldValue;
            }

            /// <summary>
            /// Restores the previous value (or removes the entry if there was no previous value).
            /// </summary>
            public void Dispose()
            {
                lock (_ctx._syncLock)
                {
                    if (_hadOld)
                    {
                        _ctx._items[_key] = _oldValue;
                    }
                    else
                    {
                        _ctx._items.Remove(_key);
                    }
                }
            }
        }

        /// <summary>
        /// Disposable struct that measures and records elapsed time when disposed.
        /// Returned by <see cref="StartTimer"/>.
        /// Uses high-precision <see cref="Stopwatch.GetTimestamp"/> for accurate measurements.
        /// </summary>
        public readonly struct TimerDisposable : IDisposable
        {
            private readonly FlowContext _ctx;
            private readonly string _key;
            private readonly long _start;

            internal TimerDisposable(FlowContext ctx, string key)
            {
                _ctx = ctx;
                _key = key;
                _start = Stopwatch.GetTimestamp();
            }

            /// <summary>
            /// Calculates the elapsed time and stores it in the context's timer dictionary under the timer's key.
            /// Elapsed time is stored as <see cref="Stopwatch"/> ticks (not <see cref="System.TimeSpan"/>) for maximum precision.
            /// Convert to <see cref="System.TimeSpan"/> via <see cref="Stopwatch.GetElapsedTime(long)"/> when needed.
            /// </summary>
            public void Dispose()
            {
                long elapsed = Stopwatch.GetTimestamp() - _start;
                lock (_ctx._syncLock)
                {
                    (_ctx._timers ??= new Dictionary<string, long>())[_key] = elapsed;
                }
            }
        }
    }

}
