## FlowPolicy\<TRequest,TResponse\>\.SetNext\(IFlowHandler\<TRequest,TResponse\>\) Method

Sets the next handler in the chain\. This method is called by the framework during pipeline construction\.

```csharp
protected internal void SetNext(FlowT.Contracts.IFlowHandler<TRequest,TResponse> next);
```
#### Parameters

<a name='FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.SetNext(FlowT.Contracts.IFlowHandler_TRequest,TResponse_).next'></a>

`next` [FlowT\.Contracts\.IFlowHandler&lt;](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')[TRequest](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TRequest')[,](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')[TResponse](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')

The next handler to call in the pipeline\.

### Remarks
This method is `protected internal` to allow test scenarios where policies need to be manually chained\.
In production, this is called automatically by the framework\.