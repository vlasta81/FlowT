## FileStreamResponse\.ContentType Property

Gets or initializes the MIME content type \(e\.g\., "application/pdf", "video/mp4"\)\.

```csharp
public string ContentType { get; init; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks
If not specified, defaults to "application/octet\-stream" \(generic binary\)\.
Set this correctly for proper browser handling \(inline display vs download\)\.