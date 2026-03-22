## UserIdentityPlugin Class

Default implementation of [IUserIdentityPlugin](IUserIdentityPlugin.md 'FlowT\.Plugins\.IUserIdentityPlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class UserIdentityPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IUserIdentityPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; UserIdentityPlugin

Implements [IUserIdentityPlugin](IUserIdentityPlugin.md 'FlowT\.Plugins\.IUserIdentityPlugin')

| Properties | |
| :--- | :--- |
| [Email](UserIdentityPlugin.Email.md 'FlowT\.Plugins\.UserIdentityPlugin\.Email') | Gets the authenticated user's e\-mail address parsed from [System\.Security\.Claims\.ClaimTypes\.Email](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.email 'System\.Security\.Claims\.ClaimTypes\.Email'), or `null` when the claim is absent or in a non\-HTTP scenario\. |
| [IsAuthenticated](UserIdentityPlugin.IsAuthenticated.md 'FlowT\.Plugins\.UserIdentityPlugin\.IsAuthenticated') | Gets a value indicating whether the current user is authenticated\. Returns `false` in non\-HTTP scenarios\. |
| [Principal](UserIdentityPlugin.Principal.md 'FlowT\.Plugins\.UserIdentityPlugin\.Principal') | Gets the raw [System\.Security\.Claims\.ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal 'System\.Security\.Claims\.ClaimsPrincipal') for the current request, or `null` in non\-HTTP scenarios\. |
| [UserId](UserIdentityPlugin.UserId.md 'FlowT\.Plugins\.UserIdentityPlugin\.UserId') | Gets the authenticated user's ID parsed from [System\.Security\.Claims\.ClaimTypes\.NameIdentifier](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.nameidentifier 'System\.Security\.Claims\.ClaimTypes\.NameIdentifier'), or `null` when unauthenticated or in a non\-HTTP scenario\. |

| Methods | |
| :--- | :--- |
| [IsInRole\(string\)](UserIdentityPlugin.IsInRole.3E0H6HI61SIPILR853PLYIM58.md 'FlowT\.Plugins\.UserIdentityPlugin\.IsInRole\(string\)') | Determines whether the current user belongs to the specified role\. Returns `false` in non\-HTTP scenarios or when unauthenticated\. |
