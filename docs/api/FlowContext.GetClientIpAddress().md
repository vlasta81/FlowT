## FlowContext\.GetClientIpAddress\(\) Method

Gets the client's IP address from the HTTP connection\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\) or the connection info is not available\.

```csharp
public string? GetClientIpAddress();
```

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The string representation of the client's IP address, or `null` if not available\.

### Remarks
This is a convenience method equivalent to `context.HttpContext?.Connection?.RemoteIpAddress?.ToString()`\.
Use this for logging, rate limiting, geo\-location, or security purposes\.
Example: `var clientIp = context.GetClientIpAddress();`
Note: Be aware of proxies and load balancers \- consider checking `X-Forwarded-For` header for the real client IP\.