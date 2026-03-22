## FlowContext\.HttpContext Property

Gets the HTTP context for this flow execution, if available\.
This is `null` for non\-HTTP scenarios \(background jobs, console apps, Blazor WebAssembly, message queue handlers, unit tests\)\.

```csharp
public Microsoft.AspNetCore.Http.HttpContext? HttpContext { get; init; }
```

#### Property Value
[Microsoft\.AspNetCore\.Http\.HttpContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext 'Microsoft\.AspNetCore\.Http\.HttpContext')

### Remarks

Having access to [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') allows handlers to:
- Read HTTP request metadata (headers, query params, cookies, path, body)
- Access authenticated user ([Microsoft\.AspNetCore\.Http\.HttpContext\.User](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.user 'Microsoft\.AspNetCore\.Http\.HttpContext\.User'), claims, identity)
- Set HTTP response metadata (status codes, headers, cookies)
- Access connection info (IP addresses, ports, certificates)
- Use per-request storage ([Microsoft\.AspNetCore\.Http\.HttpContext\.Items](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.items 'Microsoft\.AspNetCore\.Http\.HttpContext\.Items'))
- Access low-level features ([Microsoft\.AspNetCore\.Http\.HttpContext\.Features](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.features 'Microsoft\.AspNetCore\.Http\.HttpContext\.Features'))

<strong>⚠️ WARNING - Response Body Access:</strong>

<strong>DO NOT write directly to <see cref="!:HttpContext.Response.Body"/>!</strong>
            FlowT flows return typed responses (TResponse) which are automatically serialized by ASP.NET Core.
            Writing to [HttpContext\.Response\.Body](https://learn.microsoft.com/en-us/dotnet/api/httpcontext.response.body 'HttpContext\.Response\.Body') will cause:
- <strong>Double serialization</strong> - Both your write and FlowT's response will be sent
- <strong>Malformed responses</strong> - Client receives corrupted/invalid data
- <strong>Header conflicts</strong> - Cannot modify headers after body is written

<strong>✅ SAFE operations:</strong>

```csharp
// ✅ Read from request
var user = context.HttpContext.User;
var header = context.HttpContext.Request.Headers["X-Custom"];
var ip = context.HttpContext.Connection.RemoteIpAddress;

// ✅ Set response metadata (before returning)
context.HttpContext.Response.StatusCode = 201;
context.HttpContext.Response.Headers["Location"] = "/api/resource/123";
context.HttpContext.Response.Cookies.Append("session", "xyz");
```

<strong>❌ UNSAFE operations:</strong>

```csharp
// ❌ DO NOT write to Response.Body
await context.HttpContext.Response.Body.WriteAsync(data);
await context.HttpContext.Response.WriteAsync("text");
await context.HttpContext.Response.WriteAsJsonAsync(obj);
```

<strong>Alternative for custom response handling:</strong>

If you need full control over the response (streaming, custom serialization, etc.), 
use a custom middleware or endpoint instead of FlowT, or return an appropriate typed response 
(e.g., [Microsoft\.AspNetCore\.Mvc\.FileStreamResult](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filestreamresult 'Microsoft\.AspNetCore\.Mvc\.FileStreamResult'), [Microsoft\.AspNetCore\.Mvc\.IResult](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iresult 'Microsoft\.AspNetCore\.Mvc\.IResult')).

This does <strong>NOT</strong> violate singleton pattern as [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is per-request scoped.