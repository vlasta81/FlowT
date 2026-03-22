## IFlowBuilder\<TRequest,TResponse\>\.Use\<TPolicy\>\(\) Method

Adds a policy \(decorator\) to the pipeline\.
Policies wrap the handler \(and other policies\) to provide cross\-cutting concerns like logging, transactions, retry, etc\.
Policies are applied in the order they are added \(outer to inner\)\.

```csharp
FlowT.Contracts.IFlowBuilder<TRequest,TResponse> Use<TPolicy>()
    where TPolicy : FlowT.Contracts.IFlowPolicy<TRequest, TResponse>;
```
#### Type parameters

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.Use_TPolicy_().TPolicy'></a>

`TPolicy`

The type of policy to add\.

#### Returns
[FlowT\.Contracts\.IFlowBuilder&lt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TRequest](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TRequest')[,](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')  
The builder instance for method chaining\.