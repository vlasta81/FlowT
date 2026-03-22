using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FlowT.Tests.Helpers;

/// <summary>
/// Helper extensions for simplifying test setup
/// </summary>
public static class TestServiceCollectionExtensions
{
    /// <summary>
    /// Registers a single flow with all its dependencies (handlers, specs, policies)
    /// </summary>
    public static IServiceCollection AddFlowWithDependencies<TFlow>(
        this IServiceCollection services,
        params Type[] dependencies)
        where TFlow : class
    {
        // Register the flow itself
        services.AddSingleton<TFlow>();

        // Register all dependencies
        foreach (var dependency in dependencies)
        {
            services.AddTransient(dependency);
        }

        return services;
    }

    /// <summary>
    /// Registers flows from current test assembly with auto-discovery of dependencies
    /// </summary>
    public static IServiceCollection AddTestFlows(this IServiceCollection services)
    {
        var testAssembly = Assembly.GetCallingAssembly();

        // Register all flows
        services.AddFlows(testAssembly);

        // Auto-register handlers, specs, and policies from test assembly
        var types = testAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            // Register handlers
            if (type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IFlowHandler<,>)))
            {
                if (!services.Any(s => s.ServiceType == type))
                {
                    services.AddTransient(type);
                }
            }

            // Register specifications
            if (type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IFlowSpecification<>)))
            {
                if (!services.Any(s => s.ServiceType == type))
                {
                    services.AddTransient(type);
                }
            }

            // Register policies
            if (type.BaseType != null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == typeof(FlowPolicy<,>))
            {
                if (!services.Any(s => s.ServiceType == type))
                {
                    services.AddTransient(type);
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Creates a service provider with common test services pre-registered
    /// </summary>
    public static IServiceProvider BuildTestServiceProvider(this IServiceCollection services)
    {
        // Add any common test dependencies here
        return services.BuildServiceProvider();
    }
}
