## IFlowBuilder\<TRequest,TResponse\>\.Check\<TSpec\>\(\) Method

Adds a specification \(guard\) to the pipeline\.
Specifications are executed sequentially before the handler\. If any specification returns an interrupt, the pipeline stops\.

```csharp
FlowT.Contracts.IFlowBuilder<TRequest,TResponse> Check<TSpec>()
    where TSpec : FlowT.Contracts.IFlowSpecification<TRequest>;
```
#### Type parameters

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.Check_TSpec_().TSpec'></a>

`TSpec`

The type of specification to add\.

#### Returns
[FlowT\.Contracts\.IFlowBuilder&lt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TRequest](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TRequest 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TRequest')[,](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')[TResponse](IFlowBuilder_TRequest,TResponse_.md#FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowBuilder_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>')  
The builder instance for method chaining\.