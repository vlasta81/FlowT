#### [FlowT](index.md 'index')
### [FlowT\.Abstractions](FlowT.Abstractions.md 'FlowT\.Abstractions').[FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')

## FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync Method

| Overloads | |
| :--- | :--- |
| [ExecuteAsync\(TRequest, FlowContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') | Executes the flow pipeline asynchronously\. Ensures the pipeline is initialized, runs all specifications, then invokes the handler chain\. |
| [ExecuteAsync\(TRequest, HttpContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)') | Executes the flow pipeline asynchronously from an HTTP context\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\. This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\. |
| [ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, System\.IServiceProvider, System\.Threading\.CancellationToken\)') | Executes the flow pipeline asynchronously from a service provider and cancellation token\. Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\. This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\. |

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext)'></a>

## FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowContext\) Method

Executes the flow pipeline asynchronously\.
Ensures the pipeline is initialized, runs all specifications, then invokes the handler chain\.

```csharp
public System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, FlowT.FlowContext context);
```
#### Parameters

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext).request'></a>

`request` [TRequest](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TRequest')

The request to process\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext).context'></a>

`context` [FlowContext](FlowContext.md 'FlowT\.FlowContext')

The flow context providing shared state and services\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation\.

### Remarks
This method uses an optimized hot\-path for synchronous specifications \(checks [System\.Threading\.Tasks\.ValueTask&lt;&gt;\.IsCompletedSuccessfully](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1.iscompletedsuccessfully 'System\.Threading\.Tasks\.ValueTask\`1\.IsCompletedSuccessfully')\)\.
If any specification returns a [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>'), the pipeline stops immediately and returns the mapped response\.
Use this overload when executing sub\-flows where you want to share the same [FlowContext](FlowContext.md 'FlowT\.FlowContext') \(especially FlowId\)\.
For main flow execution, use [ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, System\.IServiceProvider, System\.Threading\.CancellationToken\)') instead\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext)'></a>

## FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, HttpContext\) Method

Executes the flow pipeline asynchronously from an HTTP context\.
Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') with services, cancellation token, and HTTP context from the provided [httpContext](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, Microsoft\.AspNetCore\.Http\.HttpContext\)\.httpContext')\.
This is a convenience method for ASP\.NET Core scenarios where you have [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext') available\.

```csharp
public System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, Microsoft.AspNetCore.Http.HttpContext httpContext);
```
#### Parameters

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).request'></a>

`request` [TRequest](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TRequest')

The request object containing input data for the flow\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,Microsoft.AspNetCore.Http.HttpContext).httpContext'></a>

`httpContext` [Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext')

The HTTP context providing services \([Microsoft\.AspNetCore\.Http\.HttpContext\.RequestServices](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestservices 'Microsoft\.AspNetCore\.Http\.HttpContext\.RequestServices')\), cancellation token \([Microsoft\.AspNetCore\.Http\.HttpContext\.RequestAborted](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.requestaborted 'Microsoft\.AspNetCore\.Http\.HttpContext\.RequestAborted')\), and HTTP metadata\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.

### Remarks
This method creates a new [FlowContext](FlowContext.md 'FlowT\.FlowContext') with a unique [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId') for correlation\.
The HTTP context is passed to handlers, allowing access to:
- User authentication and claims ([Microsoft\.AspNetCore\.Http\.HttpContext\.User](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.user 'Microsoft\.AspNetCore\.Http\.HttpContext\.User'))
- Request headers, query parameters, cookies ([Microsoft\.AspNetCore\.Http\.HttpContext\.Request](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.request 'Microsoft\.AspNetCore\.Http\.HttpContext\.Request'))
- Response control: status codes, headers, cookies ([Microsoft\.AspNetCore\.Http\.HttpContext\.Response](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.response 'Microsoft\.AspNetCore\.Http\.HttpContext\.Response'))
- Connection information: IP addresses ([Microsoft\.AspNetCore\.Http\.HttpContext\.Connection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.connection 'Microsoft\.AspNetCore\.Http\.HttpContext\.Connection'))
- Per-request storage ([Microsoft\.AspNetCore\.Http\.HttpContext\.Items](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.items 'Microsoft\.AspNetCore\.Http\.HttpContext\.Items'))

Use [ExecuteAsync\(TRequest, FlowContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') when executing sub\-flows to share the same [FlowContext](FlowContext.md 'FlowT\.FlowContext') \(especially [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId')\)\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken)'></a>

## FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, IServiceProvider, CancellationToken\) Method

Executes the flow pipeline asynchronously from a service provider and cancellation token\.
Automatically creates a [FlowContext](FlowContext.md 'FlowT\.FlowContext') without HTTP context\.
This is the method for non\-HTTP scenarios \(background jobs, message queue handlers, console apps, tests\)\.

```csharp
public System.Threading.Tasks.ValueTask<TResponse> ExecuteAsync(TRequest request, System.IServiceProvider serviceProvider, System.Threading.CancellationToken ct);
```
#### Parameters

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).request'></a>

`request` [TRequest](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TRequest')

The request object containing input data for the flow\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).serviceProvider'></a>

`serviceProvider` [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider')

The service provider for dependency injection\.

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,System.IServiceProvider,System.Threading.CancellationToken).ct'></a>

`ct` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

The cancellation token to observe for cancellation requests\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.

### Remarks
This method creates a new [FlowContext](FlowContext.md 'FlowT\.FlowContext') with a unique [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId') for correlation\.
The [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') property will be `null`\.
Use this method when executing flows outside of HTTP requests:
- Background services ([Microsoft\.Extensions\.Hosting\.IHostedService](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostedservice 'Microsoft\.Extensions\.Hosting\.IHostedService'), [Microsoft\.Extensions\.Hosting\.BackgroundService](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.backgroundservice 'Microsoft\.Extensions\.Hosting\.BackgroundService'))
- Message queue consumers (RabbitMQ, Azure Service Bus, etc.)
- Console applications
- Unit/integration tests
- Blazor WebAssembly (client-side, no server HttpContext)

Use [ExecuteAsync\(TRequest, FlowContext\)](FlowDefinition_TRequest,TResponse_.ExecuteAsync.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.ExecuteAsync(TRequest,FlowT.FlowContext) 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.ExecuteAsync\(TRequest, FlowT\.FlowContext\)') when executing sub\-flows to share the same [FlowContext](FlowContext.md 'FlowT\.FlowContext') \(especially [FlowId](FlowContext.FlowId.md 'FlowT\.FlowContext\.FlowId')\)\.

---
Generated by [DefaultDocumentation](https://github.com/Doraku/DefaultDocumentation 'https://github\.com/Doraku/DefaultDocumentation')