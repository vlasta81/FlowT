## TenantPlugin Class

Default implementation of [ITenantPlugin](ITenantPlugin.md 'FlowT\.Plugins\.ITenantPlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class TenantPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.ITenantPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; TenantPlugin

Implements [ITenantPlugin](ITenantPlugin.md 'FlowT\.Plugins\.ITenantPlugin')

| Properties | |
| :--- | :--- |
| [TenantId](TenantPlugin.TenantId.md 'FlowT\.Plugins\.TenantPlugin\.TenantId') | Gets the resolved tenant identifier for the current flow execution\. Never `null` — falls back to `"default"` when the tenant cannot be determined\. |
