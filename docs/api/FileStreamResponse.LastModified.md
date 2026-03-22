## FileStreamResponse\.LastModified Property

Gets or initializes the last modified date for caching\.

```csharp
public System.Nullable<System.DateTimeOffset> LastModified { get; init; }
```

#### Property Value
[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[System\.DateTimeOffset](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset 'System\.DateTimeOffset')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')

### Remarks
Used with "If\-Modified\-Since" headers to avoid re\-downloading unchanged files\.