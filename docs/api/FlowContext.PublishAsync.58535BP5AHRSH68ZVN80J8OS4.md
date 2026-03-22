## FlowContext\.PublishAsync\<TEvent\>\(TEvent, CancellationToken\) Method

Publishes a domain event asynchronously to all registered handlers\.
Handlers are resolved from the service provider and executed sequentially\.

```csharp
public System.Threading.Tasks.Task PublishAsync<TEvent>(TEvent eventData, System.Threading.CancellationToken cancellationToken);
```
#### Type parameters

<a name='FlowT.FlowContext.PublishAsync_TEvent_(TEvent,System.Threading.CancellationToken).TEvent'></a>

`TEvent`

The type of event to publish\.
#### Parameters

<a name='FlowT.FlowContext.PublishAsync_TEvent_(TEvent,System.Threading.CancellationToken).eventData'></a>

`eventData` [TEvent](FlowContext.PublishAsync.58535BP5AHRSH68ZVN80J8OS4.md#FlowT.FlowContext.PublishAsync_TEvent_(TEvent,System.Threading.CancellationToken).TEvent 'FlowT\.FlowContext\.PublishAsync\<TEvent\>\(TEvent, System\.Threading\.CancellationToken\)\.TEvent')

The event data to pass to handlers\.

<a name='FlowT.FlowContext.PublishAsync_TEvent_(TEvent,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous operation\.