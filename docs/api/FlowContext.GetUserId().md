## FlowContext\.GetUserId\(\) Method

Gets the authenticated user's identifier from claims\.
Returns `null` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available, user is not authenticated, or the claim is not present\.

```csharp
public string? GetUserId();
```

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The user identifier \(typically from [System\.Security\.Claims\.ClaimTypes\.NameIdentifier](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.nameidentifier 'System\.Security\.Claims\.ClaimTypes\.NameIdentifier') claim\), or `null` if not available\.

### Remarks
This is a convenience method equivalent to `context.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value`\.
The identifier is typically the primary key from your user database \(e\.g\., ASP\.NET Identity's UserId\)\.
Example: `var userId = context.GetUserId() ?? throw new UnauthorizedException();`