## StreamableResponse Class

Abstract base class for streaming responses with separated metadata and items serialization\.
Provides a structured approach to building responses that combine buffered metadata with streamed collections\.

```csharp
public abstract class StreamableResponse : FlowT.Contracts.IStreamableResponse
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; StreamableResponse

Derived  
&#8627; [PagedStreamResponse&lt;T&gt;](PagedStreamResponse_T_.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>')

Implements [IStreamableResponse](IStreamableResponse.md 'FlowT\.Contracts\.IStreamableResponse')

### Example

```csharp
public class UserStreamResponse : StreamableResponse
{
    public int TotalUsers { get; init; }
    public DateTime GeneratedAt { get; init; }
    public IAsyncEnumerable<UserDto> Users { get; init; }
    
    protected override Task WriteMetadataAsync(Utf8JsonWriter writer, CancellationToken ct)
    {
        WriteProperty(writer, "totalUsers", TotalUsers);
        WriteProperty(writer, "generatedAt", GeneratedAt);
        return Task.CompletedTask;
    }
    
    protected override async Task WriteItemsAsync(Utf8JsonWriter writer, CancellationToken ct)
    {
        await foreach (var user in Users.WithCancellation(ct))
        {
            JsonSerializer.Serialize(writer, user);
            await writer.FlushAsync(ct);
        }
    }
}
```

### Remarks

This class orchestrates the serialization process:
1. Writes JSON object start
2. Calls [WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteMetadataAsync.IAO5PI95GRUPJ5TVUOYKECXT6.md 'FlowT\.Abstractions\.StreamableResponse\.WriteMetadataAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') for buffered properties (counts, page info, etc.)
3. Calls [WriteItemsAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteItemsAsync.CVS78QU0H752PP2KHCH7W4ZTE.md 'FlowT\.Abstractions\.StreamableResponse\.WriteItemsAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') for streaming collection items
4. Writes JSON object end

<strong>JSON structure produced:</strong>

```csharp
{
  "property1": value1,      // ← WriteMetadataAsync
  "property2": value2,      // ← WriteMetadataAsync
  "items": [                // ← WriteItemsAsync
    {...},
    {...}
  ]
}
```

<strong>When to use:</strong>
- Standard pagination responses (total count + page + items)
- Responses with mixed metadata and large collections
- Custom response structures with consistent format

<strong>When NOT to use:</strong>
- Pure streaming without metadata → use [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/iasyncenumerable<t> 'IAsyncEnumerable\<T\>') directly
- Fully custom JSON structure → implement [IStreamableResponse](IStreamableResponse.md 'FlowT\.Contracts\.IStreamableResponse') directly

| Methods | |
| :--- | :--- |
| [WriteItemsAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteItemsAsync.CVS78QU0H752PP2KHCH7W4ZTE.md 'FlowT\.Abstractions\.StreamableResponse\.WriteItemsAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Writes streaming items to the JSON writer as a JSON array\. Override this method to enumerate and serialize collection items progressively\. |
| [WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteMetadataAsync.IAO5PI95GRUPJ5TVUOYKECXT6.md 'FlowT\.Abstractions\.StreamableResponse\.WriteMetadataAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Writes metadata properties \(non\-streaming data\) to the JSON writer\. Override this method to write buffered properties like counts, timestamps, page info, etc\. |
| [WriteProperty\(Utf8JsonWriter, string, object\)](StreamableResponse.WriteProperty.H7HOIXU0WE1N5ZKQQVG550HJC.md 'FlowT\.Abstractions\.StreamableResponse\.WriteProperty\(System\.Text\.Json\.Utf8JsonWriter, string, object\)') | Helper method to write a simple property to the JSON writer\. Handles serialization of common types \(numbers, strings, booleans, dates, etc\.\)\. |
| [WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteToStreamAsync.CML4D3PJUL4719IEVFT8RLE2A.md 'FlowT\.Abstractions\.StreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Implements [WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\)](IStreamableResponse.WriteToStreamAsync.7USWWX1IJCBOGRK21IFII23O5.md 'FlowT\.Contracts\.IStreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') by orchestrating metadata and items serialization\. This is the main entry point called by FlowT's MapFlow extension\. |
