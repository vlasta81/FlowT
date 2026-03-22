## FlowDefinition\<TRequest,TResponse\> Class

Abstract base class for defining flows\. 
Each flow represents a complete use\-case with its own pipeline of specifications, policies, and handler\.

```csharp
public abstract class FlowDefinition<TRequest,TResponse> : FlowT.Contracts.IFlow<TRequest, TResponse>
```
#### Type parameters

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request this flow processes\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response this flow produces\.

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowDefinition\<TRequest,TResponse\>

Implements [FlowT\.Contracts\.IFlow&lt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')[TRequest](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TRequest')[,](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')[TResponse](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TResponse')[&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')

### Remarks
Flows use a fluent API in the [Configure\(IFlowBuilder&lt;TRequest,TResponse&gt;\)](FlowDefinition_TRequest,TResponse_.Configure.G5IBOHYAF75GAD6HUZKJ4HLW1.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.Configure\(FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\)') method to define their pipeline\.
The pipeline is built once at startup and cached for optimal runtime performance\.
Execution follows this order: Specifications \(guards\) → Policies \(decorators\) → Handler \(business logic\)\.

| Methods | |
| :--- | :--- |
| [Configure\(IFlowBuilder&lt;TRequest,TResponse&gt;\)](FlowDefinition_TRequest,TResponse_.Configure.G5IBOHYAF75GAD6HUZKJ4HLW1.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.Configure\(FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\)') | Configures the flow pipeline using a fluent builder API\. This method is called once during initialization to define the execution chain\. |
| [ExecuteAsync\(TRequest, FlowContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') | Executes the flow pipeline asynchronously\. Ensures the pipeline is initialized, runs all specifications, then invokes the handler chain\. |
| [ExecuteAsync\(TRequest, HttpContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)') | Executes the flow pipeline asynchronously from an HTTP context\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\. This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\. |
| [ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, System\.IServiceProvider, System\.Threading\.CancellationToken\)') | Executes the flow pipeline asynchronously from a service provider and cancellation token\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\. This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\. |
