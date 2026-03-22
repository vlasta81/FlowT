## FileStreamResponse\.Dispose\(\) Method

Disposes the underlying stream\.

```csharp
public void Dispose();
```

### Remarks
When using `MapFlow`, the stream is disposed by ASP\.NET Core after the response is sent
\(via `Results.File`\)\. Call `Dispose()` manually only when using [FileStreamResponse](FileStreamResponse.md 'FlowT\.Abstractions\.FileStreamResponse')
outside of `MapFlow` \(e\.g\., in tests or custom endpoint handlers\)\.