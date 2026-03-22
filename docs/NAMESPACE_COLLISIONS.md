# Namespace Collisions in FlowT

This document explains how FlowT handles flows with the same class name but in different namespaces.

---

## 🔍 Understanding the Behavior

### Key Insight: Two Levels of Registration

When you call `AddFlow<TFlow, TRequest, TResponse>()`, FlowT registers **two things**:

1. **Concrete type** - The specific flow class (e.g., `ModuleA.CreateUserFlow`)
2. **Interface type** - The generic interface (e.g., `IFlow<CreateUserRequest, CreateUserResponse>`)

---

## ✅ Same Name, Different Namespaces = Different Types

**.NET treats these as DIFFERENT types:**

```csharp
namespace ModuleA
{
    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse> { }
}

namespace ModuleB
{
    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse> { }
}
```

**Result:**
- `typeof(ModuleA.CreateUserFlow)` ≠ `typeof(ModuleB.CreateUserFlow)`
- ✅ **Both concrete types are registered** (no collision)
- ✅ `TryAddSingleton()` allows both because they are **different Types**

---

## ⚠️ PROBLEM: Same Request/Response Types

**The issue arises with interface registration:**

```csharp
// Scenario 1: Different request/response types
namespace ModuleA
{
    public record CreateUserRequest(string Email);
    public record CreateUserResponse(Guid Id);

    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse> { }
}

namespace ModuleB
{
    public record CreateUserRequest(string Username);  // Different type!
    public record CreateUserResponse(int UserId);      // Different type!

    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse> { }
}
```

**Registrations:**
- ✅ `ModuleA.CreateUserFlow` → registered as `IFlow<ModuleA.CreateUserRequest, ModuleA.CreateUserResponse>`
- ✅ `ModuleB.CreateUserFlow` → registered as `IFlow<ModuleB.CreateUserRequest, ModuleB.CreateUserResponse>`
- ✅ **Both work** because the generic interfaces are **different types**

---

**Scenario 2: SAME request/response types (PROBLEM!):**

```csharp
// Shared types (defined once, used by both modules)
public record SharedRequest(string Data);
public record SharedResponse(string Result);

namespace ModuleA
{
    [FlowDefinition]
    public class SharedFlow : FlowDefinition<SharedRequest, SharedResponse> { }
}

namespace ModuleB
{
    [FlowDefinition]
    public class SharedFlow : FlowDefinition<SharedRequest, SharedResponse> { }
}
```

**Registrations:**
```csharp
services.AddFlow<ModuleA.SharedFlow, SharedRequest, SharedResponse>();
services.AddFlow<ModuleB.SharedFlow, SharedRequest, SharedResponse>();
```

**What happens:**
1. ✅ **Concrete types:** Both `ModuleA.SharedFlow` and `ModuleB.SharedFlow` are registered (different Types)
2. ⚠️ **Interface:** Only `IFlow<SharedRequest, SharedResponse>` is registered **ONCE**
   - First call registers `ModuleA.SharedFlow` as `IFlow<SharedRequest, SharedResponse>`
   - Second call is **ignored** by `TryAddSingleton()` (interface already registered)

**Result:**
```csharp
// ✅ Works - concrete types
var flowA = provider.GetService<ModuleA.SharedFlow>();       // ✅ Returns ModuleA.SharedFlow
var flowB = provider.GetService<ModuleB.SharedFlow>();       // ✅ Returns ModuleB.SharedFlow

// ⚠️ Problem - interface returns ONLY the first registration
var flow = provider.GetService<IFlow<SharedRequest, SharedResponse>>();  // ⚠️ Returns ModuleA.SharedFlow only!
```

---

## 🎯 Best Practices

### 1. ✅ Each Flow Should Have Unique Request/Response Types

**Good:**
```csharp
namespace UserManagement
{
    public record CreateUserRequest(string Email);
    public record CreateUserResponse(Guid UserId);

    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse> { }
}

namespace AdminPanel
{
    public record CreateAdminRequest(string Username);
    public record CreateAdminResponse(int AdminId);

    [FlowDefinition]
    public class CreateAdminFlow : FlowDefinition<CreateAdminRequest, CreateAdminResponse> { }
}
```

✅ Different request/response types → No collision → Both flows work perfectly

---

### 2. ⚠️ Avoid Shared Request/Response Types Across Different Flows

**Bad:**
```csharp
// ❌ Multiple flows using same request/response
public record GenericRequest(string Data);
public record GenericResponse(string Result);

namespace ModuleA
{
    [FlowDefinition]
    public class ProcessDataFlow : FlowDefinition<GenericRequest, GenericResponse> { }
}

namespace ModuleB
{
    [FlowDefinition]
    public class HandleDataFlow : FlowDefinition<GenericRequest, GenericResponse> { }
}

// ❌ Only ONE can be injected as IFlow<GenericRequest, GenericResponse>!
```

**Why is this bad?**
- Only the **first registered flow** will be resolvable via `IFlow<GenericRequest, GenericResponse>`
- The second flow is **invisible** to DI when injecting by interface
- Leads to confusing bugs: "Why is ModuleB.HandleDataFlow never called?"

---

### 3. ✅ If You MUST Use Same Request/Response, Inject Concrete Type

**Workaround:**
```csharp
public class MyController
{
    private readonly ModuleA.SharedFlow _flowA;
    private readonly ModuleB.SharedFlow _flowB;

    // ✅ Inject CONCRETE types instead of IFlow<,>
    public MyController(
        ModuleA.SharedFlow flowA,
        ModuleB.SharedFlow flowB)
    {
        _flowA = flowA;
        _flowB = flowB;
    }

    public async Task<IActionResult> ProcessA(SharedRequest request)
    {
        var context = new FlowContext { Services = _services, CancellationToken = HttpContext.RequestAborted };
        var result = await _flowA.ExecuteAsync(request, context);
        return Ok(result);
    }

    public async Task<IActionResult> ProcessB(SharedRequest request)
    {
        var context = new FlowContext { Services = _services, CancellationToken = HttpContext.RequestAborted };
        var result = await _flowB.ExecuteAsync(request, context);
        return Ok(result);
    }
}
```

---

## 📊 Summary Matrix

| Scenario | Concrete Types Registered | Interface Registered | Result |
|----------|---------------------------|----------------------|--------|
| **Same name, different namespace, different request/response** | ✅ Both | ✅ Both (different interfaces) | ✅ **Perfect - no collision** |
| **Same name, different namespace, SAME request/response** | ✅ Both | ⚠️ Only first | ⚠️ **Collision on interface** |
| **Different names, SAME request/response** | ✅ Both | ⚠️ Only first | ⚠️ **Collision on interface** |

---

## 🧪 Verified Behavior (Test Results)

The behavior described above is **verified by unit tests** in `NamespaceCollisionTests.cs`:

1. ✅ **AddFlow_RegistersBothFlows_WhenSameNameDifferentNamespaces** - Concrete types work
2. ✅ **AddFlow_RegistersBothInterfaces_WhenDifferentRequestResponseTypes** - Interfaces work when request/response differ
3. ✅ **AddFlow_RegistersOnlyFirstFlow_WhenSameRequestResponseTypes** - Interface collision confirmed

---

## 🔧 Technical Explanation

**Why does this happen?**

```csharp
// FlowServiceCollectionExtensions.cs
private static void RegisterFlowType(IServiceCollection services, Type flowType)
{
    // 1. Concrete type registration
    services.TryAddSingleton(flowType);  
    // ✅ typeof(ModuleA.SharedFlow) ≠ typeof(ModuleB.SharedFlow) → Both registered

    // 2. Interface registration
    Type interfaceType = typeof(IFlow<,>).MakeGenericType(requestType, responseType);
    services.TryAddSingleton(interfaceType, sp => { ... });
    // ⚠️ IFlow<SharedRequest, SharedResponse> is SAME type for both flows → Only first registered
}
```

`TryAddSingleton()` checks if `serviceType` already exists:
- For **concrete types**: `ModuleA.SharedFlow` ≠ `ModuleB.SharedFlow` → Both added
- For **interfaces**: `IFlow<SharedRequest, SharedResponse>` == `IFlow<SharedRequest, SharedResponse>` → Second ignored

---

## 📖 Related Documentation

- **[Migration Guide](MIGRATION_AddFlows.md)** - Migrating from AddFlows() to AddFlow<>()
- **[Best Practices](BEST_PRACTICES.md)** - Thread-safety and singleton patterns
- **[Main README](../README.md)** - FlowT overview and quick start

---

**Last Updated:** 2025-01-16
