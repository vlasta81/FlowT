using FlowT.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that reads and caches the idempotency key for the current flow execution.
    /// The key is read once from the <c>X-Idempotency-Key</c> request header and shared across
    /// all pipeline stages (specifications, policies, handler).
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IIdempotencyPlugin, IdempotencyPlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var idempotency = context.Plugin&lt;IIdempotencyPlugin&gt;();
    /// if (idempotency.HasKey &amp;&amp; await store.ExistsAsync(idempotency.Key!))
    ///     return cachedResponse;
    ///
    /// // ... process request ...
    /// await store.StoreAsync(idempotency.Key!, response);
    /// </code>
    /// </para>
    /// </remarks>
    public interface IIdempotencyPlugin
    {
        /// <summary>
        /// Gets a value indicating whether an idempotency key was present in the request.
        /// <c>false</c> in non-HTTP scenarios or when the <c>X-Idempotency-Key</c> header is absent.
        /// </summary>
        bool HasKey { get; }

        /// <summary>
        /// Gets the idempotency key from the <c>X-Idempotency-Key</c> request header,
        /// or <c>null</c> when the header is absent or in non-HTTP scenarios.
        /// </summary>
        string? Key { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="IIdempotencyPlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class IdempotencyPlugin : FlowPlugin, IIdempotencyPlugin
    {
        private string? _key;
        private bool _resolved;

        private void Resolve()
        {
            if (_resolved)
                return;

            HttpContext? http = Context.HttpContext;
            if (http is not null
                && http.Request.Headers.TryGetValue("X-Idempotency-Key", out var values)
                && values.Count > 0
                && !string.IsNullOrEmpty(values[0]))
            {
                _key = values[0]!;
            }

            _resolved = true;
        }

        /// <inheritdoc />
        public bool HasKey
        {
            get
            {
                Resolve();
                return _key is not null;
            }
        }

        /// <inheritdoc />
        public string? Key
        {
            get
            {
                Resolve();
                return _key;
            }
        }
    }
}
