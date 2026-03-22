## FlowContext\.SetCookie\(string, string, CookieOptions\) Method

Appends a cookie to the HTTP response\.
Does nothing if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\)\.

```csharp
public void SetCookie(string key, string value, Microsoft.AspNetCore.Http.CookieOptions? options=null);
```
#### Parameters

<a name='FlowT.FlowContext.SetCookie(string,string,Microsoft.AspNetCore.Http.CookieOptions).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the cookie\.

<a name='FlowT.FlowContext.SetCookie(string,string,Microsoft.AspNetCore.Http.CookieOptions).value'></a>

`value` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The value of the cookie\.

<a name='FlowT.FlowContext.SetCookie(string,string,Microsoft.AspNetCore.Http.CookieOptions).options'></a>

`options` [Microsoft\.AspNetCore\.Http\.CookieOptions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.cookieoptions 'Microsoft\.AspNetCore\.Http\.CookieOptions')

Optional cookie options \(expiration, path, domain, security flags\)\. Defaults to empty options if `null`\.

### Remarks
This is a convenience method for `context.HttpContext.Response.Cookies.Append(key, value, options)`\.
Use this to set cookies for session management, preferences, tracking, etc\.
Example: `context.SetCookie("session", sessionId, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });`

<strong>⚠️ WARNING:</strong> Set cookies <strong>before</strong> returning the response. Do not write to Response.Body.

<strong>Security best practices:</strong>
- <strong>HttpOnly = true</strong> - Prevents JavaScript access (XSS protection)
- <strong>Secure = true</strong> - Only send over HTTPS
- <strong>SameSite = Strict/Lax</strong> - CSRF protection
- <strong>Expires</strong> - Set explicit expiration for persistent cookies