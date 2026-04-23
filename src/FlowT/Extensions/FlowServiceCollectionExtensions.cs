using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FlowT.Extensions
{
    /// <summary>
    /// Extension methods for registering flows and modules in dependency injection.
    /// </summary>
    public static class FlowServiceCollectionExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, object, object>> _factoryCache = new();

        /// <summary>
        /// Scans the specified assemblies for classes marked with <see cref="Attributes.FlowModuleAttribute"/> and registers them.
        /// Each module's <see cref="IFlowModule.Register"/> method is called to register its flows and services.
        /// </summary>
        /// <param name="services">The service collection to add modules to.</param>
        /// <param name="assemblies">The assemblies to scan. If empty, uses the calling assembly.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// <para>
        /// This method requires types to be marked with <see cref="Attributes.FlowModuleAttribute"/> for explicit opt-in.
        /// Types must also implement <see cref="IFlowModule"/> and have a parameterless constructor.
        /// </para>
        /// <example>
        /// <code>
        /// [FlowModule]
        /// public class UserModule : IFlowModule
        /// {
        ///     public void Register(IServiceCollection services) { }
        ///     public void MapEndpoints(IEndpointRouteBuilder app) { }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public static IServiceCollection AddFlowModules(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                assemblies = [Assembly.GetCallingAssembly()];
            }
            IEnumerable<TypeInfo> modules = assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t => !t.IsAbstract 
                    && typeof(IFlowModule).IsAssignableFrom(t)
                    && t.GetCustomAttribute<Attributes.FlowModuleAttribute>() is not null);
            foreach (TypeInfo type in modules)
            {
                IFlowModule module = (IFlowModule)Activator.CreateInstance(type.AsType())!;
                module.Register(services);
                services.AddSingleton(typeof(IFlowModule), module);
            }
            return services;
        }

        /// <summary>
        /// [DEPRECATED] Scans assemblies for flows. Use <see cref="AddFlow{TFlow, TRequest, TResponse}"/> in modules instead.
        /// </summary>
        /// <param name="services">The service collection to add flows to.</param>
        /// <param name="assemblies">The assemblies to scan. If empty, uses the calling assembly.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// <para><strong>⚠️ This method is deprecated and may cause duplicate registrations when used with <see cref="AddFlowModules"/>.</strong></para>
        /// <para><strong>Migration Guide:</strong></para>
        /// <list type="bullet">
        /// <item><strong>In modules:</strong> Use <see cref="AddFlow{TFlow, TRequest, TResponse}"/> to explicitly register each flow
        /// <code>
        /// [FlowModule]
        /// public class UserModule : IFlowModule
        /// {
        ///     public void Register(IServiceCollection services)
        ///     {
        ///         // ✅ NEW: Explicit per-flow registration
        ///         services.AddFlow&lt;CreateUserFlow, CreateUserRequest, CreateUserResponse&gt;();
        ///         services.AddFlow&lt;UpdateUserFlow, UpdateUserRequest, UpdateUserResponse&gt;();
        ///         
        ///         // ❌ OLD: services.AddFlows(typeof(UserModule).Assembly);
        ///     }
        /// }
        /// </code>
        /// </item>
        /// <item><strong>In simple projects without modules:</strong> Use <see cref="AddFlow{TFlow, TRequest, TResponse}"/> in Program.cs
        /// <code>
        /// builder.Services.AddFlow&lt;SimpleFlow, SimpleRequest, SimpleResponse&gt;();
        /// </code>
        /// </item>
        /// </list>
        /// <para>
        /// Types must be marked with <see cref="Attributes.FlowDefinitionAttribute"/> and inherit from <see cref="FlowDefinition{TRequest, TResponse}"/>.
        /// </para>
        /// </remarks>
        [Obsolete("Use AddFlow<TFlow, TRequest, TResponse>() for explicit registration per flow in modules, " +
                  "or AddFlowModules() in Program.cs for modular projects. " +
                  "This method can cause duplicate registrations when multiple modules call it.", 
                  error: false)]
        public static IServiceCollection AddFlows(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                assemblies = [Assembly.GetCallingAssembly()];
            }
            IEnumerable<TypeInfo> flows = assemblies
                .SelectMany(a => a.DefinedTypes)
                .Where(t =>
                    !t.IsAbstract &&
                    t.BaseType is not null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(FlowDefinition<,>) &&
                    t.GetCustomAttribute<Attributes.FlowDefinitionAttribute>() is not null);
            foreach (TypeInfo type in flows)
            {
                RegisterFlowType(services, type.AsType());
            }
            return services;
        }

        /// <summary>
        /// Registers a single flow with dependency injection.
        /// The flow is registered as a singleton with both its concrete type and <see cref="IFlow{TRequest, TResponse}"/> interface.
        /// If the flow is already registered, the call is ignored (no duplicate registrations).
        /// </summary>
        /// <typeparam name="TFlow">The flow type to register. Must inherit from <see cref="FlowDefinition{TRequest, TResponse}"/> and be marked with <see cref="Attributes.FlowDefinitionAttribute"/>.</typeparam>
        /// <typeparam name="TRequest">The type of request the flow processes.</typeparam>
        /// <typeparam name="TResponse">The type of response the flow produces.</typeparam>
        /// <param name="services">The service collection to add the flow to.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// <para><strong>Duplicate Registration Protection:</strong></para>
        /// <para>
        /// This method uses <see cref="ServiceCollectionDescriptorExtensions.TryAddSingleton{TService}(IServiceCollection)"/> internally,
        /// so calling it multiple times with the same flow type is safe - only the first registration is kept.
        /// This prevents issues when multiple modules or code paths accidentally register the same flow.
        /// </para>
        /// <para><strong>Usage Patterns:</strong></para>
        /// <list type="number">
        /// <item>
        /// <strong>Modular projects (recommended):</strong> Use in <see cref="IFlowModule.Register"/> to explicitly register flows per module
        /// <code>
        /// [FlowModule]
        /// public class UserModule : IFlowModule
        /// {
        ///     public void Register(IServiceCollection services)
        ///     {
        ///         // ✅ Explicit flow registration
        ///         services.AddFlow&lt;CreateUserFlow, CreateUserRequest, CreateUserResponse&gt;();
        ///         services.AddFlow&lt;UpdateUserFlow, UpdateUserRequest, UpdateUserResponse&gt;();
        ///         
        ///         // Register only external dependencies (handlers/specs/policies are auto-created)
        ///         services.AddSingleton&lt;IUserRepository, UserRepository&gt;();
        ///     }
        /// }
        /// </code>
        /// </item>
        /// <item>
        /// <strong>Simple projects without modules:</strong> Use in Program.cs for standalone flows
        /// <code>
        /// builder.Services.AddFlow&lt;CreateUserFlow, CreateUserRequest, CreateUserResponse&gt;();
        /// </code>
        /// </item>
        /// <item>
        /// <strong>Hybrid approach:</strong> Use <see cref="AddFlowModules"/> for organized features + AddFlow for standalone flows
        /// <code>
        /// builder.Services.AddFlowModules(typeof(Program).Assembly);  // Registers modules
        /// builder.Services.AddFlow&lt;HealthCheckFlow, HealthCheckRequest, HealthCheckResponse&gt;();  // Standalone flow
        /// </code>
        /// </item>
        /// </list>
        /// <para><strong>Automatic Dependency Construction:</strong></para>
        /// <para>
        /// Handlers, specifications, and policies are automatically constructed using <see cref="ActivatorUtilities.CreateInstance"/>.
        /// You do NOT need to register them manually unless you want to override the default construction (e.g., for singleton state, mocking, or custom factories).
        /// </para>
        /// <code>
        /// // ✅ Handler is auto-created with dependencies from DI
        /// public class CreateUserHandler(ILogger&lt;CreateUserHandler&gt; logger, IUserRepository repo) 
        ///     : IFlowHandler&lt;CreateUserRequest, CreateUserResponse&gt;
        /// {
        ///     public async ValueTask&lt;CreateUserResponse&gt; HandleAsync(CreateUserRequest request, FlowContext context)
        ///     {
        ///         // ✅ Scoped services resolved per-request
        ///         var db = context.Service&lt;AppDbContext&gt;();
        ///         // ...
        ///     }
        /// }
        /// </code>
        /// <para><strong>⚠️ Important:</strong> Flow type must be marked with <see cref="Attributes.FlowDefinitionAttribute"/>.</para>
        /// <para><strong>Performance:</strong> Flows are cached as singletons. The pipeline (handler + policies + specs) is lazily initialized on first use and reused across all requests.</para>
        /// <para><strong>🔒 Security consideration:</strong></para>
        /// <para>
        /// Flows are registered as <strong>singletons</strong> and shared across all concurrent requests for performance.
        /// This means you must <strong>NEVER</strong> store per-request data (request/response objects, user data, FlowContext, etc.) in instance fields.
        /// Always use <see cref="FlowContext"/> for per-request state to prevent data leaks between users.
        /// FlowT analyzers (FlowT001-FlowT019) detect common violations at compile-time.
        /// </para>
        /// <example>
        /// <strong>❌ UNSAFE - Data leak:</strong>
        /// <code>
        /// public class UserHandler : IFlowHandler&lt;CreateUserRequest, CreateUserResponse&gt;
        /// {
        ///     private CreateUserRequest? _currentRequest; // ❌ Shared between all users!
        ///     
        ///     public async ValueTask&lt;CreateUserResponse&gt; HandleAsync(CreateUserRequest request, FlowContext context)
        ///     {
        ///         _currentRequest = request; // ❌ User A sees User B's request!
        ///         // ...
        ///     }
        /// }
        /// </code>
        /// <strong>✅ SAFE - Per-request isolation:</strong>
        /// <code>
        /// public class UserHandler : IFlowHandler&lt;CreateUserRequest, CreateUserResponse&gt;
        /// {
        ///     private readonly ILogger _logger; // ✅ Readonly dependencies are safe
        ///     
        ///     public async ValueTask&lt;CreateUserResponse&gt; HandleAsync(CreateUserRequest request, FlowContext context)
        ///     {
        ///         // ✅ Use context for per-request state
        ///         context.Set("currentRequest", request);
        ///         
        ///         // ✅ Resolve scoped services per-request
        ///         var db = context.Service&lt;AppDbContext&gt;();
        ///         
        ///         // ✅ Local variables are safe
        ///         var tempData = new List&lt;string&gt;();
        ///         
        ///         return new CreateUserResponse();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the flow type is not marked with <see cref="Attributes.FlowDefinitionAttribute"/>.</exception>
        public static IServiceCollection AddFlow<TFlow, TRequest, TResponse>(this IServiceCollection services)
            where TFlow : FlowDefinition<TRequest, TResponse>
            where TRequest : notnull
            where TResponse : notnull
        {
            Type flowType = typeof(TFlow);
            if (flowType.GetCustomAttribute<Attributes.FlowDefinitionAttribute>() is null)
            {
                throw new InvalidOperationException(
                    $"Flow '{flowType.Name}' must be marked with [FlowDefinition] attribute. " +
                    $"Add [FlowDefinition] to the class declaration.");
            }
            RegisterFlowType(services, flowType);
            return services;
        }

        /// <summary>
        /// Registers a plugin for use via <see cref="FlowContext.Plugin{T}"/>.
        /// The plugin is created once per flow execution and cached in <see cref="FlowContext"/> for the duration of that flow.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin interface type to register.</typeparam>
        /// <typeparam name="TImpl">The concrete implementation. Must implement <typeparamref name="TPlugin"/>.</typeparam>
        /// <param name="services">The service collection to add the plugin to.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// <para>
        /// Plugins are always <strong>PerFlow</strong> — one instance per <see cref="FlowContext"/> execution,
        /// shared across all pipeline stages (specifications, policies, handler) within the same flow.
        /// This enables plugins to accumulate state (metrics, trace spans, audit entries) across the entire pipeline.
        /// </para>
        /// <para>
        /// The implementation is registered as <strong>Transient</strong> in the DI container.
        /// <see cref="FlowContext"/> manages caching so each flow execution gets exactly one instance.
        /// </para>
        /// <code>
        /// // Program.cs
        /// builder.Services.AddFlowPlugin&lt;IRequestMetrics, RequestMetricsCollector&gt;();
        ///
        /// // Handler / Policy / Specification
        /// var metrics = context.Plugin&lt;IRequestMetrics&gt;();
        /// metrics.RecordDbQuery(elapsed);
        /// </code>
        /// </remarks>
        public static IServiceCollection AddFlowPlugin<TPlugin, TImpl>(this IServiceCollection services)
            where TPlugin : class
            where TImpl : class, TPlugin
        {
            services.TryAddTransient<TPlugin, TImpl>();
            return services;
        }

        private static void RegisterFlowType(IServiceCollection services, Type flowType)
        {
            Type baseType = flowType.BaseType!;
            Type[] args = baseType.GetGenericArguments();
            Type requestType = args[0];
            Type responseType = args[1];
            // Prevent duplicate registrations - register only if not already present
            services.TryAddSingleton(flowType);
            Type interfaceType = typeof(IFlow<,>).MakeGenericType(requestType, responseType);
            services.TryAddSingleton(interfaceType, sp =>
            {
                object definition = sp.GetRequiredService(flowType);
                Func<IServiceProvider, object, object> factory = _factoryCache.GetOrAdd(flowType, CreateFactory);
                return factory(sp, definition);
            });
        }

        private static Func<IServiceProvider, object, object> CreateFactory(Type flowType)
        {
            Type baseType = flowType.BaseType!;
            Type[] args = baseType.GetGenericArguments();
            MethodInfo method = typeof(FlowFactory)
                .GetMethod(nameof(FlowFactory.Create))!
                .MakeGenericMethod(args);
            ParameterExpression spParam = Expression.Parameter(typeof(IServiceProvider));
            ParameterExpression defParam = Expression.Parameter(typeof(object));
            MethodCallExpression body = Expression.Call(method, spParam, Expression.Convert(defParam, flowType));
            return Expression
                .Lambda<Func<IServiceProvider, object, object>>(Expression.Convert(body, typeof(object)), spParam, defParam)
                .Compile();
        }
    }

}
