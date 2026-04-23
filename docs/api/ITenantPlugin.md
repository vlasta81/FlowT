## ITenantPlugin Interface

Built\-in plugin that resolves and caches the tenant identifier for the current flow execution\.
Supports multi\-tenant applications where the tenant is conveyed via a claim, request header, or route value\.

```csharp
public interface ITenantPlugin
```

Derived  
&#8627; [TenantPlugin](TenantPlugin.md 'FlowT\.Plugins\.TenantPlugin')

### Remarks
Resolution order:
1. Claim `tid` (Azure AD tenant claim) from [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') user principal.
2. Value of the `X-Tenant-Id` request header.
3. Route value `tenantId` (e.g. `/api/{tenantId}/orders`).
4. Falls back to `"default"` when none of the above is present.

Register via `services.AddFlowPlugin<ITenantPlugin, TenantPlugin>()`\.

Usage:

```csharp
var tenant = context.Plugin<ITenantPlugin>();
logger.LogInformation("Processing request for tenant {TenantId}", tenant.TenantId);
```

| Properties | |
| :--- | :--- |
| [TenantId](ITenantPlugin.TenantId.md 'FlowT\.Plugins\.ITenantPlugin\.TenantId') | Gets the resolved tenant identifier for the current flow execution\. Never `null` — falls back to `"default"` when the tenant cannot be determined\. |
