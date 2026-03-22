## FlowTransactionPlugin\.RollbackAsync\(CancellationToken\) Method

Rolls back the active transaction asynchronously\.

```csharp
public abstract System.Threading.Tasks.ValueTask RollbackAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='FlowT.Plugins.FlowTransactionPlugin.RollbackAsync(System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Token to observe for cancellation\.

#### Returns
[System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask')