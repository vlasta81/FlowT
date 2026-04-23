## AuditEntry Class

Represents a single audit event recorded during a flow execution\.

```csharp
public sealed class AuditEntry
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; AuditEntry

| Constructors | |
| :--- | :--- |
| [AuditEntry\(string, DateTimeOffset, object\)](AuditEntry..ctor.6XM5CYBAL37CF0QF6W0EGPY71.md 'FlowT\.Plugins\.AuditEntry\.AuditEntry\(string, System\.DateTimeOffset, object\)') | Initializes a new [AuditEntry](AuditEntry.md 'FlowT\.Plugins\.AuditEntry')\. |

| Properties | |
| :--- | :--- |
| [Action](AuditEntry.Action.md 'FlowT\.Plugins\.AuditEntry\.Action') | Gets the short description of the audited action\. |
| [Data](AuditEntry.Data.md 'FlowT\.Plugins\.AuditEntry\.Data') | Gets optional contextual data associated with the action\. |
| [Timestamp](AuditEntry.Timestamp.md 'FlowT\.Plugins\.AuditEntry\.Timestamp') | Gets the UTC timestamp when the action was recorded\. |
