## IFlowPolicy\<TRequest,TResponse\> Interface

Marker interface for flow policies\.
Policies are decorators that wrap handlers to provide cross\-cutting concerns such as logging, transactions, retry, caching, etc\.
Policies form a chain of responsibility where each policy can execute logic before and after calling the next handler in the chain\.

```csharp
public interface IFlowPolicy<in TRequest,TResponse> : FlowT.Contracts.IFlowHandler<TRequest, TResponse>
```
#### Type parameters

<a name='FlowT.Contracts.IFlowPolicy_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request this policy processes\.

<a name='FlowT.Contracts.IFlowPolicy_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response this policy produces\.

Derived  
&#8627; [FlowPolicy&lt;TRequest,TResponse&gt;](FlowPolicy_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>')

### Remarks
To implement a policy, inherit from [FlowPolicy&lt;TRequest,TResponse&gt;](FlowPolicy_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>') instead of implementing this interface directly\.