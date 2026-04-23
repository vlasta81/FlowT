## FlowContext\.Plugin\<T\>\(\) Method

Resolves a plugin registered for this flow context, creating and caching it on first access\.
The plugin instance is scoped to this [FlowContext](FlowContext.md 'FlowT\.FlowContext') — one instance per flow execution\.

```csharp
public T Plugin<T>()
    where T : class;
```
#### Type parameters

<a name='FlowT.FlowContext.Plugin_T_().T'></a>

`T`

The plugin interface type\. Must be registered via [AddFlowPlugin&lt;TPlugin,TImpl&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowPlugin\<TPlugin,TImpl\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)')\.

#### Returns
[T](FlowContext.Plugin_T_().md#FlowT.FlowContext.Plugin_T_().T 'FlowT\.FlowContext\.Plugin\<T\>\(\)\.T')  
The plugin instance for this flow execution\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown when the plugin type is not registered\.

### Remarks
Plugins are always \<strong\>PerFlow\</strong\> — one instance shared across all pipeline stages
\(specifications, policies, handler\) within the same flow execution\.
This enables plugins to accumulate state \(metrics, trace spans, audit entries\) across the entire pipeline\.
Use [AddFlowPlugin&lt;TPlugin,TImpl&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowPlugin\<TPlugin,TImpl\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') in Program\.cs or module registration\.

If the plugin inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin'), the [FlowContext](FlowContext.md 'FlowT\.FlowContext') is bound
automatically after creation via an internal call, giving the plugin full access to this context
through its `protected Context` property.

<strong>Thread safety:</strong> After the plugin is created, subsequent calls use a lockless read path for performance.
            The first-time creation is protected by a lock to prevent duplicate initialization.

```csharp
var metrics = context.Plugin<IRequestMetrics>();
metrics.RecordDbQuery(elapsed);
```