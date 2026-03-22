## FlowServiceCollectionExtensions\.AddFlowModules\(this IServiceCollection, Assembly\[\]\) Method

Scans the specified assemblies for classes marked with [FlowModuleAttribute](FlowModuleAttribute.md 'FlowT\.Attributes\.FlowModuleAttribute') and registers them\.
Each module's [Register\(IServiceCollection\)](IFlowModule.Register.ZE388V5XQHD23SSNAQ7TEOOCE.md 'FlowT\.Contracts\.IFlowModule\.Register\(Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') method is called to register its flows and services\.

```csharp
public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddFlowModules(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, params System.Reflection.Assembly[] assemblies);
```
#### Parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowModules(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection,System.Reflection.Assembly[]).services'></a>

`services` [Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')

The service collection to add modules to\.

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowModules(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection,System.Reflection.Assembly[]).assemblies'></a>

`assemblies` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

The assemblies to scan\. If empty, uses the calling assembly\.

#### Returns
[Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')  
The service collection for method chaining\.

### Remarks

This method requires types to be marked with [FlowModuleAttribute](FlowModuleAttribute.md 'FlowT\.Attributes\.FlowModuleAttribute') for explicit opt-in.
Types must also implement [IFlowModule](IFlowModule.md 'FlowT\.Contracts\.IFlowModule') and have a parameterless constructor.\<example\>
  \<code\>
            \[FlowModule\]
            public class UserModule : IFlowModule
            \{
                public void Register\(IServiceCollection services\) \{ \}
                public void MapEndpoints\(IEndpointRouteBuilder app\) \{ \}
            \}
            \</code\>
\</example\>