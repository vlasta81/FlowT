## FlowDefinition\<TRequest,TResponse\>\.Configure\(IFlowBuilder\<TRequest,TResponse\>\) Method

Configures the flow pipeline using a fluent builder API\.
This method is called once during initialization to define the execution chain\.

```csharp
protected abstract void Configure(FlowT.Contracts.IFlowBuilder<TRequest,TResponse> flow);
```
#### Parameters

<a name='FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.Configure(FlowT.Contracts.IFlowBuilder_TRequest,TResponse_).flow'></a>

`flow` [FlowT\.Contracts\.IFlowBuilder&lt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TRequest](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TRequest')[,](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TResponse](FlowDefinition_TRequest,TResponse_.md#FlowT.Abstractions.FlowDefinition_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')

The builder used to configure specifications, policies, and the handler\.

### Remarks
This method must call [Handle&lt;THandler&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Handle_THandler_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Handle\<THandler\>\(\)') exactly once\.
The order of calls determines the execution order:
1. [Check&lt;TSpec&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Check_TSpec_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Check\<TSpec\>\(\)') - validation/guard logic (executed sequentially)
2. [Use&lt;TPolicy&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Use_TPolicy_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Use\<TPolicy\>\(\)') - cross-cutting concerns (wrapped outer to inner)
3. [Handle&lt;THandler&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Handle_THandler_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Handle\<THandler\>\(\)') - main business logic