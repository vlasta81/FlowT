# FlowT.Tests

Comprehensive unit and integration test suite for the FlowT orchestration library.

> 📚 **Documentation Hub:** [docs/index.md](../../docs/index.md) | [Main README](../../README.md)

---

## 📊 Test Coverage

**265+ tests** covering all critical paths and edge cases.

### Test Categories

#### 🧪 Core Functionality
- **[FlowDefinitionTests.cs](FlowDefinitionTests.cs)** - Flow pipeline configuration and execution
  - Pipeline building (specs, policies, handlers)
  - Flow registration and discovery
  - Attribute-based auto-registration
  - Error handling and validation

- **[FlowContextTests.cs](FlowContextTests.cs)** - Context operations and lifecycle
  - Set/Get/TryGet operations
  - Service resolution
  - State management
  - Cancellation token handling

- **[FlowContextNamedKeysTests.cs](FlowContextNamedKeysTests.cs)** - Named keys functionality
  - Multiple values of same type
  - Key collision handling
  - Named vs default key behavior
  - Optional key parameter

- **[NamespaceCollisionTests.cs](NamespaceCollisionTests.cs)** - Namespace isolation guarantees
  - Flows with identical names in different namespaces
  - Registration of both interface and concrete types
  - Correct dispatch when request/response types differ

#### 🔄 Integration Tests
- **[IntegrationTests.cs](IntegrationTests.cs)** - End-to-end flow execution
  - Request/response flows
  - Module registration
  - DI integration
  - Complete pipeline tests

- **[HttpContextIntegrationTests.cs](HttpContextIntegrationTests.cs)** - ASP.NET Core integration
  - HttpContext access
  - Request binding
  - Response serialization
  - Endpoint mapping

- **[PolicyIntegrationTests.cs](PolicyIntegrationTests.cs)** - Policy chain execution
  - Multiple policies
  - Policy ordering
  - Next delegate behavior
  - Exception handling in policies

#### 📤 Streaming & File Handling
- **[StreamingResponseTests.cs](StreamingResponseTests.cs)** - Streaming response patterns
  - IAsyncEnumerable streaming
  - StreamableResponse base class
  - Progressive data delivery
  - Cancellation support

- **[StreamingFlowIntegrationTests.cs](StreamingFlowIntegrationTests.cs)** - End-to-end streaming
  - Complete streaming flows
  - Endpoint integration
  - Memory efficiency validation

- **[FileStreamResponseTests.cs](FileStreamResponseTests.cs)** - File streaming
  - File download responses
  - Content-Type handling
  - Range request support
  - ETag and Last-Modified headers

#### 🛡️ Safety & Reliability
- **[ThreadSafetyTests.cs](ThreadSafetyTests.cs)** - Concurrency and thread-safety
  - Singleton component safety
  - Concurrent flow execution
  - Context isolation
  - No shared mutable state

- **[ConcurrencyAndIsolationTests.cs](ConcurrencyAndIsolationTests.cs)** - Isolation guarantees
  - Per-request context isolation
  - No data leaks between requests
  - Parallel execution correctness

- **[FlowInterruptTests.cs](FlowInterruptTests.cs)** - Type-safe early returns
  - FlowInterrupt creation
  - Success/Fail/Unauthorized patterns
  - Status code handling
  - No exception throwing

#### 🧩 Plugin System
- **[PluginTests.cs](PluginTests.cs)** - Plugin resolution, PerFlow caching, and `FlowPlugin` abstract base class
  - **AddFlowPlugin registration** (4 tests)
    - Plugin registered and resolvable from DI
    - Registered as Transient (new instance per resolution)
    - `TryAdd` semantics — second call does not overwrite first
    - Returns `IServiceCollection` for method chaining
  - **`Plugin<T>()` resolution and PerFlow caching** (5 tests)
    - Returns the registered implementation
    - Same instance for repeated calls on the same `FlowContext`
    - Different instance for different `FlowContext` instances
    - Throws `InvalidOperationException` when type is not registered
    - Plain plugins (no `FlowPlugin` base) resolve correctly
  - **`FlowPlugin` context binding** (5 tests)
    - `Context` property is set after first `Plugin<T>()` call
    - `Context` matches the exact `FlowContext` instance
    - Context is not reset on subsequent cached lookups
    - Plugin can write to flow state via `Context`
    - Plugin can read flow state and resolve services via `Context`
  - **`FlowPlugin` accessibility** (4 tests)
    - `Initialize` method is not public (reflection check)
    - `Initialize` method is `internal` (assembly-level visibility)
    - `Context` property is not public (reflection check)
    - `Context` property is `protected` (family-level visibility)
  - **Pipeline integration** (3 tests)
    - Plugin instance is shared across policy and handler within one execution
    - Plugin instances are isolated between separate flow executions
    - Plugin state does not leak across concurrent contexts

- **[BuiltInPluginTests.cs](BuiltInPluginTests.cs)** - Tests for the 4 built-in framework plugins
  - **`UserIdentityPlugin`** (11 tests)
    - Returns `null`/`false` for all identity properties when `HttpContext` is absent
    - Returns correct `UserId` (Guid), `Email`, `IsAuthenticated`, `IsInRole` from `ClaimsPrincipal`
    - `Principal` property returns the same instance on repeated calls
  - **`CorrelationPlugin`** (5 tests)
    - Falls back to `FlowId` when `HttpContext` is absent or header is missing
    - Returns `X-Correlation-Id` header value when present
    - Returns consistent value on multiple calls; PerFlow cache verified
  - **`RetryStatePlugin`** (7 tests)
    - Initial attempt counter is zero
    - `RegisterAttempt()` increments counter; multiple calls accumulate correctly
    - `ShouldRetry(maxAttempts)` returns correct bool relative to counter
    - Isolated between different `FlowContext` instances
  - **`FlowTransactionPlugin`** (7 tests)
    - Class is abstract (cannot be instantiated directly)
    - Implements `ITransactionPlugin`
    - `IsActive` starts `false`; `BeginAsync` sets it `true`
    - `CommitAsync` and `RollbackAsync` set `IsActive` back to `false`
    - Shared across plugin calls on the same context

- **[NewPluginTests.cs](NewPluginTests.cs)** - Tests for `AuditPlugin`, `TenantPlugin`, `IdempotencyPlugin`, `PerformancePlugin`, `FlowScopePlugin`
  - **`AuditPlugin`** (8 tests)
    - `Entries` is empty initially
    - `Record(action, data?)` adds entry with correct action, data, and UTC timestamp
    - Multiple calls accumulate entries in chronological order
    - Throws `ArgumentException` on null or whitespace action
    - Shared across pipeline stages on the same context
  - **`TenantPlugin`** (6 tests)
    - Falls back to `"default"` when no HTTP context, claim, or header is present
    - Reads tenant from claim `tid` (claim takes precedence over header)
    - Reads tenant from `X-Tenant-Id` header when claim is absent
    - Returns the same value on repeated calls (cached)
  - **`IdempotencyPlugin`** (6 tests)
    - `HasKey` is `false` and `Key` is `null` when `HttpContext` is absent
    - `HasKey` is `true` and `Key` returns header value when `X-Idempotency-Key` is present
    - `HasKey` is `false` when header is absent
    - Returns consistent key on multiple calls
  - **`PerformancePlugin`** (7 tests)
    - `Elapsed` dictionary is empty initially
    - `Measure(name)` does not record while scope is open; records elapsed after `Dispose()`
    - Multiple sections accumulate into `Elapsed` dictionary independently
    - Throws `ArgumentException` on null or whitespace section name
    - Shared across pipeline stages on the same context
  - **`FlowScopePlugin`** (5 tests)
    - `ScopedServices` returns a non-null `IServiceProvider`
    - Returns the same `IServiceProvider` instance on repeated calls (lazy + cached)
    - Can resolve services registered in the root container
    - `ScopedServices` throws `ObjectDisposedException` after `Dispose()`
    - `Dispose()` is idempotent (safe to call multiple times)

- **[FeatureFlagPluginTests.cs](FeatureFlagPluginTests.cs)** - Tests for `FeatureFlagPlugin` (17 tests)
  - **`IsEnabledAsync(feature)`** — simple on/off gate
    - Returns `true` for enabled features, `false` for disabled and unknown
    - Throws `ArgumentNullException` on null feature name
    - Throws `ArgumentException` on whitespace feature name
    - Result is cached: second call does not re-evaluate (mock called once)
  - **`Cache` and `TryGetCached`**
    - `Cache` dictionary is empty before any evaluation
    - Contains evaluated features after `IsEnabledAsync` is called
    - `TryGetCached` returns `false` before evaluation, `true` after
    - Returns the correct boolean value (both enabled and disabled cases)
  - **`IsEnabledAsync<TContext>(feature, context)`** — contextual gate
    - Returns `true` for enabled features with context
    - Throws on null feature name; result is cached
  - **Isolation and wiring**
    - Shared across pipeline stages on the same `FlowContext`
    - Isolated between separate `FlowContext` instances
    - Throws `ArgumentNullException` when `IVariantFeatureManager` is not registered

#### 🔌 Dependency Injection
- **[ServiceCollectionExtensionsTests.cs](ServiceCollectionExtensionsTests.cs)** - DI registration
  - **`AddFlow<TFlow, TRequest, TResponse>()`** (5 tests)
    - Flow registered and executable end-to-end
    - Registered as Singleton (same instance per resolution)
    - Duplicate registrations silently ignored (`TryAddSingleton`)
    - Throws `InvalidOperationException` when `[FlowDefinition]` attribute is missing
    - Pipeline is lazily initialized on first execution
  - **`AddFlows()` — obsolete** (5 tests, backward-compatibility coverage)
    - Scans assembly and registers flows marked with `[FlowDefinition]`
    - Uses calling assembly when no assembly argument provided
    - Registers flows as Singleton
    - Handles multiple flows in one assembly
    - Ignores types without `[FlowDefinition]` attribute
  - **`AddFlowModules()`** (4 tests)
    - Discovers and registers modules marked with `[FlowModule]`
    - Calls `module.Register()` to register module services
    - Uses calling assembly when no assembly argument provided
    - Ignores types without `[FlowModule]` attribute

---

## 🚀 Running Tests

### Run All Tests
```powershell
# From solution root
dotnet test

# From test project
cd tests\FlowT.Tests
dotnet test
```

### Run Specific Test Class
```powershell
dotnet test --filter FlowDefinitionTests
dotnet test --filter StreamingResponseTests
dotnet test --filter ThreadSafetyTests
```

### Run with Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Run in Visual Studio
- Open Test Explorer (Ctrl+E, T)
- Run All / Run Selected
- Debug tests with breakpoints

---

## 📁 Test Structure

### Helpers/
Shared test utilities and base classes:

- **[FlowTestBase.cs](Helpers/FlowTestBase.cs)** - Base class for flow tests
  - Common setup/teardown
  - Service collection configuration
  - Test data builders

- **[TestModels.cs](Helpers/TestModels.cs)** - Test DTOs and models
  - Sample request/response types
  - Test entities
  - Mock data

- **[TestServiceCollectionExtensions.cs](Helpers/TestServiceCollectionExtensions.cs)** - DI test helpers
  - Test service registration
  - Mock service setup

- **[PolicyChainBuilder.cs](Helpers/PolicyChainBuilder.cs)** - Policy testing utilities
  - Fluent policy chain construction
  - Policy execution verification

---

## ✅ Test Quality Standards

### Coverage Requirements
- ✅ **100% coverage** of critical paths
- ✅ **All public APIs** have tests
- ✅ **Edge cases** explicitly tested
- ✅ **Error scenarios** validated

### Test Characteristics
- ✅ **Fast** - No external dependencies, in-memory only
- ✅ **Isolated** - No shared state between tests
- ✅ **Deterministic** - No timing-dependent assertions
- ✅ **Readable** - Clear Arrange-Act-Assert structure

### Naming Convention
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var flow = ...;
    
    // Act
    var result = flow.ExecuteAsync(...);
    
    // Assert
    Assert.Equal(expected, result);
}
```

---

## 🐛 Debugging Tests

### Common Issues

**Test hangs:**
- Check for missing `await` keywords
- Verify CancellationToken is not cancelled
- Look for deadlocks in synchronous code

**Flaky tests:**
- Remove timing assumptions (Task.Delay)
- Use deterministic test data
- Check for shared static state

**DI errors:**
- Ensure all dependencies are registered
- Check service lifetimes (singleton vs scoped)
- Verify flow registration with AddFlows()

---

## 📖 Related Documentation

- **[Main README](../../README.md)** - Library overview
- **[Best Practices](../../docs/BEST_PRACTICES.md)** - Thread-safety patterns
- **[Analyzers](../../src/FlowT.Analyzers/README.md)** - Compile-time safety rules

---

**Test Framework:** xUnit 2.4.2  
**Test Coverage:** 100% of critical paths  
**Last Updated:** 2025-07-11
