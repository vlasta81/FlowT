## IFlowBuilder\<TRequest,TResponse\>\.OnInterrupt\(Func\<FlowInterrupt\<object\>,TResponse\>\) Method

Registers a mapper function to convert [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') results from specifications to typed responses\.
Invoked only when a [IFlowSpecification&lt;TRequest&gt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>') added via [Check&lt;TSpec&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Check_TSpec_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Check\<TSpec\>\(\)') returns a non\-null interrupt\.
Exceptions thrown by policies or the handler propagate normally and are \<b\>not\</b\> caught by this mapper\.
Can only be called once per flow\.

```csharp
FlowT.Contracts.IFlowBuilder<TRequest,TResponse> OnInterrupt(System.Func<FlowT.FlowInterrupt<object?>,TResponse> mapper);
```
#### Parameters

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.OnInterrupt(System.Func_FlowT.FlowInterrupt_object_,TResponse_).mapper'></a>

`mapper` [System\.Func&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[,](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')[TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')

A function that receives the interrupt \(with [Message](FlowInterrupt_TResponse_.Message.md 'FlowT\.FlowInterrupt\<TResponse\>\.Message'), [StatusCode](FlowInterrupt_TResponse_.StatusCode.md 'FlowT\.FlowInterrupt\<TResponse\>\.StatusCode'),
[IsFailure](FlowInterrupt_TResponse_.IsFailure.md 'FlowT\.FlowInterrupt\<TResponse\>\.IsFailure') and [IsEarlyReturn](FlowInterrupt_TResponse_.IsEarlyReturn.md 'FlowT\.FlowInterrupt\<TResponse\>\.IsEarlyReturn')\) and must return a [TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')\.
May throw instead of returning when the response type cannot carry error information\.

#### Returns
[FlowT\.Contracts\.IFlowBuilder&lt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TRequest](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TRequest')[,](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')  
The builder instance for method chaining\.