## ITenantPlugin\.TenantId Property

Gets the resolved tenant identifier for the current flow execution\.
Never `null` — falls back to `"default"` when the tenant cannot be determined\.

```csharp
string TenantId { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')