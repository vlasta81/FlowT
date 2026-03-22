## PagedStreamResponse\<T\>\.WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\) Method

Writes pagination metadata \(totalCount, page, pageSize, hasMore\) to the JSON stream\.

```csharp
protected override System.Threading.Tasks.Task WriteMetadataAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken ct);
```
#### Parameters

<a name='FlowT.Abstractions.PagedStreamResponse_T_.WriteMetadataAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer\.

<a name='FlowT.Abstractions.PagedStreamResponse_T_.WriteMetadataAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).ct'></a>

`ct` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')

### Remarks
This method is called first by [WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteToStreamAsync.CML4D3PJUL4719IEVFT8RLE2A.md 'FlowT\.Abstractions\.StreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)')\.
The metadata is buffered before streaming begins, ensuring clients receive
pagination information immediately\.