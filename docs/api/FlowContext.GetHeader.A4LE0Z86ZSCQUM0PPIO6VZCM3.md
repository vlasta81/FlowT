## FlowContext\.GetHeader\(string\) Method

Gets the value of a specific HTTP request header\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\) or the header is not present\.

```csharp
public string? GetHeader(string name);
```
#### Parameters

<a name='FlowT.FlowContext.GetHeader(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the header to retrieve \(case\-insensitive\)\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The first value of the specified header, or `null` if not found\.

### Remarks

This is a convenience method equivalent to `(string?)context.HttpContext?.Request.Headers[name]`.
Use this to access custom headers, authentication tokens, content negotiation, etc.

If the header has multiple values, only the first value is returned.
Use `context.HttpContext?.Request.Headers[name]` directly to access all values.

<strong>⚠️ SECURITY WARNING - USER INPUT:</strong>

Request headers can be controlled by clients and must be treated as <strong>untrusted user input</strong>.
Always validate header values, especially for authentication, authorization, and routing decisions.\<example\>
  \<strong\>✅ SAFE \- Validated header:\</strong\>
  \<code\>
            var apiKey = context\.GetHeader\("X\-API\-Key"\);
            if \(string\.IsNullOrWhiteSpace\(apiKey\) \|\| \!IsValidApiKey\(apiKey\)\)
                return FlowInterrupt\.Fail\("Invalid API key", 401\);
                
            var contentType = context\.GetHeader\("Content\-Type"\);
            if \(contentType \!= "application/json"\)
                return FlowInterrupt\.Fail\("Unsupported content type", 415\);
            \</code\>
  \<strong\>❌ UNSAFE \- Direct usage without validation:\</strong\>
  \<code\>
            // ❌ TRUST CLIENT HEADER FOR AUTHENTICATION \- VULNERABLE\!
            var userId = context\.GetHeader\("X\-User\-Id"\); // Client can set ANY value\!
            var user = await db\.Users\.FindAsync\(userId\); // SECURITY BREACH\!
            
            // ❌ HEADER INJECTION RISK
            var customHeader = context\.GetHeader\("X\-Custom"\);
            context\.SetResponseHeader\("X\-Echo", customHeader\); // Can inject CRLF characters\!
            \</code\>
  \<strong\>✅ SECURE patterns:\</strong\>
  \<code\>
            // ✅ Authentication via middleware \(secure\)
            var user = context\.GetUser\(\); // Already validated by ASP\.NET Core authentication
            
            // ✅ Authorization header with proper validation
            var auth = context\.GetHeader\("Authorization"\);
            if \(auth?\.StartsWith\("Bearer "\) == true\)
            \{
                var token = auth\.Substring\(7\);
                if \(await \_tokenValidator\.ValidateAsync\(token\)\)
                    // Proceed
            \}
            \</code\>
\</example\>