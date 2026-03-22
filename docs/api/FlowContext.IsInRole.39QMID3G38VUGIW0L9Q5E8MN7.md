## FlowContext\.IsInRole\(string\) Method

Determines whether the current user belongs to the specified role\.
Returns `false` if [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is not available \(non\-HTTP scenarios\) or user is not in the role\.

```csharp
public bool IsInRole(string role);
```
#### Parameters

<a name='FlowT.FlowContext.IsInRole(string).role'></a>

`role` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the role to check\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
`true` if the user is in the specified role; otherwise, `false`\.

### Remarks
This is a convenience method equivalent to `context.HttpContext?.User?.IsInRole(role) == true`\.
Use this for role\-based authorization in handlers\.
Example: `if (!context.IsInRole("Admin")) return FlowInterrupt.Abort(new ForbiddenResponse());`