using FlowT.Contracts;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Abstractions
{
    /// <summary>
    /// Abstract base class for streaming responses with separated metadata and items serialization.
    /// Provides a structured approach to building responses that combine buffered metadata with streamed collections.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class orchestrates the serialization process:
    /// <list type="number">
    /// <item>Writes JSON object start</item>
    /// <item>Calls <see cref="WriteMetadataAsync"/> for buffered properties (counts, page info, etc.)</item>
    /// <item>Calls <see cref="WriteItemsAsync"/> for streaming collection items</item>
    /// <item>Writes JSON object end</item>
    /// </list>
    /// </para>
    /// <para><strong>JSON structure produced:</strong></para>
    /// <code>
    /// {
    ///   "property1": value1,      // ← WriteMetadataAsync
    ///   "property2": value2,      // ← WriteMetadataAsync
    ///   "items": [                // ← WriteItemsAsync
    ///     {...},
    ///     {...}
    ///   ]
    /// }
    /// </code>
    /// <para><strong>When to use:</strong></para>
    /// <list type="bullet">
    /// <item>Standard pagination responses (total count + page + items)</item>
    /// <item>Responses with mixed metadata and large collections</item>
    /// <item>Custom response structures with consistent format</item>
    /// </list>
    /// <para><strong>When NOT to use:</strong></para>
    /// <list type="bullet">
    /// <item>Pure streaming without metadata → use <see cref="IAsyncEnumerable{T}"/> directly</item>
    /// <item>Fully custom JSON structure → implement <see cref="IStreamableResponse"/> directly</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class UserStreamResponse : StreamableResponse
    /// {
    ///     public int TotalUsers { get; init; }
    ///     public DateTime GeneratedAt { get; init; }
    ///     public IAsyncEnumerable&lt;UserDto&gt; Users { get; init; }
    ///     
    ///     protected override Task WriteMetadataAsync(Utf8JsonWriter writer, CancellationToken ct)
    ///     {
    ///         WriteProperty(writer, "totalUsers", TotalUsers);
    ///         WriteProperty(writer, "generatedAt", GeneratedAt);
    ///         return Task.CompletedTask;
    ///     }
    ///     
    ///     protected override async Task WriteItemsAsync(Utf8JsonWriter writer, CancellationToken ct)
    ///     {
    ///         await foreach (var user in Users.WithCancellation(ct))
    ///         {
    ///             JsonSerializer.Serialize(writer, user);
    ///             await writer.FlushAsync(ct);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class StreamableResponse : IStreamableResponse
    {
        /// <summary>
        /// Writes metadata properties (non-streaming data) to the JSON writer.
        /// Override this method to write buffered properties like counts, timestamps, page info, etc.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Use <see cref="WriteProperty"/> helper method for simple properties.
        /// This method is called BEFORE <see cref="WriteItemsAsync"/>, so metadata is sent to client first.
        /// </remarks>
        protected abstract Task WriteMetadataAsync(Utf8JsonWriter writer, CancellationToken cancellationToken);

        /// <summary>
        /// Writes streaming items to the JSON writer as a JSON array.
        /// Override this method to enumerate and serialize collection items progressively.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="cancellationToken">Cancellation token to observe for client disconnection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// This method should:
        /// <list type="bullet">
        /// <item>Enumerate <see cref="IAsyncEnumerable{T}"/> collections</item>
        /// <item>Serialize each item using <c>JsonSerializer.Serialize(writer, item)</c></item>
        /// <item>Call <c>await writer.FlushAsync(cancellationToken)</c> periodically (e.g., every 10-100 items)</item>
        /// </list>
        /// </para>
        /// <para><strong>Example:</strong></para>
        /// <code>
        /// await foreach (var item in Items.WithCancellation(cancellationToken))
        /// {
        ///     JsonSerializer.Serialize(writer, item);
        ///     await writer.FlushAsync(cancellationToken); // Send chunk to client
        /// }
        /// </code>
        /// </remarks>
        protected abstract Task WriteItemsAsync(Utf8JsonWriter writer, CancellationToken cancellationToken);

        /// <summary>
        /// Implements <see cref="IStreamableResponse.WriteToStreamAsync"/> by orchestrating metadata and items serialization.
        /// This is the main entry point called by FlowT's MapFlow extension.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer connected to HTTP response stream.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task WriteToStreamAsync(Utf8JsonWriter writer, CancellationToken cancellationToken)
        {
            writer.WriteStartObject();

            // Write metadata first (buffered, sent immediately)
            await WriteMetadataAsync(writer, cancellationToken).ConfigureAwait(false);

            // Write items array (streamed, sent progressively)
            writer.WritePropertyName("items");
            writer.WriteStartArray();
            await WriteItemsAsync(writer, cancellationToken).ConfigureAwait(false);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        /// <summary>
        /// Helper method to write a simple property to the JSON writer.
        /// Handles serialization of common types (numbers, strings, booleans, dates, etc.).
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="name">The property name (camelCase recommended).</param>
        /// <param name="value">The property value to serialize.</param>
        /// <remarks>
        /// Use this in <see cref="WriteMetadataAsync"/> for brevity:
        /// <code>
        /// WriteProperty(writer, "totalCount", 12345);
        /// WriteProperty(writer, "page", 0);
        /// </code>
        /// </remarks>
        protected void WriteProperty(Utf8JsonWriter writer, string name, object? value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value);
        }
    }
}
