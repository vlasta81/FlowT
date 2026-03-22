## FlowEndpointInfoAttribute Class

Provides OpenAPI/Swagger metadata for flow endpoints\.
Apply this attribute to flow classes to customize their documentation in API explorers\.

```csharp
public sealed class FlowEndpointInfoAttribute : System.Attribute
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [System\.Attribute](https://learn.microsoft.com/en-us/dotnet/api/system.attribute 'System\.Attribute') &#129106; FlowEndpointInfoAttribute

### Remarks
This attribute is read by [MapFlow&lt;TFlow,TRequest,TResponse&gt;\(this IEndpointRouteBuilder, string, string\)](FlowEndpointExtensions.MapFlow.ZBG6HKQSPLZH3C384SRU1AT8.md 'FlowT\.Extensions\.FlowEndpointExtensions\.MapFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder, string, string\)') to apply metadata to generated endpoints\.
The actual HTTP route and method are defined in the module's [MapEndpoints\(IEndpointRouteBuilder\)](IFlowModule.MapEndpoints.PIXQMFCULRCV7AUDTC6OLRBBC.md 'FlowT\.Contracts\.IFlowModule\.MapEndpoints\(Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder\)') method\.

| Constructors | |
| :--- | :--- |
| [FlowEndpointInfoAttribute\(string\[\]\)](FlowEndpointInfoAttribute..ctor.KOUN6NSNL42CFHVJ4ACRPUNB2.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute\.FlowEndpointInfoAttribute\(string\[\]\)') | Initializes a new instance of the [FlowEndpointInfoAttribute](FlowEndpointInfoAttribute.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute') class with the specified tags\. |

| Properties | |
| :--- | :--- |
| [Description](FlowEndpointInfoAttribute.Description.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute\.Description') | Gets or sets the detailed description for the endpoint \(can be multi\-line\)\. |
| [Summary](FlowEndpointInfoAttribute.Summary.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute\.Summary') | Gets or sets the summary description for the endpoint \(short one\-line description\)\. |
| [Tags](FlowEndpointInfoAttribute.Tags.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute\.Tags') | Gets or sets the tags used to group endpoints in API documentation\. |
