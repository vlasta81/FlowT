## FileStreamResponse Class

Represents a file streaming response for large file downloads\.
Use this when you need to stream binary files efficiently without loading them into memory\.

```csharp
public class FileStreamResponse : System.IDisposable
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FileStreamResponse

Implements [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable')

### Remarks

When a flow's `TResponse` is [FileStreamResponse](FileStreamResponse.md 'FlowT\.Abstractions\.FileStreamResponse'), `MapFlow` automatically
calls `Results.File()` with all properties (Content-Type, Content-Disposition, ETag,
Last-Modified, range support) — no manual stream wiring required in the endpoint.

This response type enables efficient file downloads by streaming content directly from disk (or other source)
to the HTTP response without buffering the entire file in memory.

<strong>Key Benefits:</strong>
- O(1) memory usage regardless of file size
- Supports range requests (HTTP 206 Partial Content)
- Automatic content-type detection
- Download progress tracking on client side

<strong>Example Usage:</strong>

```csharp
public class DownloadReportHandler : IFlowHandler<DownloadReportRequest, FileStreamResponse>
{
    public async ValueTask<FileStreamResponse> HandleAsync(
        DownloadReportRequest request,
        FlowContext context)
    {
        // ✅ Resolve per-request (safe in singleton handlers)
        var storage = context.Service<IFileStorage>();
        var stream = await storage.OpenReadAsync($"reports/{request.ReportId}.pdf");
        
        return new FileStreamResponse
        {
            Stream = stream,
            ContentType = "application/pdf",
            FileDownloadName = $"Report-{request.ReportId}.pdf",
            EnableRangeProcessing = true
        };
    }
}
```

<strong>When to Use:</strong>
- Downloading large files (> 1 MB)
- Media streaming (video, audio)
- Report exports (PDF, Excel)
- Backups and archive downloads

<strong>When NOT to Use:</strong>
- Small files (< 1 MB) - use byte[] for simplicity
- JSON/XML data - use regular responses
- In-memory buffers - defeats streaming purpose

| Properties | |
| :--- | :--- |
| [ContentType](FileStreamResponse.ContentType.md 'FlowT\.Abstractions\.FileStreamResponse\.ContentType') | Gets or initializes the MIME content type \(e\.g\., "application/pdf", "video/mp4"\)\. |
| [EnableRangeProcessing](FileStreamResponse.EnableRangeProcessing.md 'FlowT\.Abstractions\.FileStreamResponse\.EnableRangeProcessing') | Gets or initializes whether range requests \(HTTP 206\) are supported\. |
| [EntityTag](FileStreamResponse.EntityTag.md 'FlowT\.Abstractions\.FileStreamResponse\.EntityTag') | Gets or initializes the entity tag \(ETag\) for caching\. |
| [FileDownloadName](FileStreamResponse.FileDownloadName.md 'FlowT\.Abstractions\.FileStreamResponse\.FileDownloadName') | Gets or initializes the suggested filename for downloads\. |
| [LastModified](FileStreamResponse.LastModified.md 'FlowT\.Abstractions\.FileStreamResponse\.LastModified') | Gets or initializes the last modified date for caching\. |
| [Stream](FileStreamResponse.Stream.md 'FlowT\.Abstractions\.FileStreamResponse\.Stream') | Gets or initializes the stream containing the file content\. |

| Methods | |
| :--- | :--- |
| [Dispose\(\)](FileStreamResponse.Dispose().md 'FlowT\.Abstractions\.FileStreamResponse\.Dispose\(\)') | Disposes the underlying stream\. |
