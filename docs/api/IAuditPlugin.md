## IAuditPlugin Interface

Built\-in plugin that accumulates structured audit entries for the current flow execution\.
Entries are stored in memory for the lifetime of the flow and can be flushed to a
persistent store \(database, log, audit service\) by the caller after the flow completes\.

```csharp
public interface IAuditPlugin
```

Derived  
&#8627; [AuditPlugin](AuditPlugin.md 'FlowT\.Plugins\.AuditPlugin')

### Remarks
Register via `services.AddFlowPlugin<IAuditPlugin, AuditPlugin>()`\.

Usage:

```csharp
var audit = context.Plugin<IAuditPlugin>();
audit.Record("OrderCreated", new { orderId, userId });

foreach (var entry in audit.Entries)
    logger.LogInformation("[{Timestamp}] {Action}", entry.Timestamp, entry.Action);
```

| Properties | |
| :--- | :--- |
| [Entries](IAuditPlugin.Entries.md 'FlowT\.Plugins\.IAuditPlugin\.Entries') | Gets all audit entries recorded during the current flow execution, in chronological order\. |

| Methods | |
| :--- | :--- |
| [Record\(string, object\)](IAuditPlugin.Record.CPBI7WB2DHPIVIB2PA59KA7M4.md 'FlowT\.Plugins\.IAuditPlugin\.Record\(string, object\)') | Records an audit action with an optional data payload\. The entry timestamp is set to [System\.DateTimeOffset\.UtcNow](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.utcnow 'System\.DateTimeOffset\.UtcNow') at the time of the call\. |
