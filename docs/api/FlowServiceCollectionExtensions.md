## FlowServiceCollectionExtensions Class

Extension methods for registering flows and modules in dependency injection\.

```csharp
public static class FlowServiceCollectionExtensions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowServiceCollectionExtensions

| Methods | |
| :--- | :--- |
| [AddFlow&lt;TFlow,TRequest,TResponse&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlow.IN2LROACCX9J5TGD4ZZ7N0FB4.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') | Registers a single flow with dependency injection\. The flow is registered as a singleton with both its concrete type and [IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>') interface\. If the flow is already registered, the call is ignored \(no duplicate registrations\)\. |
| [AddFlowModules\(this IServiceCollection, Assembly\[\]\)](FlowServiceCollectionExtensions.AddFlowModules.CRABQVK4FE6FJR6BTVIJUR4R9.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowModules\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection, System\.Reflection\.Assembly\[\]\)') | Scans the specified assemblies for classes marked with [FlowModuleAttribute](FlowModuleAttribute.md 'FlowT\.Attributes\.FlowModuleAttribute') and registers them\. Each module's [Register\(IServiceCollection\)](IFlowModule.Register.ZE388V5XQHD23SSNAQ7TEOOCE.md 'FlowT\.Contracts\.IFlowModule\.Register\(Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') method is called to register its flows and services\. |
| [AddFlowPlugin&lt;TPlugin,TImpl&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowPlugin\<TPlugin,TImpl\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') | Registers a plugin for use via [Plugin&lt;T&gt;\(\)](FlowContext.Plugin_T_().md 'FlowT\.FlowContext\.Plugin\<T\>\(\)')\. The plugin is created once per flow execution and cached in [FlowContext](FlowContext.md 'FlowT\.FlowContext') for the duration of that flow\. |
| [AddFlows\(this IServiceCollection, Assembly\[\]\)](FlowServiceCollectionExtensions.AddFlows.X7RY68MYH6QHDKD789FMOXD6C.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlows\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection, System\.Reflection\.Assembly\[\]\)') | \[DEPRECATED\] Scans assemblies for flows\. Use [AddFlow&lt;TFlow,TRequest,TResponse&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlow.IN2LROACCX9J5TGD4ZZ7N0FB4.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') in modules instead\. |
