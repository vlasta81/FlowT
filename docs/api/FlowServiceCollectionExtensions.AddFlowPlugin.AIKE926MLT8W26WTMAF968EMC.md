## FlowServiceCollectionExtensions\.AddFlowPlugin\<TPlugin,TImpl\>\(this IServiceCollection\) Method

Registers a plugin for use via [Plugin&lt;T&gt;\(\)](FlowContext.Plugin_T_().md 'FlowT\.FlowContext\.Plugin\<T\>\(\)')\.
The plugin is created once per flow execution and cached in [FlowContext](FlowContext.md 'FlowT\.FlowContext') for the duration of that flow\.

```csharp
public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddFlowPlugin<TPlugin,TImpl>(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    where TPlugin : class
    where TImpl : class, TPlugin;
```
#### Type parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowPlugin_TPlugin,TImpl_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TPlugin'></a>

`TPlugin`

The plugin interface type to register\.

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowPlugin_TPlugin,TImpl_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TImpl'></a>

`TImpl`

The concrete implementation\. Must implement [TPlugin](FlowServiceCollectionExtensions.AddFlowPlugin.AIKE926MLT8W26WTMAF968EMC.md#FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowPlugin_TPlugin,TImpl_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TPlugin 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowPlugin\<TPlugin,TImpl\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)\.TPlugin')\.
#### Parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowPlugin_TPlugin,TImpl_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).services'></a>

`services` [Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')

The service collection to add the plugin to\.

#### Returns
[Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')  
The service collection for method chaining\.

### Remarks

Plugins are always <strong>PerFlow</strong> — one instance per [FlowContext](FlowContext.md 'FlowT\.FlowContext') execution,
shared across all pipeline stages (specifications, policies, handler) within the same flow.
This enables plugins to accumulate state (metrics, trace spans, audit entries) across the entire pipeline.

The implementation is registered as <strong>Transient</strong> in the DI container.
[FlowContext](FlowContext.md 'FlowT\.FlowContext') manages caching so each flow execution gets exactly one instance.

```csharp
// Program.cs
builder.Services.AddFlowPlugin<IRequestMetrics, RequestMetricsCollector>();

// Handler / Policy / Specification
var metrics = context.Plugin<IRequestMetrics>();
metrics.RecordDbQuery(elapsed);
```