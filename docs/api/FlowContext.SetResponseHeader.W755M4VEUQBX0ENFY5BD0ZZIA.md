## FlowContext\.SetResponseHeader\(string, string\) Method

Sets a custom HTTP response header\.
Does nothing if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\)\.

```csharp
public void SetResponseHeader(string name, string value);
```
#### Parameters

<a name='FlowT.FlowContext.SetResponseHeader(string,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the header to set\.

<a name='FlowT.FlowContext.SetResponseHeader(string,string).value'></a>

`value` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The value of the header\.

### Remarks
This is a convenience method for setting `context.HttpContext.Response.Headers[name]`\.
Use this to add custom headers to the HTTP response\.
Example: `context.SetResponseHeader("Location", "/api/users/123");` for 201 Created responses\.

<strong>⚠️ WARNING:</strong> Set headers <strong>before</strong> returning the response. Do not write to Response.Body.
Common response headers:
- Location - URL of newly created resource (with 201 Created)
- Cache-Control - Caching directives
- ETag - Resource version identifier
- X-Custom-Header - Custom application headers