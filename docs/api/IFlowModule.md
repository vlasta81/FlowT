## IFlowModule Interface

Represents a modular unit that groups related flows, services, and endpoint mappings\.
Modules provide a way to organize features using vertical slice architecture\.

```csharp
public interface IFlowModule
```

| Methods | |
| :--- | :--- |
| [MapEndpoints\(IEndpointRouteBuilder\)](IFlowModule.MapEndpoints.PIXQMFCULRCV7AUDTC6OLRBBC.md 'FlowT\.Contracts\.IFlowModule\.MapEndpoints\(Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder\)') | Maps HTTP endpoints for flows defined in this module\. |
| [Register\(IServiceCollection\)](IFlowModule.Register.ZE388V5XQHD23SSNAQ7TEOOCE.md 'FlowT\.Contracts\.IFlowModule\.Register\(Microsoft\.Extensions\.DependencyInjection\.IServiceCollection\)') | Registers flows and services required by this module into the dependency injection container\. |
