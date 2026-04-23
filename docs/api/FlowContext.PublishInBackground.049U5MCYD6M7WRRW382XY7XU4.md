## FlowContext\.PublishInBackground\<TEvent\>\(TEvent, CancellationToken\) Method

Publishes a domain event in the background \(fire\-and\-forget\) without blocking the current flow\.
Use this for non\-critical side effects that shouldn't delay the response\.

```csharp
public System.Threading.Tasks.Task PublishInBackground<TEvent>(TEvent eventData, System.Threading.CancellationToken cancellationToken);
```
#### Type parameters

<a name='FlowT.FlowContext.PublishInBackground_TEvent_(TEvent,System.Threading.CancellationToken).TEvent'></a>

`TEvent`

The type of event to publish\.
#### Parameters

<a name='FlowT.FlowContext.PublishInBackground_TEvent_(TEvent,System.Threading.CancellationToken).eventData'></a>

`eventData` [TEvent](FlowContext.PublishInBackground.049U5MCYD6M7WRRW382XY7XU4.md#FlowT.FlowContext.PublishInBackground_TEvent_(TEvent,System.Threading.CancellationToken).TEvent 'FlowT\.FlowContext\.PublishInBackground\<TEvent\>\(TEvent, System\.Threading\.CancellationToken\)\.TEvent')

The event data to pass to handlers\.

<a name='FlowT.FlowContext.PublishInBackground_TEvent_(TEvent,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the background operation\. The returned task is typically not awaited by callers\.

### Remarks
Any exception thrown by an event handler is caught and logged via [Microsoft\.Extensions\.Logging\.ILoggerFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory 'Microsoft\.Extensions\.Logging\.ILoggerFactory')
resolved from [Services](FlowContext.Services.md 'FlowT\.FlowContext\.Services')\. If no logger factory is registered the exception is silently discarded\.
The background work is dispatched via [System\.Threading\.Tasks\.Task\.Run\(System\.Func\{System\.Threading\.Tasks\.Task\},System\.Threading\.CancellationToken\)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run#system-threading-tasks-task-run(system-func{system-threading-tasks-task}-system-threading-cancellationtoken) 'System\.Threading\.Tasks\.Task\.Run\(System\.Func\{System\.Threading\.Tasks\.Task\},System\.Threading\.CancellationToken\)')
and respects the provided [cancellationToken](FlowContext.PublishInBackground.049U5MCYD6M7WRRW382XY7XU4.md#FlowT.FlowContext.PublishInBackground_TEvent_(TEvent,System.Threading.CancellationToken).cancellationToken 'FlowT\.FlowContext\.PublishInBackground\<TEvent\>\(TEvent, System\.Threading\.CancellationToken\)\.cancellationToken')\.