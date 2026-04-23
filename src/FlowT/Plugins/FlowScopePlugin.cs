using FlowT.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that creates and exposes a dedicated <see cref="IServiceScope"/> for the current flow execution.
    /// Useful in non-HTTP scenarios (background jobs, message consumers, hosted services) where ASP.NET Core does not
    /// automatically manage a per-request DI scope.
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IFlowScopePlugin, FlowScopePlugin&gt;()</c>.
    /// <para>
    /// <strong>Disposal responsibility:</strong> The plugin implements <see cref="IDisposable"/>.
    /// Because plugins are PerFlow singletons (not managed by a DI scope), the caller is responsible for
    /// disposing the plugin — and therefore the scope — after the flow completes.
    /// When used inside a <see cref="FlowContext"/> that is created and owned by a hosted service or
    /// pipeline host, that host should dispose the plugin at the end of each unit of work.
    /// </para>
    /// <para>
    /// Usage:
    /// <code>
    /// var scopePlugin = context.Plugin&lt;IFlowScopePlugin&gt;();
    /// var dbContext = scopePlugin.ScopedServices.GetRequiredService&lt;AppDbContext&gt;();
    ///
    /// // ... use dbContext ...
    ///
    /// scopePlugin.Dispose(); // dispose the scope when the flow is finished
    /// </code>
    /// </para>
    /// </remarks>
    public interface IFlowScopePlugin : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> from the dedicated scope for this flow.
        /// The scope is created lazily on the first access to this property.
        /// </summary>
        IServiceProvider ScopedServices { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="IFlowScopePlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class FlowScopePlugin : FlowPlugin, IFlowScopePlugin
    {
        private IServiceScope? _scope;
        private bool _disposed;

        /// <inheritdoc />
        public IServiceProvider ScopedServices
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(FlowScopePlugin));

                if (_scope is null)
                    _scope = Context.Service<IServiceScopeFactory>().CreateScope();

                return _scope.ServiceProvider;
            }
        }

        /// <summary>
        /// Disposes the underlying <see cref="IServiceScope"/>, releasing all scoped services.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _scope?.Dispose();
            _scope = null;
        }
    }
}
