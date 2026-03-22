## IFlowSpecification\<TRequest\>\.CheckAsync\(TRequest, FlowContext\) Method

Checks whether the request satisfies this specification\.

```csharp
System.Threading.Tasks.ValueTask<System.Nullable<FlowT.FlowInterrupt<object?>>> CheckAsync(TRequest request, FlowT.FlowContext context);
```
#### Parameters

<a name='FlowT.Contracts.IFlowSpecification_TRequest_.CheckAsync(TRequest,FlowT.FlowContext).request'></a>

`request` [TRequest](IFlowSpecification_TRequest_.md#FlowT.Contracts.IFlowSpecification_TRequest_.TRequest 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>\.TRequest')

The request to validate\.

<a name='FlowT.Contracts.IFlowSpecification_TRequest_.CheckAsync(TRequest,FlowT.FlowContext).context'></a>

`context` [FlowContext](FlowContext.md 'FlowT\.FlowContext')

The flow context providing access to shared state and services\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
A [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') containing either:
- `null` if validation passes (pipeline continues),
- or a [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') if validation fails (pipeline stops).