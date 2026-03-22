## IFlowHandler\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowContext\) Method

Handles the request asynchronously and produces a response\.

```csharp
System.Threading.Tasks.ValueTask<TResponse> HandleAsync(TRequest request, FlowT.FlowContext context);
```
#### Parameters

<a name='FlowT.Contracts.IFlowHandler_TRequest,TResponse_.HandleAsync(TRequest,FlowT.FlowContext).request'></a>

`request` [TRequest](IFlowHandler_TRequest,TResponse_.md#FlowT.Contracts.IFlowHandler_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>\.TRequest')

The request to process\.

<a name='FlowT.Contracts.IFlowHandler_TRequest,TResponse_.HandleAsync(TRequest,FlowT.FlowContext).context'></a>

`context` [FlowContext](FlowContext.md 'FlowT\.FlowContext')

The flow context providing shared state and services\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](IFlowHandler_TRequest,TResponse_.md#FlowT.Contracts.IFlowHandler_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation that produces the response\.