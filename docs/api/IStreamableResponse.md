## IStreamableResponse Interface

Marker interface for flow responses that support streaming serialization\.
Implement this interface to enable automatic chunked transfer encoding with custom JSON structure\.

```csharp
public interface IStreamableResponse
```

Derived  
&#8627; [StreamableResponse](StreamableResponse.md 'FlowT\.Abstractions\.StreamableResponse')

### Example

```csharp
// Custom streaming response
public class CustomStreamResponse : IStreamableResponse
{
    public int TotalCount { get; init; }
    public IAsyncEnumerable<Item> Items { get; init; }
    
    public async Task WriteToStreamAsync(Utf8JsonWriter writer, CancellationToken ct)
    {
        writer.WriteStartObject();
        writer.WriteNumber("totalCount", TotalCount);
        writer.WritePropertyName("items");
        writer.WriteStartArray();
        
        await foreach (var item in Items.WithCancellation(ct))
        {
            JsonSerializer.Serialize(writer, item);
            await writer.FlushAsync(ct);
        }
        
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
```

### Remarks

When a flow's `TResponse` implements this interface, `MapFlow` automatically selects
`Results.Stream()` for the endpoint — no manual streaming boilerplate required.
For a ready-made implementation, use [PagedStreamResponse&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/pagedstreamresponse<t> 'PagedStreamResponse\<T\>') or subclass `StreamableResponse`.
Implement this interface directly only when you need a fully custom JSON structure.

<strong>Use cases:</strong>
- Paginated results with metadata (total count, page info) + streaming items
- Large dataset exports (CSV, JSON arrays with thousands of records)
- Real-time data feeds with metadata headers
- Custom response structures combining buffered metadata with streamed collections

<strong>Alternatives:</strong>
- For pure streaming without metadata, use [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/iasyncenumerable<t> 'IAsyncEnumerable\<T\>') as TResponse directly
- For small datasets (less than 1000 items), use standard buffered responses (List, Array)

| Methods | |
| :--- | :--- |
| [WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\)](IStreamableResponse.WriteToStreamAsync.7USWWX1IJCBOGRK21IFII23O5.md 'FlowT\.Contracts\.IStreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Writes the response to the HTTP output stream using progressive serialization\. This method is called by FlowT's `MapFlow` extension when streaming is detected\. |
