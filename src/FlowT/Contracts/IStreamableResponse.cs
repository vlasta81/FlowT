using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowT.Abstractions;

namespace FlowT.Contracts
{
    /// <summary>
    /// Marker interface for flow responses that support streaming serialization.
    /// Implement this interface to enable automatic chunked transfer encoding with custom JSON structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a flow's <c>TResponse</c> implements this interface, <c>MapFlow</c> automatically selects
    /// <c>Results.Stream()</c> for the endpoint — no manual streaming boilerplate required.
    /// For a ready-made implementation, use <see cref="PagedStreamResponse{T}"/> or subclass <c>StreamableResponse</c>.
    /// Implement this interface directly only when you need a fully custom JSON structure.
    /// </para>
    /// <para><strong>Use cases:</strong></para>
    /// <list type="bullet">
    /// <item>Paginated results with metadata (total count, page info) + streaming items</item>
    /// <item>Large dataset exports (CSV, JSON arrays with thousands of records)</item>
    /// <item>Real-time data feeds with metadata headers</item>
    /// <item>Custom response structures combining buffered metadata with streamed collections</item>
    /// </list>
    /// <para><strong>Alternatives:</strong></para>
    /// <list type="bullet">
    /// <item>For pure streaming without metadata, use <see cref="IAsyncEnumerable{T}"/> as TResponse directly</item>
    /// <item>For small datasets (less than 1000 items), use standard buffered responses (List, Array)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Custom streaming response
    /// public class CustomStreamResponse : IStreamableResponse
    /// {
    ///     public int TotalCount { get; init; }
    ///     public IAsyncEnumerable&lt;Item&gt; Items { get; init; }
    ///     
    ///     public async Task WriteToStreamAsync(Utf8JsonWriter writer, CancellationToken ct)
    ///     {
    ///         writer.WriteStartObject();
    ///         writer.WriteNumber("totalCount", TotalCount);
    ///         writer.WritePropertyName("items");
    ///         writer.WriteStartArray();
    ///         
    ///         await foreach (var item in Items.WithCancellation(ct))
    ///         {
    ///             JsonSerializer.Serialize(writer, item);
    ///             await writer.FlushAsync(ct);
    ///         }
    ///         
    ///         writer.WriteEndArray();
    ///         writer.WriteEndObject();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IStreamableResponse
    {
        /// <summary>
        /// Writes the response to the HTTP output stream using progressive serialization.
        /// This method is called by FlowT's <c>MapFlow</c> extension when streaming is detected.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer connected to the HTTP response stream.</param>
        /// <param name="cancellationToken">Cancellation token to observe for client disconnection.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        /// <remarks>
        /// <para>
        /// Implementations should:
        /// <list type="number">
        /// <item>Write buffered metadata first (properties like counts, page info, timestamps)</item>
        /// <item>Stream collection properties using <c>IAsyncEnumerable&lt;T&gt;</c></item>
        /// <item>Call <c>await writer.FlushAsync(cancellationToken)</c> after each item to send chunks progressively</item>
        /// <item>Respect <paramref name="cancellationToken"/> for early termination on client disconnect</item>
        /// </list>
        /// </para>
        /// <para><strong>Performance tips:</strong></para>
        /// <list type="bullet">
        /// <item>Flush every 10-100 items for optimal chunk size (balance latency vs overhead)</item>
        /// <item>Use <c>JsonSerializer.Serialize(writer, item)</c> for items (not string concatenation)</item>
        /// <item>Avoid buffering entire collections in memory before streaming</item>
        /// </list>
        /// </remarks>
        Task WriteToStreamAsync(Utf8JsonWriter writer, CancellationToken cancellationToken);
    }
}
