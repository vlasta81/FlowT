## IFlowScopePlugin Interface

Built\-in plugin that creates and exposes a dedicated [Microsoft\.Extensions\.DependencyInjection\.IServiceScope](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescope 'Microsoft\.Extensions\.DependencyInjection\.IServiceScope') for the current flow execution\.
Useful in non\-HTTP scenarios \(background jobs, message consumers, hosted services\) where ASP\.NET Core does not
automatically manage a per\-request DI scope\.

```csharp
public interface IFlowScopePlugin : System.IDisposable
```

Derived  
&#8627; [FlowScopePlugin](FlowScopePlugin.md 'FlowT\.Plugins\.FlowScopePlugin')

### Remarks
Register via `services.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>()`\.

<strong>Disposal responsibility:</strong> The plugin implements [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable').
             Because plugins are PerFlow singletons (not managed by a DI scope), the caller is responsible for
             disposing the plugin — and therefore the scope — after the flow completes.
             When used inside a [FlowContext](FlowContext.md 'FlowT\.FlowContext') that is created and owned by a hosted service or
             pipeline host, that host should dispose the plugin at the end of each unit of work.

Usage:

```csharp
var scopePlugin = context.Plugin<IFlowScopePlugin>();
var dbContext = scopePlugin.ScopedServices.GetRequiredService<AppDbContext>();

// ... use dbContext ...

scopePlugin.Dispose(); // dispose the scope when the flow is finished
```

| Properties | |
| :--- | :--- |
| [ScopedServices](IFlowScopePlugin.ScopedServices.md 'FlowT\.Plugins\.IFlowScopePlugin\.ScopedServices') | Gets the [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider') from the dedicated scope for this flow\. The scope is created lazily on the first access to this property\. |
