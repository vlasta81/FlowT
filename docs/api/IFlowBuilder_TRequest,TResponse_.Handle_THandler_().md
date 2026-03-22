## IFlowBuilder\<TRequest,TResponse\>\.Handle\<THandler\>\(\) Method

Sets the main handler for this flow\.
The handler contains the core business logic\. This method must be called exactly once per flow\.

```csharp
FlowT.Contracts.IFlowBuilder<TRequest,TResponse> Handle<THandler>()
    where THandler : FlowT.Contracts.IFlowHandler<TRequest, TResponse>;
```
#### Type parameters

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.Handle_THandler_().THandler'></a>

`THandler`

The type of handler to use\.

#### Returns
[FlowT\.Contracts\.IFlowBuilder&lt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TRequest](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TRequest')[,](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')  
The builder instance for method chaining\.