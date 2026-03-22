## FlowContext\.GetRouteValue\(string\) Method

Gets the value of a specific route parameter\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\) or the route parameter is not present\.

```csharp
public string? GetRouteValue(string name);
```
#### Parameters

<a name='FlowT.FlowContext.GetRouteValue(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the route parameter to retrieve \(case\-insensitive\)\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The string representation of the route parameter value, or `null` if not found\.

### Remarks

This is a convenience method equivalent to `context.HttpContext?.GetRouteValue(name)?.ToString()`.
Use this to access route parameters defined in your endpoint patterns.
Example: `var userId = context.GetRouteValue("id");` for route pattern `"/users/{id}"`.

<strong>⚠️ SECURITY WARNING - USER INPUT:</strong>

Route parameters are <strong>untrusted user input</strong> derived from URL segments.
Always validate format, type, and authorization before using them to access resources.\<example\>
  \<strong\>✅ SAFE \- Validated route parameter:\</strong\>
  \<code\>
            var idStr = context\.GetRouteValue\("id"\);
            if \(\!int\.TryParse\(idStr, out var id\) \|\| id &lt;= 0\)
                return FlowInterrupt\.Fail\("Invalid ID", 400\);
                
            // ✅ Check authorization
            var userId = context\.GetUserId\(\);
            var order = await db\.Orders\.FindAsync\(id\);
            if \(order?\.UserId \!= userId\)
                return FlowInterrupt\.Fail\("Forbidden", 403\);
            \</code\>
  \<strong\>❌ UNSAFE \- Direct usage without validation:\</strong\>
  \<code\>
            // ❌ IDOR \(Insecure Direct Object Reference\) vulnerability
            var id = context\.GetRouteValue\("userId"\);
            var user = await db\.Users\.FindAsync\(id\); // No authorization check\!
            return new UserResponse \{ Email = user\.Email \}; // LEAKED\!
            
            // ❌ Path traversal risk
            var filename = context\.GetRouteValue\("file"\);
            var path = Path\.Combine\("uploads", filename\); // VULNERABLE if filename = "\.\./\.\./etc/passwd"
            \</code\>
\</example\>