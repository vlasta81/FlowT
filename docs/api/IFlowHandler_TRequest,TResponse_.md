## IFlowHandler\<TRequest,TResponse\> Interface

Represents a handler that processes a request and produces a response\.
Handlers contain the main business logic of a flow\. They can also be wrapped by policies\.

```csharp
public interface IFlowHandler<in TRequest,TResponse>
```
#### Type parameters

<a name='FlowT.Contracts.IFlowHandler_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request this handler processes\.

<a name='FlowT.Contracts.IFlowHandler_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response this handler produces\.

Derived  
&#8627; [FlowPolicy&lt;TRequest,TResponse&gt;](FlowPolicy_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>')  
&#8627; [IFlowPolicy&lt;TRequest,TResponse&gt;](IFlowPolicy_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowPolicy\<TRequest,TResponse\>')

| Methods | |
| :--- | :--- |
| [HandleAsync\(TRequest, FlowContext\)](IFlowHandler_TRequest,TResponse_.HandleAsync.N1GT85IT7Q6IH7COL3H8PI7IC.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowT\.FlowContext\)') | Handles the request asynchronously and produces a response\. |
