# Migration Guide: UserIdentityPlugin Removal (v1.2.0)

## Overview

In FlowT v1.2.0, the `UserIdentityPlugin` and `IUserIdentityPlugin` interface have been **removed**. User identity functionality is now provided by built-in methods on `FlowContext`, making the separate plugin redundant and simplifying the API.

## Why Was It Removed?

1. **Redundancy**: The core `FlowContext` already provides all necessary user identity methods
2. **Simplification**: Fewer plugins to register and manage
3. **Performance**: Direct method calls instead of plugin resolution overhead
4. **Clarity**: More intuitive API with methods directly on the context

## Migration Steps

### Step 1: Remove Plugin Registration

**Before (v1.1.x):**
```csharp
// Program.cs or module
services.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>();
```

**After (v1.2.0+):**
```csharp
// Simply remove the registration - no longer needed!
```

### Step 2: Update Code References

Replace all `context.Plugin<IUserIdentityPlugin>()` calls with the corresponding `FlowContext` methods:

| Old Plugin Method | New Built-in Method | Notes |
|------------------|---------------------|-------|
| `context.Plugin<IUserIdentityPlugin>().UserId` | `context.GetUserId()` | Returns `Guid?` |
| `context.Plugin<IUserIdentityPlugin>().Email` | `context.GetUser()?.Email` | Returns `string?` |
| `context.Plugin<IUserIdentityPlugin>().IsAuthenticated` | `context.IsAuthenticated()` | Returns `bool` |
| `context.Plugin<IUserIdentityPlugin>().IsInRole("Admin")` | `context.IsInRole("Admin")` | Returns `bool` |
| `context.Plugin<IUserIdentityPlugin>().Principal` | `context.GetUser()` | Returns `ClaimsPrincipal?` |

### Step 3: Update Imports (if needed)

Remove any `using FlowT.Plugins;` statements that were only importing the plugin interface.

## Code Examples

### Example 1: Authentication Check

**Before:**
```csharp
public class SecureHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var identity = ctx.Plugin<IUserIdentityPlugin>();
        if (!identity.IsAuthenticated)
            return FlowInterrupt<Response>.Fail("Unauthorized", 401);
        
        var userId = identity.UserId;
        // ... rest of logic
    }
}
```

**After:**
```csharp
public class SecureHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        if (!ctx.IsAuthenticated())
            return FlowInterrupt<Response>.Fail("Unauthorized", 401);
        
        var userId = ctx.GetUserId();
        // ... rest of logic
    }
}
```

### Example 2: Role-Based Authorization

**Before:**
```csharp
public class AdminHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var identity = ctx.Plugin<IUserIdentityPlugin>();
        if (!identity.IsInRole("Admin"))
            return FlowInterrupt<Response>.Fail("Forbidden", 403);
        
        // Admin-only logic
    }
}
```

**After:**
```csharp
public class AdminHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        if (!ctx.IsInRole("Admin"))
            return FlowInterrupt<Response>.Fail("Forbidden", 403);
        
        // Admin-only logic
    }
}
```

### Example 3: Getting User Details

**Before:**
```csharp
public class ProfileHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var identity = ctx.Plugin<IUserIdentityPlugin>();
        var email = identity.Email;
        var principal = identity.Principal;
        
        // Use email and principal...
    }
}
```

**After:**
```csharp
public class ProfileHandler : IFlowHandler<Request, Response>
{
    public async ValueTask<Response> HandleAsync(Request req, FlowContext ctx)
    {
        var email = ctx.GetUser()?.Email;
        var principal = ctx.GetUser();
        
        // Use email and principal...
    }
}
```

## FlowContext User Methods Reference

### `bool IsAuthenticated()`
Returns `true` if the current request has an authenticated user.

```csharp
if (context.IsAuthenticated())
{
    // User is logged in
}
```

### `Guid? GetUserId()`
Returns the authenticated user's ID parsed from the `NameIdentifier` claim, or `null` if unauthenticated.

```csharp
var userId = context.GetUserId();
if (userId.HasValue)
{
    // Use userId.Value
}
```

### `ClaimsPrincipal? GetUser()`
Returns the raw `ClaimsPrincipal` for the current request, or `null` in non-HTTP scenarios.

```csharp
var user = context.GetUser();
if (user?.FindFirst("custom_claim")?.Value is var claimValue)
{
    // Use custom claim
}
```

### `bool IsInRole(string role)`
Determines whether the current user belongs to the specified role.

```csharp
if (context.IsInRole("Admin"))
{
    // Admin-only logic
}
```

## Non-HTTP Scenarios

All `FlowContext` user methods return safe defaults in non-HTTP scenarios (background jobs, console apps, tests):

| Method | Return Value in Non-HTTP |
|--------|-------------------------|
| `IsAuthenticated()` | `false` |
| `GetUserId()` | `null` |
| `GetUser()` | `null` |
| `IsInRole(role)` | `false` |

This behavior is identical to the old plugin, so no additional migration is needed for non-HTTP code.

## Testing Considerations

If your unit tests mocked `IUserIdentityPlugin`, update them to use `FlowContext` methods directly:

**Before:**
```csharp
var mockIdentity = new Mock<IUserIdentityPlugin>();
mockIdentity.Setup(x => x.IsAuthenticated).Returns(true);
mockIdentity.Setup(x => x.UserId).Returns(testUserId);

var context = new FlowContext { /* ... */ };
// Plugin resolution would use the mock
```

**After:**
```csharp
// Create a FlowContext with a mock HttpContext for testing
var mockHttpContext = new Mock<HttpContext>();
mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal(
    new ClaimsIdentity(new[] { 
        new Claim(ClaimTypes.NameIdentifier, testUserId.ToString()),
        new Claim(ClaimTypes.Email, "test@example.com")
    }, "test")));

var context = new FlowContext 
{ 
    HttpContext = mockHttpContext.Object,
    // ... other properties
};

// Now context.IsAuthenticated(), context.GetUserId(), etc. will work
```

## Troubleshooting

### Build Error: `IUserIdentityPlugin` not found
**Solution**: Remove the `using FlowT.Plugins;` statement and update all references to use `FlowContext` methods.

### Runtime Error: Plugin not registered
**Solution**: The plugin no longer exists. Remove `AddFlowPlugin<IUserIdentityPlugin, ...>()` calls and use built-in methods.

### NullReferenceException with GetUser()
**Solution**: `GetUser()` returns `null` in non-HTTP scenarios. Always check for null:
```csharp
var email = context.GetUser()?.Email; // Safe null-conditional access
```

## Related Documentation

- [FlowContext Complete Guide](../FLOWCONTEXT.md)
- [Best Practices](../BEST_PRACTICES.md)
- [Plugin System Guide](../PLUGINS.md) - for remaining plugins

## Version History

| Version | Change |
|---------|--------|
| 1.1.x | `UserIdentityPlugin` available as built-in plugin |
| 1.2.0 | `UserIdentityPlugin` removed; functionality moved to `FlowContext` |

---

> 💡 **Tip**: Use your IDE's "Find References" feature to locate all `IUserIdentityPlugin` usages before migrating.
