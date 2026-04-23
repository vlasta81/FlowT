## AuditPlugin\.Record\(string, object\) Method

Records an audit action with an optional data payload\.
The entry timestamp is set to [System\.DateTimeOffset\.UtcNow](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.utcnow 'System\.DateTimeOffset\.UtcNow') at the time of the call\.

```csharp
public void Record(string action, object? data=null);
```
#### Parameters

<a name='FlowT.Plugins.AuditPlugin.Record(string,object).action'></a>

`action` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

A short description of the action being audited \(e\.g\., "OrderCreated", "PaymentFailed"\)\.

<a name='FlowT.Plugins.AuditPlugin.Record(string,object).data'></a>

`data` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

An optional object carrying contextual data for the entry\. Not serialized automatically\.