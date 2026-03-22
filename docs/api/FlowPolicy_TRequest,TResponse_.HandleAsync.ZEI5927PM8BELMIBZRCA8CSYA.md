## FlowPolicy\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowContext\) Method

Handles the request, applying policy logic before and/or after calling the next handler\.

```csharp
public abstract System.Threading.Tasks.ValueTask<TResponse> HandleAsync(TRequest request, FlowT.FlowContext context);
```
#### Parameters

<a name='FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.HandleAsync(TRequest,FlowT.FlowContext).request'></a>

`request` [TRequest](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TRequest')

The request to process\.

<a name='FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.HandleAsync(TRequest,FlowT.FlowContext).context'></a>

`context` [FlowContext](FlowContext.md 'FlowT\.FlowContext')

The flow context providing shared state and services\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[TResponse](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1') representing the asynchronous operation\.