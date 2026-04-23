## FlowContext\.GetQueryParam\(string\) Method

Gets the value of a specific query string parameter\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\) or the parameter is not present\.

```csharp
public string? GetQueryParam(string name);
```
#### Parameters

<a name='FlowT.FlowContext.GetQueryParam(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the query parameter to retrieve \(case\-sensitive\)\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The first value of the specified query parameter, or `null` if not found\.

### Remarks

This is a convenience method equivalent to `(string?)context.HttpContext?.Request.Query[name]`.
Use this to access query string parameters from the URL.

If the parameter has multiple values, only the first value is returned.
Use `context.HttpContext?.Request.Query[name]` directly to access all values.

<strong>⚠️ SECURITY WARNING - USER INPUT:</strong>

Query parameters are <strong>untrusted user input</strong> and must be validated/sanitized before use.
Always validate format, range, and content to prevent injection attacks and ensure data integrity.\<example\>
  \<strong\>✅ SAFE \- Validated input:\</strong\>
  \<code\>
            var pageStr = context\.GetQueryParam\("page"\);
            if \(\!int\.TryParse\(pageStr, out var page\) \|\| page &lt; 0 \|\| page &gt; 1000\)
                return FlowInterrupt\.Fail\("Invalid page number"\);
                
            var status = context\.GetQueryParam\("status"\);
            if \(\!Enum\.TryParse&lt;OrderStatus&gt;\(status, out var orderStatus\)\)
                return FlowInterrupt\.Fail\("Invalid status value"\);
            \</code\>
  \<strong\>❌ UNSAFE \- Direct usage without validation:\</strong\>
  \<code\>
            // ❌ SQL INJECTION RISK\!
            var name = context\.GetQueryParam\("name"\);
            var sql = $"SELECT \* FROM Users WHERE Name = '\{name\}'"; // VULNERABLE\!
            
            // ❌ XSS RISK\!
            var message = context\.GetQueryParam\("msg"\);
            return new Response \{ Html = $"&lt;div&gt;\{message\}&lt;/div&gt;" \}; // VULNERABLE\!
            
            // ❌ PATH TRAVERSAL RISK\!
            var file = context\.GetQueryParam\("file"\);
            var path = Path\.Combine\("uploads", file\); // VULNERABLE if file = "\.\./\.\./etc/passwd"
            \</code\>
\</example\>