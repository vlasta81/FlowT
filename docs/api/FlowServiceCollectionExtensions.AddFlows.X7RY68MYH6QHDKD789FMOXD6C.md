## FlowServiceCollectionExtensions\.AddFlows\(this IServiceCollection, Assembly\[\]\) Method

\[DEPRECATED\] Scans assemblies for flows\. Use [AddFlow&lt;TFlow,TRequest,TResponse&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlow.IN2LROACCX9J5TGD4ZZ7N0FB4.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') in modules instead\.

```csharp
public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddFlows(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, params System.Reflection.Assembly[] assemblies);
```
#### Parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlows(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection,System.Reflection.Assembly[]).services'></a>

`services` [Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')

The service collection to add flows to\.

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlows(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection,System.Reflection.Assembly[]).assemblies'></a>

`assemblies` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

The assemblies to scan\. If empty, uses the calling assembly\.

#### Returns
[Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')  
The service collection for method chaining\.

### Remarks

<strong>⚠️ This method is deprecated and may cause duplicate registrations when used with <see cref="M:FlowT.Extensions.FlowServiceCollectionExtensions.AddFlowModules(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Reflection.Assembly[])"/>.</strong>

<strong>Migration Guide:</strong>
- <strong>In modules:</strong> Use [AddFlow&lt;TFlow,TRequest,TResponse&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlow.IN2LROACCX9J5TGD4ZZ7N0FB4.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') to explicitly register each flow
              
  
  ```csharp
  [FlowModule]
  public class UserModule : IFlowModule
  {
      public void Register(IServiceCollection services)
      {
          // ✅ NEW: Explicit per-flow registration
          services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
          services.AddFlow<UpdateUserFlow, UpdateUserRequest, UpdateUserResponse>();
          
          // ❌ OLD: services.AddFlows(typeof(UserModule).Assembly);
      }
  }
  ```
- <strong>In simple projects without modules:</strong> Use [AddFlow&lt;TFlow,TRequest,TResponse&gt;\(this IServiceCollection\)](FlowServiceCollectionExtensions.AddFlow.IN2LROACCX9J5TGD4ZZ7N0FB4.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') in Program.cs
              
  
  ```csharp
  builder.Services.AddFlow<SimpleFlow, SimpleRequest, SimpleResponse>();
  ```

Types must be marked with [FlowDefinitionAttribute](FlowDefinitionAttribute.md 'FlowT\.Attributes\.FlowDefinitionAttribute') and inherit from [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>').