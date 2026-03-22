## IFlowBuilder\<TRequest,TResponse\> Interface

Fluent API builder for configuring a flow pipeline\.
Use this interface in [Configure\(IFlowBuilder&lt;TRequest,TResponse&gt;\)](FlowDefinition_TRequest,TResponse_.Configure.G5IBOHYAF75GAD6HUZKJ4HLW1.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.Configure\(FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\)') to define the execution chain\.

```csharp
public interface IFlowBuilder<TRequest,TResponse>
```
#### Type parameters

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request processed by this flow\.

<a name='FlowT.Contracts.IFlowBuilder_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response produced by this flow\.

| Methods | |
| :--- | :--- |
| [Check&lt;TSpec&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Check_TSpec_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Check\<TSpec\>\(\)') | Adds a specification \(guard\) to the pipeline\. Specifications are executed sequentially before the handler\. If any specification returns an interrupt, the pipeline stops\. |
| [Handle&lt;THandler&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Handle_THandler_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Handle\<THandler\>\(\)') | Sets the main handler for this flow\. The handler contains the core business logic\. This method must be called exactly once per flow\. |
| [OnInterrupt\(Func&lt;FlowInterrupt&lt;object&gt;,TResponse&gt;\)](IFlowBuilder_TRequest,TResponse_.OnInterrupt.GUDAEN4XYZGDLTYRH92RZPLYC.md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.OnInterrupt\(System\.Func\<FlowT\.FlowInterrupt\<object\>,TResponse\>\)') | Registers a mapper function to convert [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') results from specifications to typed responses\. Invoked only when a [IFlowSpecification&lt;TRequest&gt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>') added via [Check&lt;TSpec&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Check_TSpec_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Check\<TSpec\>\(\)') returns a non\-null interrupt\. Exceptions thrown by policies or the handler propagate normally and are \<b\>not\</b\> caught by this mapper\. Can only be called once per flow\. |
| [Use&lt;TPolicy&gt;\(\)](IFlowBuilder_TRequest,TResponse_.Use_TPolicy_().md 'FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\.Use\<TPolicy\>\(\)') | Adds a policy \(decorator\) to the pipeline\. Policies wrap the handler \(and other policies\) to provide cross\-cutting concerns like logging, transactions, retry, etc\. Policies are applied in the order they are added \(outer to inner\)\. |
