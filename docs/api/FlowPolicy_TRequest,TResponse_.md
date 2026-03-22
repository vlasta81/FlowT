## FlowPolicy\<TRequest,TResponse\> Class

Abstract base class for implementing flow policies \(decorators\) using the Chain of Responsibility pattern\.
Policies wrap handlers to provide cross\-cutting concerns such as logging, transactions, caching, retry logic, etc\.

```csharp
public abstract class FlowPolicy<TRequest,TResponse> : FlowT.Contracts.IFlowPolicy<TRequest, TResponse>, FlowT.Contracts.IFlowHandler<TRequest, TResponse>
```
#### Type parameters

<a name='FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TRequest'></a>

`TRequest`

The type of request this policy processes\.

<a name='FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TResponse'></a>

`TResponse`

The type of response this policy produces\.

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowPolicy\<TRequest,TResponse\>

Implements [FlowT\.Contracts\.IFlowPolicy&lt;](IFlowPolicy_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowPolicy\<TRequest,TResponse\>')[TRequest](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TRequest')[,](IFlowPolicy_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowPolicy\<TRequest,TResponse\>')[TResponse](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowPolicy_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowPolicy\<TRequest,TResponse\>'), [FlowT\.Contracts\.IFlowHandler&lt;](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')[TRequest](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TRequest 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TRequest')[,](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')[TResponse](FlowPolicy_TRequest,TResponse_.md#FlowT.Abstractions.FlowPolicy_TRequest,TResponse_.TResponse 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.TResponse')[&gt;](IFlowHandler_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>')

### Remarks
Policies are chained together at startup\. Each policy receives a reference to the next handler in the chain via [Next](FlowPolicy_TRequest,TResponse_.Next.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.Next')\.
Override [HandleAsync\(TRequest, FlowContext\)](FlowPolicy_TRequest,TResponse_.HandleAsync.ZEI5927PM8BELMIBZRCA8CSYA.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowT\.FlowContext\)') to implement your policy logic, typically wrapping a call to `await Next.HandleAsync(request, context)`\.

| Fields | |
| :--- | :--- |
| [Next](FlowPolicy_TRequest,TResponse_.Next.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.Next') | Gets the next handler in the pipeline chain\. This is set automatically by the framework during pipeline construction\. Call this from your [HandleAsync\(TRequest, FlowContext\)](FlowPolicy_TRequest,TResponse_.HandleAsync.ZEI5927PM8BELMIBZRCA8CSYA.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowT\.FlowContext\)') implementation to continue the pipeline\. |

| Methods | |
| :--- | :--- |
| [HandleAsync\(TRequest, FlowContext\)](FlowPolicy_TRequest,TResponse_.HandleAsync.ZEI5927PM8BELMIBZRCA8CSYA.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.HandleAsync\(TRequest, FlowT\.FlowContext\)') | Handles the request, applying policy logic before and/or after calling the next handler\. |
| [SetNext\(IFlowHandler&lt;TRequest,TResponse&gt;\)](FlowPolicy_TRequest,TResponse_.SetNext.KX0NCCWVOD11RV5WS8287E3M6.md 'FlowT\.Abstractions\.FlowPolicy\<TRequest,TResponse\>\.SetNext\(FlowT\.Contracts\.IFlowHandler\<TRequest,TResponse\>\)') | Sets the next handler in the chain\. This method is called by the framework during pipeline construction\. |
