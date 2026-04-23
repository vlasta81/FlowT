## AuditPlugin Class

Default implementation of [IAuditPlugin](IAuditPlugin.md 'FlowT\.Plugins\.IAuditPlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class AuditPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IAuditPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; AuditPlugin

Implements [IAuditPlugin](IAuditPlugin.md 'FlowT\.Plugins\.IAuditPlugin')

| Properties | |
| :--- | :--- |
| [Entries](AuditPlugin.Entries.md 'FlowT\.Plugins\.AuditPlugin\.Entries') | Gets all audit entries recorded during the current flow execution, in chronological order\. |

| Methods | |
| :--- | :--- |
| [Record\(string, object\)](AuditPlugin.Record.B5PAARKFM8KLYUVGYWBQE9MZB.md 'FlowT\.Plugins\.AuditPlugin\.Record\(string, object\)') | Records an audit action with an optional data payload\. The entry timestamp is set to [System\.DateTimeOffset\.UtcNow](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.utcnow 'System\.DateTimeOffset\.UtcNow') at the time of the call\. |
