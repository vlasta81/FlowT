## FlowTransactionPlugin Class

Abstract base for database transaction plugins\. Inherit from this class to provide a concrete
implementation for a specific database provider \(e\.g\., EF Core, Dapper, ADO\.NET\)\.
Automatically receives the current [FlowContext](FlowContext.md 'FlowT\.FlowContext') via the `protected Context` property\.

```csharp
public abstract class FlowTransactionPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.ITransactionPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; FlowTransactionPlugin

Implements [ITransactionPlugin](ITransactionPlugin.md 'FlowT\.Plugins\.ITransactionPlugin')

### Example
EF Core example:

```csharp
public class EfCoreTransactionPlugin : FlowTransactionPlugin
{
    private IDbContextTransaction? _tx;

    public override async ValueTask BeginAsync(CancellationToken cancellationToken = default)
    {
        _tx = await Context.Service<AppDbContext>().Database.BeginTransactionAsync(cancellationToken);
        IsActive = true;
    }

    public override async ValueTask CommitAsync(CancellationToken cancellationToken = default)
    {
        await _tx!.CommitAsync(cancellationToken);
        IsActive = false;
    }

    public override async ValueTask RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _tx!.RollbackAsync(cancellationToken);
        IsActive = false;
    }
}

// Registration
services.AddFlowPlugin<ITransactionPlugin, EfCoreTransactionPlugin>();
```

| Properties | |
| :--- | :--- |
| [IsActive](FlowTransactionPlugin.IsActive.md 'FlowT\.Plugins\.FlowTransactionPlugin\.IsActive') | Gets a value indicating whether a transaction is currently active\. Set to `true` after [BeginAsync\(CancellationToken\)](ITransactionPlugin.BeginAsync.68EP3056CKK4WKJMTJMX9YH56.md 'FlowT\.Plugins\.ITransactionPlugin\.BeginAsync\(System\.Threading\.CancellationToken\)') and `false` after [CommitAsync\(CancellationToken\)](ITransactionPlugin.CommitAsync.POHH5V5FS278DV41Z3OFWY659.md 'FlowT\.Plugins\.ITransactionPlugin\.CommitAsync\(System\.Threading\.CancellationToken\)') or [RollbackAsync\(CancellationToken\)](ITransactionPlugin.RollbackAsync.BWVSHBYF5S0FPTV4HXEEPD8E9.md 'FlowT\.Plugins\.ITransactionPlugin\.RollbackAsync\(System\.Threading\.CancellationToken\)')\. |

| Methods | |
| :--- | :--- |
| [BeginAsync\(CancellationToken\)](FlowTransactionPlugin.BeginAsync.H96ZSN403FF8O1OGHHHKIMR8D.md 'FlowT\.Plugins\.FlowTransactionPlugin\.BeginAsync\(System\.Threading\.CancellationToken\)') | Begins a new transaction asynchronously\. |
| [CommitAsync\(CancellationToken\)](FlowTransactionPlugin.CommitAsync.JM16GUNNG830XWX4FCKPUUTO4.md 'FlowT\.Plugins\.FlowTransactionPlugin\.CommitAsync\(System\.Threading\.CancellationToken\)') | Commits the active transaction asynchronously\. |
| [RollbackAsync\(CancellationToken\)](FlowTransactionPlugin.RollbackAsync.H9406Y6BANJM4EFTFP610UME6.md 'FlowT\.Plugins\.FlowTransactionPlugin\.RollbackAsync\(System\.Threading\.CancellationToken\)') | Rolls back the active transaction asynchronously\. |
