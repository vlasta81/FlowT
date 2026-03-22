## StreamableResponse\.WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\) Method

Writes metadata properties \(non\-streaming data\) to the JSON writer\.
Override this method to write buffered properties like counts, timestamps, page info, etc\.

```csharp
protected abstract System.Threading.Tasks.Task WriteMetadataAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='FlowT.Abstractions.StreamableResponse.WriteMetadataAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer\.

<a name='FlowT.Abstractions.StreamableResponse.WriteMetadataAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous operation\.

### Remarks
Use [WriteProperty\(Utf8JsonWriter, string, object\)](StreamableResponse.WriteProperty.H7HOIXU0WE1N5ZKQQVG550HJC.md 'FlowT\.Abstractions\.StreamableResponse\.WriteProperty\(System\.Text\.Json\.Utf8JsonWriter, string, object\)') helper method for simple properties\.
This method is called BEFORE [WriteItemsAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteItemsAsync.CVS78QU0H752PP2KHCH7W4ZTE.md 'FlowT\.Abstractions\.StreamableResponse\.WriteItemsAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)'), so metadata is sent to client first\.