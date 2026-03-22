## ITransactionPlugin Interface

Built\-in plugin interface that coordinates a database transaction across all pipeline stages
within a single flow execution \(specifications → policies → handler\)\.

```csharp
public interface ITransactionPlugin
```

Derived  
&#8627; [FlowTransactionPlugin](FlowTransactionPlugin.md 'FlowT\.Plugins\.FlowTransactionPlugin')

### Remarks
Implement this interface via [FlowTransactionPlugin](FlowTransactionPlugin.md 'FlowT\.Plugins\.FlowTransactionPlugin') for a specific database provider\.
Because the plugin is PerFlow, a policy can begin a transaction that the handler participates in
without any direct coupling between the two\.

Register the concrete implementation via:
`services.AddFlowPlugin<ITransactionPlugin, MyTransactionPlugin>()`

Usage in a transaction policy:

```csharp
public class TransactionPolicy : FlowPolicy<TRequest, TResponse>
{
    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        var tx = context.Plugin<ITransactionPlugin>();
        await tx.BeginAsync(context.CancellationToken);
        try
        {
            var response = await Next!.HandleAsync(request, context);
            await tx.CommitAsync(context.CancellationToken);
            return response;
        }
        catch
        {
            await tx.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
```

| Properties | |
| :--- | :--- |
| [IsActive](ITransactionPlugin.IsActive.md 'FlowT\.Plugins\.ITransactionPlugin\.IsActive') | Gets a value indicating whether a transaction is currently active\. Set to `true` after [BeginAsync\(CancellationToken\)](ITransactionPlugin.BeginAsync.68EP3056CKK4WKJMTJMX9YH56.md 'FlowT\.Plugins\.ITransactionPlugin\.BeginAsync\(System\.Threading\.CancellationToken\)') and `false` after [CommitAsync\(CancellationToken\)](ITransactionPlugin.CommitAsync.POHH5V5FS278DV41Z3OFWY659.md 'FlowT\.Plugins\.ITransactionPlugin\.CommitAsync\(System\.Threading\.CancellationToken\)') or [RollbackAsync\(CancellationToken\)](ITransactionPlugin.RollbackAsync.BWVSHBYF5S0FPTV4HXEEPD8E9.md 'FlowT\.Plugins\.ITransactionPlugin\.RollbackAsync\(System\.Threading\.CancellationToken\)')\. |

| Methods | |
| :--- | :--- |
| [BeginAsync\(CancellationToken\)](ITransactionPlugin.BeginAsync.68EP3056CKK4WKJMTJMX9YH56.md 'FlowT\.Plugins\.ITransactionPlugin\.BeginAsync\(System\.Threading\.CancellationToken\)') | Begins a new transaction asynchronously\. |
| [CommitAsync\(CancellationToken\)](ITransactionPlugin.CommitAsync.POHH5V5FS278DV41Z3OFWY659.md 'FlowT\.Plugins\.ITransactionPlugin\.CommitAsync\(System\.Threading\.CancellationToken\)') | Commits the active transaction asynchronously\. |
| [RollbackAsync\(CancellationToken\)](ITransactionPlugin.RollbackAsync.BWVSHBYF5S0FPTV4HXEEPD8E9.md 'FlowT\.Plugins\.ITransactionPlugin\.RollbackAsync\(System\.Threading\.CancellationToken\)') | Rolls back the active transaction asynchronously\. |
