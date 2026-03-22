## FlowServiceCollectionExtensions\.AddFlow\<TFlow,TRequest,TResponse\>\(this IServiceCollection\) Method

Registers a single flow with dependency injection\.
The flow is registered as a singleton with both its concrete type and [IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>') interface\.
If the flow is already registered, the call is ignored \(no duplicate registrations\)\.

```csharp
public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddFlow<TFlow,TRequest,TResponse>(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    where TFlow : FlowT.Abstractions.FlowDefinition<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull;
```
#### Type parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlow_TFlow,TRequest,TResponse_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TFlow'></a>

`TFlow`

The flow type to register\. Must inherit from [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>') and be marked with [FlowDefinitionAttribute](FlowDefinitionAttribute.md 'FlowT\.Attributes\.FlowDefinitionAttribute')\.

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlow_TFlow,TRequest,TResponse_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TRequest'></a>

`TRequest`

The type of request the flow processes\.

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlow_TFlow,TRequest,TResponse_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).TResponse'></a>

`TResponse`

The type of response the flow produces\.
#### Parameters

<a name='FlowT.Extensions.FlowServiceCollectionExtensions.AddFlow_TFlow,TRequest,TResponse_(thisMicrosoft.Extensions.DependencyInjection.IServiceCollection).services'></a>

`services` [Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')

The service collection to add the flow to\.

#### Returns
[Microsoft\.Extensions\.DependencyInjection\.IServiceCollection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection 'Microsoft\.Extensions\.DependencyInjection\.IServiceCollection')  
The service collection for method chaining\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if the flow type is not marked with [FlowDefinitionAttribute](FlowDefinitionAttribute.md 'FlowT\.Attributes\.FlowDefinitionAttribute')\.

### Remarks

<strong>Duplicate Registration Protection:</strong>

This method uses [Microsoft\.Extensions\.DependencyInjection\.Extensions\.ServiceCollectionDescriptorExtensions\.TryAddSingleton&lt;&gt;\.Extensions\.DependencyInjection\.IServiceCollection\)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.tryaddsingleton--1#microsoft-extensions-dependencyinjection-extensions-servicecollectiondescriptorextensions-tryaddsingleton--1(microsoft-extensions-dependencyinjection-iservicecollection) 'Microsoft\.Extensions\.DependencyInjection\.Extensions\.ServiceCollectionDescriptorExtensions\.TryAddSingleton\`\`1\(Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') internally,
so calling it multiple times with the same flow type is safe - only the first registration is kept.
This prevents issues when multiple modules or code paths accidentally register the same flow.

<strong>Usage Patterns:</strong>
1. <strong>Modular projects (recommended):</strong> Use in [Register\(IServiceCollection\)](IFlowModule.Register.ZE388V5XQHD23SSNAQ7TEOOCE.md 'FlowT\.Contracts\.IFlowModule\.Register\(Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') to explicitly register flows per module
              
  
  ```csharp
  [FlowModule]
  public class UserModule : IFlowModule
  {
      public void Register(IServiceCollection services)
      {
          // ✅ Explicit flow registration
          services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
          services.AddFlow<UpdateUserFlow, UpdateUserRequest, UpdateUserResponse>();
          
          // Register only external dependencies (handlers/specs/policies are auto-created)
          services.AddSingleton<IUserRepository, UserRepository>();
      }
  }
  ```
2. <strong>Simple projects without modules:</strong> Use in Program.cs for standalone flows
              
  
  ```csharp
  builder.Services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
  ```
3. <strong>Hybrid approach:</strong> Use [AddFlowModules\(this IServiceCollection, Assembly\[\]\)](FlowServiceCollectionExtensions.AddFlowModules.CRABQVK4FE6FJR6BTVIJUR4R9.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowModules\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection, System\.Reflection\.Assembly\[\]\)') for organized features + AddFlow for standalone flows
              
  
  ```csharp
  builder.Services.AddFlowModules(typeof(Program).Assembly);  // Registers modules
  builder.Services.AddFlow<HealthCheckFlow, HealthCheckRequest, HealthCheckResponse>();  // Standalone flow
  ```

<strong>Automatic Dependency Construction:</strong>

Handlers, specifications, and policies are automatically constructed using [Microsoft\.Extensions\.DependencyInjection\.ActivatorUtilities\.CreateInstance\(System\.IServiceProvider,System\.Type,System\.Object\[\]\)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.activatorutilities.createinstance#microsoft-extensions-dependencyinjection-activatorutilities-createinstance(system-iserviceprovider-system-type-system-object[]) 'Microsoft\.Extensions\.DependencyInjection\.ActivatorUtilities\.CreateInstance\(System\.IServiceProvider,System\.Type,System\.Object\[\]\)').
You do NOT need to register them manually unless you want to override the default construction (e.g., for singleton state, mocking, or custom factories).

```csharp
// ✅ Handler is auto-created with dependencies from DI
public class CreateUserHandler(ILogger<CreateUserHandler> logger, IUserRepository repo) 
    : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    public async ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
    {
        // ✅ Scoped services resolved per-request
        var db = context.Service<AppDbContext>();
        // ...
    }
}
```

<strong>⚠️ Important:</strong> Flow type must be marked with [FlowDefinitionAttribute](FlowDefinitionAttribute.md 'FlowT\.Attributes\.FlowDefinitionAttribute').

<strong>Performance:</strong> Flows are cached as singletons. The pipeline (handler + policies + specs) is lazily initialized on first use and reused across all requests.

<strong>🔒 Security consideration:</strong>

Flows are registered as <strong>singletons</strong> and shared across all concurrent requests for performance.
This means you must <strong>NEVER</strong> store per-request data (request/response objects, user data, FlowContext, etc.) in instance fields.
Always use [FlowContext](FlowContext.md 'FlowT\.FlowContext') for per-request state to prevent data leaks between users.
FlowT analyzers (FlowT001-FlowT019) detect common violations at compile-time.\<example\>
  \<strong\>❌ UNSAFE \- Data leak:\</strong\>
  \<code\>
            public class UserHandler : IFlowHandler&lt;CreateUserRequest, CreateUserResponse&gt;
            \{
                private CreateUserRequest? \_currentRequest; // ❌ Shared between all users\!
                
                public async ValueTask&lt;CreateUserResponse&gt; HandleAsync\(CreateUserRequest request, FlowContext context\)
                \{
                    \_currentRequest = request; // ❌ User A sees User B's request\!
                    // \.\.\.
                \}
            \}
            \</code\>
  \<strong\>✅ SAFE \- Per\-request isolation:\</strong\>
  \<code\>
            public class UserHandler : IFlowHandler&lt;CreateUserRequest, CreateUserResponse&gt;
            \{
                private readonly ILogger \_logger; // ✅ Readonly dependencies are safe
                
                public async ValueTask&lt;CreateUserResponse&gt; HandleAsync\(CreateUserRequest request, FlowContext context\)
                \{
                    // ✅ Use context for per\-request state
                    context\.Set\("currentRequest", request\);
                    
                    // ✅ Resolve scoped services per\-request
                    var db = context\.Service&lt;AppDbContext&gt;\(\);
                    
                    // ✅ Local variables are safe
                    var tempData = new List&lt;string&gt;\(\);
                    
                    return new CreateUserResponse\(\);
                \}
            \}
            \</code\>
\</example\>