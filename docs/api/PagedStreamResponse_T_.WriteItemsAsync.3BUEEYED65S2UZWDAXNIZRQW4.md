## PagedStreamResponse\<T\>\.WriteItemsAsync\(Utf8JsonWriter, CancellationToken\) Method

Progressively writes items from [Items](PagedStreamResponse_T_.Items.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.Items') to the JSON array\.

```csharp
protected override System.Threading.Tasks.Task WriteItemsAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken ct);
```
#### Parameters

<a name='FlowT.Abstractions.PagedStreamResponse_T_.WriteItemsAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer\.

<a name='FlowT.Abstractions.PagedStreamResponse_T_.WriteItemsAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).ct'></a>

`ct` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')

### Remarks

Items are serialized one-by-one using [System\.Text\.Json\.JsonSerializer](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializer 'System\.Text\.Json\.JsonSerializer') and flushed
immediately to enable progressive response delivery. This approach:
- Minimizes memory usage (only current item buffered)
- Reduces time-to-first-byte (TTFB) for clients
- Enables HTTP chunked transfer encoding
- Respects cancellation tokens for early client disconnection

The method uses [IAsyncEnumerable&lt;T&gt;\.WithCancellation](https://learn.microsoft.com/en-us/dotnet/api/iasyncenumerable<t>.withcancellation 'IAsyncEnumerable\<T\>\.WithCancellation') to ensure
the data source respects the cancellation token, preventing wasted database queries
or I/O operations if the client disconnects.