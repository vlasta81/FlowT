## IStreamableResponse\.WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\) Method

Writes the response to the HTTP output stream using progressive serialization\.
This method is called by FlowT's `MapFlow` extension when streaming is detected\.

```csharp
System.Threading.Tasks.Task WriteToStreamAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='FlowT.Contracts.IStreamableResponse.WriteToStreamAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer connected to the HTTP response stream\.

<a name='FlowT.Contracts.IStreamableResponse.WriteToStreamAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token to observe for client disconnection\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous write operation\.

### Remarks

Implementations should:
1. Write buffered metadata first (properties like counts, page info, timestamps)
2. Stream collection properties using `IAsyncEnumerable<T>`
3. Call `await writer.FlushAsync(cancellationToken)` after each item to send chunks progressively
4. Respect [cancellationToken](IStreamableResponse.WriteToStreamAsync.7USWWX1IJCBOGRK21IFII23O5.md#FlowT.Contracts.IStreamableResponse.WriteToStreamAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).cancellationToken 'FlowT\.Contracts\.IStreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)\.cancellationToken') for early termination on client disconnect

<strong>Performance tips:</strong>
- Flush every 10-100 items for optimal chunk size (balance latency vs overhead)
- Use `JsonSerializer.Serialize(writer, item)` for items (not string concatenation)
- Avoid buffering entire collections in memory before streaming