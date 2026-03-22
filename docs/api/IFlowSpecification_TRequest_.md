## IFlowSpecification\<TRequest\> Interface

Represents a guard specification that validates a request before it reaches the handler\.
Specifications implement business rules and validation logic\.
If validation fails, the specification returns a [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') which stops pipeline execution\.

```csharp
public interface IFlowSpecification<in TRequest>
```
#### Type parameters

<a name='FlowT.Contracts.IFlowSpecification_TRequest_.TRequest'></a>

`TRequest`

The type of request to validate\.

| Methods | |
| :--- | :--- |
| [CheckAsync\(TRequest, FlowContext\)](IFlowSpecification_TRequest_.CheckAsync.NZM8Q48866JUC17QS9N2RABZ8.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>\.CheckAsync\(TRequest, FlowT\.FlowContext\)') | Checks whether the request satisfies this specification\. |
