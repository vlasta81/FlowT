## FlowContext\.IsAuthenticated\(\) Method

Determines whether the current user is authenticated\.
Returns `false` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\)\.

```csharp
public bool IsAuthenticated();
```

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
`true` if the user is authenticated; otherwise, `false`\.

### Remarks
This is a convenience method equivalent to `context.HttpContext?.User?.Identity?.IsAuthenticated == true`\.
Use this to check if a user has successfully authenticated before accessing protected resources\.
Example: `if (!context.IsAuthenticated()) return FlowInterrupt.Abort(new UnauthorizedResponse());`