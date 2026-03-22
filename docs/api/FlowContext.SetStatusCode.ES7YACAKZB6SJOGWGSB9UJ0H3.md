## FlowContext\.SetStatusCode\(int\) Method

Sets the HTTP response status code\.
Does nothing if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\)\.

```csharp
public void SetStatusCode(int statusCode);
```
#### Parameters

<a name='FlowT.FlowContext.SetStatusCode(int).statusCode'></a>

`statusCode` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The HTTP status code to set \(e\.g\., 200, 201, 400, 404, 500\)\.

### Remarks
This is a convenience method for setting `context.HttpContext.Response.StatusCode`\.
Use this to return custom status codes based on flow logic\.
Example: `context.SetStatusCode(201);` for created resources\.

<strong>⚠️ WARNING:</strong> Set the status code <strong>before</strong> returning the response. Do not write to Response.Body.
Common status codes:
- 200 (OK) - Default for successful requests
- 201 (Created) - Resource successfully created
- 204 (No Content) - Success with no response body
- 400 (Bad Request) - Invalid input
- 404 (Not Found) - Resource not found
- 500 (Internal Server Error) - Unhandled exception