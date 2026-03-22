## FlowTransactionPlugin\.BeginAsync\(CancellationToken\) Method

Begins a new transaction asynchronously\.

```csharp
public abstract System.Threading.Tasks.ValueTask BeginAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='FlowT.Plugins.FlowTransactionPlugin.BeginAsync(System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Token to observe for cancellation\.

#### Returns
[System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask')