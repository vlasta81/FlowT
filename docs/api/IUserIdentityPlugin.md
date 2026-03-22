## IUserIdentityPlugin Interface

Built\-in plugin that exposes authenticated user identity for the current flow execution\.
Claims are resolved once from [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') on first access and
cached for the lifetime of the flow\. Returns `null` / `false` in non\-HTTP scenarios\.

```csharp
public interface IUserIdentityPlugin
```

Derived  
&#8627; [UserIdentityPlugin](UserIdentityPlugin.md 'FlowT\.Plugins\.UserIdentityPlugin')

### Remarks
Register via `services.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>()`\.

Usage:

```csharp
var identity = context.Plugin<IUserIdentityPlugin>();
if (!identity.IsAuthenticated)
    return FlowInterrupt<object?>.Fail("Unauthorized", 401);

var userId = identity.UserId;         // Guid? from NameIdentifier claim
var email  = identity.Email;          // string? from Email claim
var isAdmin = identity.IsInRole("Admin");
```

| Properties | |
| :--- | :--- |
| [Email](IUserIdentityPlugin.Email.md 'FlowT\.Plugins\.IUserIdentityPlugin\.Email') | Gets the authenticated user's e\-mail address parsed from [System\.Security\.Claims\.ClaimTypes\.Email](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.email 'System\.Security\.Claims\.ClaimTypes\.Email'), or `null` when the claim is absent or in a non\-HTTP scenario\. |
| [IsAuthenticated](IUserIdentityPlugin.IsAuthenticated.md 'FlowT\.Plugins\.IUserIdentityPlugin\.IsAuthenticated') | Gets a value indicating whether the current user is authenticated\. Returns `false` in non\-HTTP scenarios\. |
| [Principal](IUserIdentityPlugin.Principal.md 'FlowT\.Plugins\.IUserIdentityPlugin\.Principal') | Gets the raw [System\.Security\.Claims\.ClaimsPrincipal](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal 'System\.Security\.Claims\.ClaimsPrincipal') for the current request, or `null` in non\-HTTP scenarios\. |
| [UserId](IUserIdentityPlugin.UserId.md 'FlowT\.Plugins\.IUserIdentityPlugin\.UserId') | Gets the authenticated user's ID parsed from [System\.Security\.Claims\.ClaimTypes\.NameIdentifier](https://learn.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes.nameidentifier 'System\.Security\.Claims\.ClaimTypes\.NameIdentifier'), or `null` when unauthenticated or in a non\-HTTP scenario\. |

| Methods | |
| :--- | :--- |
| [IsInRole\(string\)](IUserIdentityPlugin.IsInRole.S6ABSPTVZ4TLH1N8486QDTMA6.md 'FlowT\.Plugins\.IUserIdentityPlugin\.IsInRole\(string\)') | Determines whether the current user belongs to the specified role\. Returns `false` in non\-HTTP scenarios or when unauthenticated\. |
