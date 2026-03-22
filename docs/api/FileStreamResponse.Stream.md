## FileStreamResponse\.Stream Property

Gets or initializes the stream containing the file content\.

```csharp
public System.IO.Stream Stream { get; init; }
```

#### Property Value
[System\.IO\.Stream](https://learn.microsoft.com/en-us/dotnet/api/system.io.stream 'System\.IO\.Stream')

### Remarks

This stream will be automatically disposed after the response is sent.
Ensure the stream supports reading and seeking (for range requests).

<strong>Important:</strong> The stream is consumed asynchronously by ASP.NET Core.
            Do not dispose it manually in your handler.