using System;
using System.IO;

namespace FlowT.Abstractions
{
    /// <summary>
    /// Represents a file streaming response for large file downloads.
    /// Use this when you need to stream binary files efficiently without loading them into memory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a flow's <c>TResponse</c> is <see cref="FileStreamResponse"/>, <c>MapFlow</c> automatically
    /// calls <c>Results.File()</c> with all properties (Content-Type, Content-Disposition, ETag,
    /// Last-Modified, range support) — no manual stream wiring required in the endpoint.
    /// </para>
    /// <para>
    /// This response type enables efficient file downloads by streaming content directly from disk (or other source)
    /// to the HTTP response without buffering the entire file in memory.
    /// </para>
    /// 
    /// <para><strong>Key Benefits:</strong></para>
    /// <list type="bullet">
    /// <item><description>O(1) memory usage regardless of file size</description></item>
    /// <item><description>Supports range requests (HTTP 206 Partial Content)</description></item>
    /// <item><description>Automatic content-type detection</description></item>
    /// <item><description>Download progress tracking on client side</description></item>
    /// </list>
    /// 
    /// <para><strong>Example Usage:</strong></para>
    /// <code>
    /// public class DownloadReportHandler : IFlowHandler&lt;DownloadReportRequest, FileStreamResponse&gt;
    /// {
    ///     public async ValueTask&lt;FileStreamResponse&gt; HandleAsync(
    ///         DownloadReportRequest request,
    ///         FlowContext context)
    ///     {
    ///         // ✅ Resolve per-request (safe in singleton handlers)
    ///         var storage = context.Service&lt;IFileStorage&gt;();
    ///         var stream = await storage.OpenReadAsync($"reports/{request.ReportId}.pdf");
    ///         
    ///         return new FileStreamResponse
    ///         {
    ///             Stream = stream,
    ///             ContentType = "application/pdf",
    ///             FileDownloadName = $"Report-{request.ReportId}.pdf",
    ///             EnableRangeProcessing = true
    ///         };
    ///     }
    /// }
    /// </code>
    /// 
    /// <para><strong>When to Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Downloading large files (> 1 MB)</description></item>
    /// <item><description>Media streaming (video, audio)</description></item>
    /// <item><description>Report exports (PDF, Excel)</description></item>
    /// <item><description>Backups and archive downloads</description></item>
    /// </list>
    /// 
    /// <para><strong>When NOT to Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Small files (&lt; 1 MB) - use byte[] for simplicity</description></item>
    /// <item><description>JSON/XML data - use regular responses</description></item>
    /// <item><description>In-memory buffers - defeats streaming purpose</description></item>
    /// </list>
    /// </remarks>
    public class FileStreamResponse : IDisposable
    {
        /// <summary>
        /// Gets or initializes the stream containing the file content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This stream will be automatically disposed after the response is sent.
        /// Ensure the stream supports reading and seeking (for range requests).
        /// </para>
        /// <para>
        /// <strong>Important:</strong> The stream is consumed asynchronously by ASP.NET Core.
        /// Do not dispose it manually in your handler.
        /// </para>
        /// </remarks>
        public Stream Stream { get; init; } = Stream.Null;

        /// <summary>
        /// Gets or initializes the MIME content type (e.g., "application/pdf", "video/mp4").
        /// </summary>
        /// <remarks>
        /// If not specified, defaults to "application/octet-stream" (generic binary).
        /// Set this correctly for proper browser handling (inline display vs download).
        /// </remarks>
        public string ContentType { get; init; } = "application/octet-stream";

        /// <summary>
        /// Gets or initializes the suggested filename for downloads.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If specified, sets the Content-Disposition header to "attachment; filename=...",
        /// prompting the browser to download rather than display inline.
        /// </para>
        /// <para>
        /// If null, the file may be displayed inline if the browser supports the content type.
        /// </para>
        /// </remarks>
        public string? FileDownloadName { get; init; }

        /// <summary>
        /// Gets or initializes whether range requests (HTTP 206) are supported.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When true, enables:
        /// <list type="bullet">
        /// <item><description>Resume interrupted downloads</description></item>
        /// <item><description>Video/audio seeking</description></item>
        /// <item><description>Partial content delivery</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Requires the stream to support seeking (<see cref="Stream.CanSeek"/> = true).
        /// </para>
        /// <para>
        /// Default: false (simpler, but no resume support).
        /// </para>
        /// </remarks>
        public bool EnableRangeProcessing { get; init; }

        /// <summary>
        /// Gets or initializes the entity tag (ETag) for caching.
        /// </summary>
        /// <remarks>
        /// ETags enable efficient caching via "If-None-Match" headers (HTTP 304 responses).
        /// Example: File hash, last modified timestamp, or version identifier.
        /// </remarks>
        public string? EntityTag { get; init; }

        /// <summary>
        /// Gets or initializes the last modified date for caching.
        /// </summary>
        /// <remarks>
        /// Used with "If-Modified-Since" headers to avoid re-downloading unchanged files.
        /// </remarks>
        public DateTimeOffset? LastModified { get; init; }

        /// <summary>
        /// Disposes the underlying stream.
        /// </summary>
        /// <remarks>
        /// When using <c>MapFlow</c>, the stream is disposed by ASP.NET Core after the response is sent
        /// (via <c>Results.File</c>). Call <c>Dispose()</c> manually only when using <see cref="FileStreamResponse"/>
        /// outside of <c>MapFlow</c> (e.g., in tests or custom endpoint handlers).
        /// </remarks>
        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
