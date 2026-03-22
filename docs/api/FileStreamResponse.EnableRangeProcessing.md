## FileStreamResponse\.EnableRangeProcessing Property

Gets or initializes whether range requests \(HTTP 206\) are supported\.

```csharp
public bool EnableRangeProcessing { get; init; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

### Remarks

When true, enables:
- Resume interrupted downloads
- Video/audio seeking
- Partial content delivery

Requires the stream to support seeking ([System\.IO\.Stream\.CanSeek](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.canseek 'System\.IO\.Stream\.CanSeek') = true).

Default: false (simpler, but no resume support).