using FlowT.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that resolves and caches the tenant identifier for the current flow execution.
    /// Supports multi-tenant applications where the tenant is conveyed via a claim, request header, or route value.
    /// </summary>
    /// <remarks>
    /// Resolution order:
    /// <list type="number">
    /// <item><description>Claim <c>tid</c> (Azure AD tenant claim) from <see cref="FlowContext.HttpContext"/> user principal.</description></item>
    /// <item><description>Value of the <c>X-Tenant-Id</c> request header.</description></item>
    /// <item><description>Route value <c>tenantId</c> (e.g. <c>/api/{tenantId}/orders</c>).</description></item>
    /// <item><description>Falls back to <c>"default"</c> when none of the above is present.</description></item>
    /// </list>
    /// Register via <c>services.AddFlowPlugin&lt;ITenantPlugin, TenantPlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var tenant = context.Plugin&lt;ITenantPlugin&gt;();
    /// logger.LogInformation("Processing request for tenant {TenantId}", tenant.TenantId);
    /// </code>
    /// </para>
    /// </remarks>
    public interface ITenantPlugin
    {
        /// <summary>
        /// Gets the resolved tenant identifier for the current flow execution.
        /// Never <c>null</c> — falls back to <c>"default"</c> when the tenant cannot be determined.
        /// </summary>
        string TenantId { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="ITenantPlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class TenantPlugin : FlowPlugin, ITenantPlugin
    {
        private string? _tenantId;

        /// <inheritdoc />
        public string TenantId
        {
            get
            {
                if (_tenantId is not null)
                    return _tenantId;

                HttpContext? http = Context.HttpContext;

                if (http is not null)
                {
                    // 1. Claim "tid"
                    string? claim = http.User?.FindFirst("tid")?.Value;
                    if (!string.IsNullOrEmpty(claim))
                    {
                        _tenantId = claim;
                        return _tenantId;
                    }

                    // 2. X-Tenant-Id header
                    if (http.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValues)
                        && headerValues.Count > 0
                        && !string.IsNullOrEmpty(headerValues[0]))
                    {
                        _tenantId = headerValues[0]!;
                        return _tenantId;
                    }

                    // 3. Route value "tenantId"
                    string? routeValue = Context.GetRouteValue("tenantId");
                    if (!string.IsNullOrEmpty(routeValue))
                    {
                        _tenantId = routeValue;
                        return _tenantId;
                    }
                }

                // 4. Default fallback
                _tenantId = "default";
                return _tenantId;
            }
        }
    }
}
