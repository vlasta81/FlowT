## FlowEndpointExtensions Class

Extension methods for mapping flows to HTTP endpoints\.

```csharp
public static class FlowEndpointExtensions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowEndpointExtensions

| Methods | |
| :--- | :--- |
| [MapFlow&lt;TFlow,TRequest,TResponse&gt;\(this IEndpointRouteBuilder, string, string\)](FlowEndpointExtensions.MapFlow.ZBG6HKQSPLZH3C384SRU1AT8.md 'FlowT\.Extensions\.FlowEndpointExtensions\.MapFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder, string, string\)') | Maps a flow to an HTTP endpoint with automatic request binding and response serialization\. The flow is resolved from dependency injection and executed with a new [FlowContext](FlowContext.md 'FlowT\.FlowContext')\. |
