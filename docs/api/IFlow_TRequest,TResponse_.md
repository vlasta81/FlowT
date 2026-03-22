## IFlow\<TRequest,TResponse\> Interface

Represents the main entry point for executing a flow pipeline\.
This interface is implemented by [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>') and represents a complete flow execution unit\.

```csharp
public interface IFlow<in TRequest,TResponse>
```
#### Type parameters

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request processed by this flow\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response returned by this flow\.

Derived  
&#8627; [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')

| Methods | |
| :--- | :--- |
| [ExecuteAsync\(TRequest, FlowContext\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') | Executes the flow pipeline asynchronously with an existing flow context\. Use this method when executing sub\-flows to share the same context \(especially FlowId\)\. |
| [ExecuteAsync\(TRequest, HttpContext\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)') | Executes the flow pipeline asynchronously from an HTTP context\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\. This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\. |
| [ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, System\.IServiceProvider, System\.Threading\.CancellationToken\)') | Executes the flow pipeline asynchronously from a service provider and cancellation token\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\. This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\. |
