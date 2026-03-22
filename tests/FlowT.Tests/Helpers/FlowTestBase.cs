using FlowT;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests.Helpers;

/// <summary>
/// Base class for flow tests with common setup
/// </summary>
public abstract class FlowTestBase
{
    /// <summary>
    /// Creates a FlowContext with a fresh service provider
    /// </summary>
    protected static FlowContext CreateContext(IServiceProvider? services = null)
    {
        services ??= new ServiceCollection().BuildServiceProvider();

        return new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };
    }

    /// <summary>
    /// Creates a FlowContext with custom cancellation token
    /// </summary>
    protected static FlowContext CreateContext(IServiceProvider services, CancellationToken cancellationToken)
    {
        return new FlowContext
        {
            Services = services,
            CancellationToken = cancellationToken
        };
    }

    /// <summary>
    /// Creates a basic service collection for tests
    /// </summary>
    protected static IServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection();
    }

    /// <summary>
    /// Builds a service provider with the given services
    /// </summary>
    protected static IServiceProvider BuildServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }
}
