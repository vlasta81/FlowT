## StreamableResponse\.WriteItemsAsync\(Utf8JsonWriter, CancellationToken\) Method

Writes streaming items to the JSON writer as a JSON array\.
Override this method to enumerate and serialize collection items progressively\.

```csharp
protected abstract System.Threading.Tasks.Task WriteItemsAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='FlowT.Abstractions.StreamableResponse.WriteItemsAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer\.

<a name='FlowT.Abstractions.StreamableResponse.WriteItemsAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token to observe for client disconnection\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous operation\.

### Remarks

This method should:
- Enumerate [IAsyncEnumerable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/iasyncenumerable<t> 'IAsyncEnumerable\<T\>') collections
- Serialize each item using `JsonSerializer.Serialize(writer, item)`
- Call `await writer.FlushAsync(cancellationToken)` periodically (e.g., every 10-100 items)

<strong>Example:</strong>

```csharp
await foreach (var item in Items.WithCancellation(cancellationToken))
{
    JsonSerializer.Serialize(writer, item);
    await writer.FlushAsync(cancellationToken); // Send chunk to client
}
```