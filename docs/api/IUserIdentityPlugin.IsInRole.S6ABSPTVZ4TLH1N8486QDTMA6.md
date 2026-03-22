## IUserIdentityPlugin\.IsInRole\(string\) Method

Determines whether the current user belongs to the specified role\.
Returns `false` in non\-HTTP scenarios or when unauthenticated\.

```csharp
bool IsInRole(string role);
```
#### Parameters

<a name='FlowT.Plugins.IUserIdentityPlugin.IsInRole(string).role'></a>

`role` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The role name to check\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')