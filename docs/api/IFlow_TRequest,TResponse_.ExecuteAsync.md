#### [FlowT](index.md 'index')
### [FlowT\.Contracts](FlowT.Contracts.md 'FlowT\.Contracts').[IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')

## IFlow\<TRequest,TResponse\>\.ExecuteAsync Method

| Overloads | |
| :--- | :--- |
| [ExecuteAsync\(TRequest, FlowContext\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') | Executes the flow pipeline asynchronously with an existing flow context\. Use this method when executing sub\-flows to share the same context \(especially FlowId\)\. |
| [ExecuteAsync\(TRequest, HttpContext\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)') | Executes the flow pipeline asynchronously from an HTTP context\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\. This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\. |
| [ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\)](IFlow_TRequest,TResponse_.ExecuteAsync.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken) 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, System\.IServiceProvider, System\.Threading\.CancellationToken\)') | Executes the flow pipeline asynchronously from a service provider and cancellation token\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\. This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\. |

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext)'></a>

## IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowContext\) Method

Executes the flow pipeline asynchronously with an existing flow context\.
Use this method when executing sub\-flows to share the same context \(especially FlowId\)\.

```csharp
System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, FlowT.FlowContext context);
```
#### Parameters

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext).request'></a>

`request` [TRequest](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TRequest')

The request object containing input data for the flow\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext).context'></a>

`context` [FlowContext](FlowContext.md 'FlowT\.FlowContext')

The flow context providing shared state, services, and execution metadata\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext)'></a>

## IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, HttpContext\) Method

Executes the flow pipeline asynchronously from an HTTP context\.
Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\.
This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\.

```csharp
System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, Microsoft.AspNetCore.Http.HttpContext httpContext);
```
#### Parameters

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).request'></a>

`request` [TRequest](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TRequest')

The request object containing input data for the flow\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext'></a>

`httpContext` [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext')

The HTTP context providing services \([Microsoft\.AspNetCore\.Http\.HttpContext\.RequestServices](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestservices 'Microsoft\.AspNetCore\.Http\.HttpContext\.RequestServices')\), cancellation token \([Microsoft\.AspNetCore\.Http\.HttpContext\.RequestAborted](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted 'Microsoft\.AspNetCore\.Http\.HttpContext\.RequestAborted')\), and HTTP metadata\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.

### Remarks
This method is equivalent to manually creating a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with:

```csharp
var context = new FlowContext
{
    Services = httpContext.RequestServices,
    CancellationToken = httpContext.RequestAborted,
    HttpContext = httpContext
};
await flow.ExecuteAsync(request, context);
```
Each invocation creates a new [FlowContext](FlowContext.md 'FlowT\.FlowContext') with a unique [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId') for correlation\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken)'></a>

## IFlow\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\) Method

Executes the flow pipeline asynchronously from a service provider and cancellation token\.
Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\.
This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\.

```csharp
System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, System.IServiceProvider serviceProvider, System.Threading.CancellationToken ct);
```
#### Parameters

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).request'></a>

`request` [TRequest](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TRequest')

The request object containing input data for the flow\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).serviceProvider'></a>

`serviceProvider` [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider')

The service provider for dependency injection\.

<a name='FlowT.Contracts.IFlow_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).ct'></a>

`ct` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](IFlow_TRequest,TResponse_.md#FlowT.Contracts.IFlow_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.

### Remarks
This method is equivalent to manually creating a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with:

```csharp
var context = new FlowContext
{
    Services = serviceProvider,
    CancellationToken = ct,
    HttpContext = null
};
await flow.ExecuteAsync(request, context);
```
Use this method when executing flows outside of HTTP requests:
- Background services (IHostedService, BackgroundService)
- Message queue consumers (RabbitMQ, Azure Service Bus, etc.)
- Console applications
- Unit/integration tests
- Blazor WebAssembly (client-side, no server HttpContext)

Each invocation creates a new [FlowContext](FlowContext.md 'FlowT\.FlowContext') with a unique [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId') for correlation\.

---
Generated by [DefaultDocumentation](https://github.com/Doraku/DefaultDocumentation 'https://github\.com/Doraku/DefaultDocumentation')