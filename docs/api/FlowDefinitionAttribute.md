## FlowDefinitionAttribute Class

Marks a class as a flow definition for automatic discovery and registration\.
Apply this attribute to classes that inherit from [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')\.

```csharp
public sealed class FlowDefinitionAttribute : System.Attribute
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [System\.Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.attribute 'System\.Attribute') &#129106; FlowDefinitionAttribute

### Remarks

This attribute is <strong>required</strong> for automatic registration by [AddFlows\(this IServiceCollection, Assembly\[\]\)](FlowServiceCollectionExtensions.AddFlows.X7RY68MYH6QHDKD789FMOXD6C.md 'FlowT\.Extensions\.FlowServiceCollectionExtensions\.AddFlows\(this Microsoft\.Extensions\.DependencyInjection\.IServiceCollection, System\.Reflection\.Assembly\[\]\)').
Without this attribute, the flow will not be discovered during assembly scanning.

<strong>Why required?</strong> Explicit opt-in prevents accidental registration of base classes, 
            test fixtures, or internal flows not meant for production use. It also improves code readability
            by clearly marking which flows are part of the application's public API.\<example\>
  \<code\>
            \[FlowDefinition\]
            public class CreateUserFlow : FlowDefinition&lt;CreateUserRequest, CreateUserResponse&gt;
            \{
                protected override void Configure\(IFlowBuilder&lt;CreateUserRequest, CreateUserResponse&gt; flow\)
                \{
                    flow\.Handle&lt;CreateUserHandler&gt;\(\);
                \}
            \}
            \</code\>
\</example\>

| Constructors | |
| :--- | :--- |
| [FlowDefinitionAttribute\(\)](FlowDefinitionAttribute.FlowDefinitionAttribute().md 'FlowT\.Attributes\.FlowDefinitionAttribute\.FlowDefinitionAttribute\(\)') | Initializes a new instance of the [FlowDefinitionAttribute](FlowDefinitionAttribute.md 'FlowT\.Attributes\.FlowDefinitionAttribute') class\. |
