## TransactionPlugin\.IsActive Property

Gets a value indicating whether a transaction is currently active\.
Set to `true` after [BeginAsync\(CancellationToken\)](ITransactionPlugin.BeginAsync.68EP3056CKK4WKJMTJMX9YH56.md 'FlowT\.Plugins\.ITransactionPlugin\.BeginAsync\(System\.Threading\.CancellationToken\)') and `false` after
[CommitAsync\(CancellationToken\)](ITransactionPlugin.CommitAsync.POHH5V5FS278DV41Z3OFWY659.md 'FlowT\.Plugins\.ITransactionPlugin\.CommitAsync\(System\.Threading\.CancellationToken\)') or [RollbackAsync\(CancellationToken\)](ITransactionPlugin.RollbackAsync.BWVSHBYF5S0FPTV4HXEEPD8E9.md 'FlowT\.Plugins\.ITransactionPlugin\.RollbackAsync\(System\.Threading\.CancellationToken\)')\.

```csharp
public bool IsActive { get; protected set; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')