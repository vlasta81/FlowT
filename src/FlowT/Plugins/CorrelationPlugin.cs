using FlowT.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that provides a stable correlation ID for the current flow execution.
    /// The ID is resolved once per flow and cached.
    /// </summary>
    /// <remarks>
    /// Resolution order:
    /// <list type="number">
    /// <item><description>Value of the <c>X-Correlation-Id</c> request header (when an <see cref="FlowContext.HttpContext"/> is present).</description></item>
    /// <item><description>The flow's own ID via <see cref="FlowContext.GetFlowIdString()"/> (non-HTTP scenarios or missing header).</description></item>
    /// </list>
    /// Register via <c>services.AddFlowPlugin&lt;ICorrelationPlugin, CorrelationPlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var correlation = context.Plugin&lt;ICorrelationPlugin&gt;();
    /// logger.LogInformation("[{CorrelationId}] Processing request", correlation.CorrelationId);
    /// </code>
    /// </para>
    /// </remarks>
    public interface ICorrelationPlugin
    {
        /// <summary>
        /// Gets the correlation ID for the current flow execution.
        /// Resolved from the <c>X-Correlation-Id</c> request header when available;
        /// falls back to the flow's own ID (<see cref="FlowContext.GetFlowIdString()"/>).
        /// </summary>
        string CorrelationId { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="ICorrelationPlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class CorrelationPlugin : FlowPlugin, ICorrelationPlugin
    {
        private string? _id;

        /// <inheritdoc />
        public string CorrelationId
        {
            get
            {
                if (_id is not null)
                    return _id;

                HttpContext? http = Context.HttpContext;
                _id = http is not null
                    && http.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
                    && values.Count > 0
                        ? values[0]!
                        : Context.GetFlowIdString();

                return _id;
            }
        }
    }
}
