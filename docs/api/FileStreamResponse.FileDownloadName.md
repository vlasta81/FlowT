## FileStreamResponse\.FileDownloadName Property

Gets or initializes the suggested filename for downloads\.

```csharp
public string? FileDownloadName { get; init; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

### Remarks

If specified, sets the Content-Disposition header to "attachment; filename=...",
prompting the browser to download rather than display inline.

If null, the file may be displayed inline if the browser supports the content type.