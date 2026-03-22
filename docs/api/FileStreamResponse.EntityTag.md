## FileStreamResponse\.EntityTag Property

Gets or initializes the entity tag \(ETag\) for caching\.

```csharp
public string? EntityTag { get; init; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
ETags enable efficient caching via "If\-None\-Match" headers \(HTTP 304 responses\)\.
Example: File hash, last modified timestamp, or version identifier\.