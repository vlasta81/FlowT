## FlowScopePlugin Class

Default implementation of [IFlowScopePlugin](IFlowScopePlugin.md 'FlowT\.Plugins\.IFlowScopePlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class FlowScopePlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IFlowScopePlugin, System.IDisposable
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; FlowScopePlugin

Implements [IFlowScopePlugin](IFlowScopePlugin.md 'FlowT\.Plugins\.IFlowScopePlugin'), [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable')

| Properties | |
| :--- | :--- |
| [ScopedServices](FlowScopePlugin.ScopedServices.md 'FlowT\.Plugins\.FlowScopePlugin\.ScopedServices') | Gets the [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider') from the dedicated scope for this flow\. The scope is created lazily on the first access to this property\. |

| Methods | |
| :--- | :--- |
| [Dispose\(\)](FlowScopePlugin.Dispose().md 'FlowT\.Plugins\.FlowScopePlugin\.Dispose\(\)') | Disposes the underlying [Microsoft\.Extensions\.DependencyInjection\.IServiceScope](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescope 'Microsoft\.Extensions\.DependencyInjection\.IServiceScope'), releasing all scoped services\. |
