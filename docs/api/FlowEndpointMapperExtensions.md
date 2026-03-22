## FlowEndpointMapperExtensions Class

Extension methods for automatically mapping flow module endpoints to the application's route builder\.

```csharp
public static class FlowEndpointMapperExtensions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowEndpointMapperExtensions

| Methods | |
| :--- | :--- |
| [MapFlowModules\(this IEndpointRouteBuilder\)](FlowEndpointMapperExtensions.MapFlowModules.MPWB8OTTOSWH4M1EE8S1VUYG6.md 'FlowT\.Extensions\.FlowEndpointMapperExtensions\.MapFlowModules\(this Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder\)') | Maps all registered flow modules' endpoints to the application\. This method enumerates all [IFlowModule](IFlowModule.md 'FlowT\.Contracts\.IFlowModule') instances registered in the dependency injection container and calls their [MapEndpoints\(IEndpointRouteBuilder\)](IFlowModule.MapEndpoints.PIXQMFCULRCV7AUDTC6OLRBBC.md 'FlowT\.Contracts\.IFlowModule\.MapEndpoints\(Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder\)') method to register HTTP endpoints\. |
