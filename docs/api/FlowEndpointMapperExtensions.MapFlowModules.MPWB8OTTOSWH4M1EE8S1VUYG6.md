## FlowEndpointMapperExtensions\.MapFlowModules\(this IEndpointRouteBuilder\) Method

Maps all registered flow modules' endpoints to the application\.
This method enumerates all [IFlowModule](IFlowModule.md 'FlowT\.Contracts\.IFlowModule') instances registered in the dependency injection container
and calls their [MapEndpoints\(IEndpointRouteBuilder\)](IFlowModule.MapEndpoints.PIXQMFCULRCV7AUDTC6OLRBBC.md 'FlowT\.Contracts\.IFlowModule\.MapEndpoints\(Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder\)') method to register HTTP endpoints\.

```csharp
public static void MapFlowModules(this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app);
```
#### Parameters

<a name='FlowT.Extensions.FlowEndpointMapperExtensions.MapFlowModules(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder).app'></a>

`app` [Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.iendpointroutebuilder 'Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder')

The endpoint route builder where endpoints will be mapped\.

### Remarks
This method should be called in the application's startup/configuration after services are registered\.
It provides a centralized way to discover and map all flow module endpoints without explicitly
calling each module's mapping method individually\.

Example usage:

```csharp
var app = builder.Build();
app.MapFlowModules(); // Automatically maps all module endpoints
app.Run();
```