## FlowModuleAttribute Class

Marks a class as a flow module for automatic discovery and registration\.
Apply this attribute to classes that implement [IFlowModule](IFlowModule.md 'FlowT\.Contracts\.IFlowModule')\.

```csharp
public sealed class FlowModuleAttribute : System.Attribute
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [System\.Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.attribute 'System\.Attribute') &#129106; FlowModuleAttribute

### Remarks

This attribute is <strong>required</strong> for automatic registration by [AddFlowModules\(this IServiceCollection, Assembly\[\]\)](FlowServiceCollectionExtensions.AddFlowModules.CRABQVK4FE6FJR6BTVIJUR4R9.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlowModules\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection, System\.Reflection\.Assembly\[\]\)').
Without this attribute, the module will not be discovered during assembly scanning.

<strong>Why required?</strong> Explicit opt-in provides clear intent and prevents accidental registration
            of helper classes or test modules. It enables vertical slice architecture by clearly identifying feature boundaries.

Modules provide a way to organize flows, services, and endpoints using vertical slice architecture.
Each module encapsulates all code for a specific feature or bounded context.\<example\>
  \<code\>
            \[FlowModule\]
            public class UserModule : IFlowModule
            \{
                public void Register\(IServiceCollection services\)
                \{
                    services\.AddFlows\(typeof\(UserModule\)\.Assembly\);
                    services\.AddScoped&lt;IUserRepository, UserRepository&gt;\(\);
                \}
                
                public void MapEndpoints\(IEndpointRouteBuilder app\)
                \{
                    app\.MapPost\("/api/users", \.\.\.\);
                \}
            \}
            \</code\>
\</example\>

| Constructors | |
| :--- | :--- |
| [FlowModuleAttribute\(\)](FlowModuleAttribute.FlowModuleAttribute().md 'FlowT\.Attributes\.FlowModuleAttribute\.FlowModuleAttribute\(\)') | Initializes a new instance of the [FlowModuleAttribute](FlowModuleAttribute.md 'FlowT\.Attributes\.FlowModuleAttribute') class\. |
