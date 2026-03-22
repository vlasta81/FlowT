## StreamableResponse\.WriteProperty\(Utf8JsonWriter, string, object\) Method

Helper method to write a simple property to the JSON writer\.
Handles serialization of common types \(numbers, strings, booleans, dates, etc\.\)\.

```csharp
protected void WriteProperty(System.Text.Json.Utf8JsonWriter writer, string name, object? value);
```
#### Parameters

<a name='FlowT.Abstractions.StreamableResponse.WriteProperty(System.Text.Json.Utf8JsonWriter,string,object).writer'></a>

`writer` [System\.Text\.Json\.Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter 'System\.Text\.Json\.Utf8JsonWriter')

The UTF\-8 JSON writer\.

<a name='FlowT.Abstractions.StreamableResponse.WriteProperty(System.Text.Json.Utf8JsonWriter,string,object).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The property name \(camelCase recommended\)\.

<a name='FlowT.Abstractions.StreamableResponse.WriteProperty(System.Text.Json.Utf8JsonWriter,string,object).value'></a>

`value` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property value to serialize\.

### Remarks
Use this in [WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\)](StreamableResponse.WriteMetadataAsync.IAO5PI95GRUPJ5TVUOYKECXT6.md 'FlowT\.Abstractions\.StreamableResponse\.WriteMetadataAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') for brevity:

```csharp
WriteProperty(writer, "totalCount", 12345);
WriteProperty(writer, "page", 0);
```