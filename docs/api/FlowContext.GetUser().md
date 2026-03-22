## FlowContext\.GetUser\(\) Method

Gets the authenticated user principal from the HTTP context\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\)\.

```csharp
public System.Security.Claims.ClaimsPrincipal? GetUser();
```

#### Returns
[System\.Security\.Claims\.ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal 'System\.Security\.Claims\.ClaimsPrincipal')  
The [System\.Security\.Claims\.ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal 'System\.Security\.Claims\.ClaimsPrincipal') representing the authenticated user, or `null` if not available\.

### Remarks
This is a convenience method equivalent to `context.HttpContext?.User`\.
Use this to access user identity, claims, and role information in handlers\.
Example: `var userId = context.GetUser()?.FindFirst(ClaimTypes.NameIdentifier)?.Value;`