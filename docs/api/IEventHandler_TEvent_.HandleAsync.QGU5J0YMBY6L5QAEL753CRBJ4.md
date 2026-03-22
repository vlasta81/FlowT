## IEventHandler\<TEvent\>\.HandleAsync\(TEvent, CancellationToken\) Method

Handles the event asynchronously\.

```csharp
System.Threading.Tasks.Task HandleAsync(TEvent eventData, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='FlowT.Contracts.IEventHandler_TEvent_.HandleAsync(TEvent,System.Threading.CancellationToken).eventData'></a>

`eventData` [TEvent](IEventHandler_TEvent_.md#FlowT.Contracts.IEventHandler_TEvent_.TEvent 'FlowT\.Contracts\.IEventHandler\<TEvent\>\.TEvent')

The event data to process\.

<a name='FlowT.Contracts.IEventHandler_TEvent_.HandleAsync(TEvent,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous operation\.