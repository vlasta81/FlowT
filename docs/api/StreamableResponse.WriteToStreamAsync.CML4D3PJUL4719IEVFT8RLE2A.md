## StreamableResponse\.WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\) Method

Implements [WriteToStreamAsync\(Utf8JsonWriter, CancellationToken\)](IStreamableResponse.WriteToStreamAsync.7USWWX1IJCBOGRK21IFII23O5.md 'FlowT\.Contracts\.IStreamableResponse\.WriteToStreamAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') by orchestrating metadata and items serialization\.
This is the main entry point called by FlowT's MapFlow extension\.

```csharp
public System.Threading.Tasks.Task WriteToStreamAsync(System.Text.Json.Utf8JsonWriter writer, System.Threading.CancellationToken cancellationToken);
```
#### Parameters

<a name='FlowT.Abstractions.StreamableResponse.WriteToStreamAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer connected to HTTP response stream\.

<a name='FlowT.Abstractions.StreamableResponse.WriteToStreamAsync(System.Text.Json.Utf8JsonWriter,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

Cancellation token\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A task representing the asynchronous operation\.