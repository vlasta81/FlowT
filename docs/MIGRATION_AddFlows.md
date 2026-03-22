# Migration Guide: AddFlows() → AddFlow<>()

This guide explains how to migrate from the deprecated `AddFlows()` method to the recommended `AddFlow<TFlow, TRequest, TResponse>()` method.

---

## ⚠️ Why Migrate?

### Problem with `AddFlows()`
`AddFlows(assembly)` scans assemblies for all flows marked with `[FlowDefinition]` and registers them automatically. This causes **duplicate registrations** when:

1. **Multiple modules** call `AddFlows()` on the same assembly
2. **Modules + Program.cs** both register flows from the same assembly
3. **Nested assemblies** are scanned multiple times

**Example of the problem:**
```csharp
// ❌ OLD: Duplicate registrations!
[FlowModule]
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlows(typeof(UserModule).Assembly); // Registers ALL flows
    }
}

[FlowModule]
public class OrderModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlows(typeof(OrderModule).Assembly); // Registers ALL flows AGAIN if same assembly!
    }
}

// Result: Flows registered 2× → DI container confusion, wrong instances
```

### Solution: Explicit Registration with Duplicate Protection
`AddFlow<TFlow, TRequest, TResponse>()` registers **one specific flow** explicitly. Since FlowT 1.1+, it uses `TryAddSingleton()` internally, which **prevents duplicate registrations** - calling it multiple times with the same flow is safe (only the first registration is kept).

**Benefits:**
- ✅ **No duplicates** - Even if called multiple times, only one registration exists
- ✅ **Explicit control** - You know exactly which flows are registered
- ✅ **Module isolation** - Each module registers only its own flows
- ✅ **Safe refactoring** - Moving flows between modules won't cause issues

---

## 🚀 Migration Steps

### Step 1: Update Module Registration

**Before (❌ Deprecated):**
```csharp
[FlowModule]
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlows(typeof(UserModule).Assembly); // ❌ Scans entire assembly
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
```

**After (✅ Recommended):**
```csharp
[FlowModule]
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        // ✅ Explicit per-flow registration
        services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
        services.AddFlow<GetUserFlow, GetUserRequest, GetUserResponse>();
        services.AddFlow<UpdateUserFlow, UpdateUserRequest, UpdateUserResponse>();
        services.AddFlow<DeleteUserFlow, DeleteUserRequest, DeleteUserResponse>();
        
        // Register external dependencies (handlers/specs/policies are auto-created)
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
```

### Step 2: Update Program.cs (if applicable)

**Before (❌ Deprecated):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// ❌ Scans assembly for all flows
builder.Services.AddFlows(typeof(Program).Assembly);

var app = builder.Build();
app.Run();
```

**After (✅ Recommended):**

**Option A: Use modules (recommended for large projects):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// ✅ Registers all modules, which register their own flows
builder.Services.AddFlowModules(typeof(Program).Assembly);

var app = builder.Build();
app.MapFlowModules(); // Maps all module endpoints
app.Run();
```

**Option B: Register flows directly (simple projects without modules):**
```csharp
var builder = WebApplication.CreateBuilder(args);

// ✅ Explicit standalone flows
builder.Services.AddFlow<HealthCheckFlow, HealthCheckRequest, HealthCheckResponse>();
builder.Services.AddFlow<ConfigurationFlow, ConfigRequest, ConfigResponse>();

var app = builder.Build();
app.Run();
```

### Step 3: Suppress Obsolete Warnings (if needed)

If you're maintaining legacy code and can't migrate immediately, suppress warnings:

```csharp
#pragma warning disable CS0618 // Type or member is obsolete
services.AddFlows(typeof(MyModule).Assembly);
#pragma warning restore CS0618
```

---

## 📋 Checklist

Use this checklist to ensure complete migration:

- [ ] **Find all `AddFlows()` usages**
  ```powershell
  # Search in solution
  Get-ChildItem -Recurse -Filter *.cs | Select-String "AddFlows"
  ```

- [ ] **Identify all flows in each module**
  ```powershell
  # Find all [FlowDefinition] classes
  Get-ChildItem -Recurse -Filter *.cs | Select-String "\[FlowDefinition\]"
  ```

- [ ] **Replace assembly scans with explicit registrations**
  ```csharp
  // Old: services.AddFlows(assembly);
  // New: services.AddFlow<FlowType, RequestType, ResponseType>();
  ```

- [ ] **Test each module individually**
  - Ensure all flows are registered
  - Check for missing dependencies
  - Verify endpoints still work

- [ ] **Remove `#pragma warning disable` directives**
  - Clean up temporary suppressions
  - Ensure no more `AddFlows()` calls

- [ ] **Run all tests**
  ```powershell
  dotnet test
  ```

- [ ] **Verify DI registrations**
  ```csharp
  // In tests or startup validation
  var flow = serviceProvider.GetRequiredService<IFlow<TRequest, TResponse>>();
  Assert.NotNull(flow);
  ```

---

## 🔍 Common Migration Scenarios

### Scenario 1: Single Module with Multiple Flows

**Before:**
```csharp
services.AddFlows(typeof(UserModule).Assembly); // Registers 10+ flows
```

**After:**
```csharp
services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
services.AddFlow<GetUserFlow, GetUserRequest, GetUserResponse>();
services.AddFlow<UpdateUserFlow, UpdateUserRequest, UpdateUserResponse>();
services.AddFlow<DeleteUserFlow, DeleteUserRequest, DeleteUserResponse>();
services.AddFlow<ListUsersFlow, ListUsersRequest, ListUsersResponse>();
// ... etc.
```

### Scenario 2: Multiple Modules in Same Assembly

**Before (Duplicate registrations!):**
```csharp
// UserModule.cs
services.AddFlows(typeof(UserModule).Assembly); // Registers Users + Orders + Products

// OrderModule.cs
services.AddFlows(typeof(OrderModule).Assembly); // Registers Users + Orders + Products AGAIN
```

**After (No duplicates):**
```csharp
// UserModule.cs
services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
services.AddFlow<GetUserFlow, GetUserRequest, GetUserResponse>();

// OrderModule.cs
services.AddFlow<CreateOrderFlow, CreateOrderRequest, CreateOrderResponse>();
services.AddFlow<GetOrderFlow, GetOrderRequest, GetOrderResponse>();
```

### Scenario 3: Hybrid Approach (Modules + Standalone Flows)

**Before:**
```csharp
builder.Services.AddFlowModules(typeof(Program).Assembly);
builder.Services.AddFlows(typeof(Program).Assembly); // Duplicate!
```

**After:**
```csharp
// Register all modules
builder.Services.AddFlowModules(typeof(Program).Assembly);

// Add standalone flows not in modules
builder.Services.AddFlow<HealthCheckFlow, HealthCheckRequest, HealthCheckResponse>();
builder.Services.AddFlow<MetricsFlow, MetricsRequest, MetricsResponse>();
```

---

## ❓ FAQ

### Q: Do I need to register handlers, specs, and policies?
**A:** No! FlowT automatically creates them using `ActivatorUtilities.CreateInstance()`. You only register:
- ✅ Flows (with `AddFlow<>()`)
- ✅ External dependencies (repositories, services, etc.)

### Q: What if I accidentally call AddFlow<>() multiple times for the same flow?
**A:** It's safe! Since FlowT 1.1+, `AddFlow<>()` uses `TryAddSingleton()` internally, so duplicate calls are ignored. Only the first registration is kept. This prevents issues during refactoring or when multiple code paths register the same flow.

### Q: What if I have 50+ flows in one module?
**A:** Consider splitting into smaller, feature-focused modules. But if needed, explicit registration is still better:
```csharp
// Create a helper method
private static void RegisterUserFlows(IServiceCollection services)
{
    services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
    services.AddFlow<GetUserFlow, GetUserRequest, GetUserResponse>();
    // ... 48 more
}

public void Register(IServiceCollection services)
{
    RegisterUserFlows(services);
    services.AddScoped<IUserRepository, UserRepository>();
}
```

### Q: Can I still use `AddFlows()` for testing?
**A:** Yes, but use `#pragma warning disable CS0618` to suppress obsolete warnings:
```csharp
[Fact]
public void TestFlowRegistration()
{
#pragma warning disable CS0618
    services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618
    
    // Test...
}
```

### Q: When will `AddFlows()` be removed?
**A:** It's marked as obsolete but not yet removed. Plan to migrate before the next major version (2.0).

---

## 📚 Related Documentation

- **[Main README](../README.md)** - Quick start with updated examples
- **[FlowServiceCollectionExtensions API](../docs/api/FlowServiceCollectionExtensions.md)** - Complete API reference
- **[Best Practices](BEST_PRACTICES.md)** - Thread-safety and performance patterns

---

**Migration completed?** ✅ Your project now has explicit, duplicate-free flow registrations!
