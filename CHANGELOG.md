# Changelog

## [1.3.0] - 2026-04-23

### Added
- **`FlowSpecification<TRequest>` abstract base class**: Optional base for `IFlowSpecification<TRequest>` implementations — provides `Continue()` (cached, zero-allocation), `Fail(message, statusCode=400)`, and `Stop(earlyReturn, statusCode=200)` helpers to eliminate verbose `ValueTask.FromResult<FlowInterrupt<object?>?>()` boilerplate; all 27 analyzers now recognise `FlowSpecification` as a FlowT component alongside `FlowPolicy`
- **6 new built-in plugins**:
- **`FlowT026` analyzer rule** (`SynchronousBlockingAnalyzer`): Separate diagnostic for `Thread.Sleep()` in async methods — previously shared the `FlowT010` ID; now properly isolated as `FlowT026` (total: 27 rules)
- **`PublishInBackground` exception logging**: Unhandled exceptions in background event handlers are now caught and logged via `ILoggerFactory` (resolved from `Services`); if no logger is registered the exception is silently discarded instead of crashing the thread pool
- **`FlowContext.FlowIdString` property**: Replaces the `GetFlowIdString()` method with a read-only property (method marked for removal; backwards-compatible)
- **`InvalidOperationException` on unmapped interrupt**: `FlowDefinition.ExecuteAsync` now throws `InvalidOperationException` with a descriptive message when an interrupt cannot be cast to `TResponse` and no `OnInterrupt()` mapper was registered — previously returned `default!` silently
- **265+ unit tests**: Expanded from 206 (1.2.0) with new coverage for timers, plugins, `FeatureFlagPlugin`, `PublishInBackground` error handling, and `FlowDefinition` edge cases; added `Microsoft.FeatureManagement.AspNetCore` v4.4.0 test dependency

### Changed
- **`FlowContext._plugins` lazy initialization**: Plugin dictionary is now allocated on first `Plugin<T>()` call; contexts that never use plugins save ~104 B allocation
- **`FlowContext._timers` dedicated field**: Timer storage moved from the general-purpose `_items` dictionary to a dedicated `Dictionary<string, long>? _timers` field; eliminates `CompositeKey` overhead for timer operations; field is null until first `StartTimer` call
- **`FlowContext.Plugin<T>()` lockless fast path**: Added a null-check + `TryGetValue` read before acquiring the lock — repeat plugin access no longer contends on `_syncLock`
- **`FlowContext.TryGet<T>` simplified**: Removed outer double-check read outside the lock; single-lock path is cleaner and avoids torn reads on 32-bit platforms
- **`FlowContext.GetHeader` / `GetQueryParam`**: Implementation changed from `.FirstOrDefault()` to direct `(string?)` cast — removes a LINQ allocation on every header/query read
- **`FlowContext.SetCookie`**: Default `CookieOptions` is now a static readonly field (`_defaultCookieOptions`) instead of `new CookieOptions()` per call — eliminates one allocation per cookie write
- **`FlowContext.PublishAsync`**: Internal loop refactored from manual `IEnumerator` to `foreach` — cleaner code, same performance
- **`FlowDefinition` spec initialization**: `LINQ .Select().ToArray()` replaced with a pre-allocated `for` loop — avoids closure allocation during flow initialization
- **`FlowDefinition.ExecuteAsync(IServiceProvider, CT)`**: Removed explicit `HttpContext = null` assignment (property default is already `null`; no behavioral change)
- **`FlowBuilder.Handler`**: Type changed from `Type` to `Type?` — correctly reflects that the handler is optional until `Configure()` completes
- **`CaptiveDependencyAnalyzer`**: Scoped-type matching changed from `Contains` to exact `==` — reduces false positives on type names that contain a known scoped type name as a substring
- **`ConfigureAwaitAnalyzer`**: Removed `IHandler` interface from the FlowT interface check list — was matching non-FlowT handler types

### Fixed
- **`FlowContext` timer storage**: Timers are no longer stored in `_items` under a special `CompositeKey`; calling `TryGet<Dictionary<string, long>>` could accidentally retrieve internal timer state — now impossible
- **`FlowBuilder.Handler` nullable annotation**: Previously typed as non-nullable `Type`, causing a potential `NullReferenceException` if `Configure()` omitted `Handle<T>()`; now `Type?` with proper null checks in `EnsureInitialized`

---

## [1.2.0] - 2026-03-29

### Removed
- **UserIdentityPlugin**: Removed `UserIdentityPlugin.cs` and `IUserIdentityPlugin` interface — user identity functionality is now provided by the core library's built-in user handling via `FlowContext.GetUser()` and `FlowContext.IsAuthenticated()`
- **Documentation cleanup**: Removed UserIdentityPlugin API reference files from `docs/api/`
- **Icon consolidation**: Kept only `icon.png` for NuGet package; backed up other icon variants

### Changed
- **NuGet package**: Unified `README.md` in `src/FlowT/` for both project and NuGet usage; updated `.csproj` reference
- **Documentation structure**: Streamlined API reference to remove obsolete plugin documentation
- **Migration guide**: Added `docs/MIGRATION_UserIdentityPlugin.md` with step-by-step migration instructions

### Added
- **Migration documentation**: `docs/MIGRATION_UserIdentityPlugin.md` — complete guide for transitioning from the removed plugin to built-in `FlowContext` methods
- **Analyzer documentation**: Updated `src/FlowT.Analyzers/README.md` to reflect all 26 diagnostic rules (14 errors, 9 warnings, 3 info)

---

## [1.1.2] - 2026-03-22

### Added
- `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` to `FlowT.Analyzers` — required by `EnforceExtendedAnalyzerRules=true`; lists all 25 FlowT rules (RS2008 compliance)
- Updated API reference docs (`docs/api/`) for plugins, streaming, and `OnInterrupt`

### Changed
- `.editorconfig`: `*.ps1` charset changed from `utf-8` to `utf-8-bom` — prevents Visual Studio from stripping the BOM on save, which is required for Windows PowerShell 5.1 compatibility
- `tasks.ps1 status`: `NUGET_API_KEY` check no longer shows a warning when the local environment variable is absent — GitHub secret is intentionally inaccessible outside CI

### Fixed
- **CI build failure** (GitHub Actions, ubuntu-latest): `EnforceExtendedAnalyzerRules=true` caused RS2008 "Enable analyzer release tracking" to be a build error; fixed by adding the two required markdown tracking files
- **CS8600 nullable warning** in `CancellationTokenSourceStorageAnalyzer.cs`: `as`-cast result assigned to non-nullable `IFieldSymbol` — changed to `IFieldSymbol?`
- **Windows PowerShell 5.1 parse error** in `tasks.ps1`: Unicode `→` (U+2192, UTF-8 `E2 86 92`) contains byte `0x92` which Windows-1250 decodes as U+2019 (right single quotation mark) — PS 5.1 treated it as a closing string delimiter, causing `ParseException` on lines 482 and 530; replaced with ASCII `->` and `<-`

---

## [1.1.0] - 2026-03-22

### Added
- **Plugin system**: `FlowPlugin` abstract base class for context-aware plugins; `FlowContext.Plugin<T>()` resolver (one instance per flow execution, shared across all pipeline stages); `AddFlowPlugin<TPlugin, TImpl>()` DI registration extension
- **Built-in plugin interfaces**: `ICorrelationPlugin` (resolves `X-Correlation-Id` header or falls back to `FlowId`), `IRetryStatePlugin` (shared retry counter across pipeline stages), `ITransactionPlugin` (flow-scoped database transaction coordination), `IUserIdentityPlugin` (authenticated user claims from `HttpContext`)
- **Streaming responses**: `StreamableResponse` abstract base (JSON metadata + streamed collection), `PagedStreamResponse<T>` (zero-boilerplate paginated streaming), `FileStreamResponse` (binary file downloads with ETag/Content-Disposition/range support), `IStreamableResponse` contract for stream detection
- **`FlowContext.HttpContext`**: Optional `HttpContext?` property exposes the ASP.NET Core request/response context to all pipeline stages (null in non-HTTP scenarios)
- **`FlowDefinition.ExecuteAsync(TRequest, HttpContext)`**: Convenience overload that creates a `FlowContext` with services, cancellation token, and `HttpContext` from a single parameter — no boilerplate in endpoints
- **`AddFlow<TFlow, TRequest, TResponse>()`**: Explicit per-flow DI registration with `TryAddSingleton` duplicate protection; replaces error-prone assembly scanning
- **8 new Roslyn analyzers** (total: 25): FlowT011 `MissingHandlerAnalyzer`, FlowT019 `SingletonStateLeakAnalyzer`, FlowT020 `ConfigureAwaitAnalyzer`, FlowT021 `FlowPluginCapturingAnalyzer`, FlowT022 `MultipleHandlerAnalyzer`, FlowT023 `HttpClientInstantiationAnalyzer`, FlowT024 `SynchronousFileIOAnalyzer`, FlowT025 `DirectServiceProviderAccessAnalyzer`
- **206 unit tests**: Expanded from 91 (1.0.0) with full coverage of plugins, streaming, and new analyzers

### Changed
- **`AddFlows()` marked `[Obsolete]`**: Use `AddFlow<TFlow, TRequest, TResponse>()` for explicit per-flow registration or `AddFlowModules()` for modular projects; the scan-based approach could cause duplicate registrations
- **`FlowDefinition.ExecuteAsync(TRequest, IServiceProvider, CancellationToken)`**: Now explicitly sets `HttpContext = null` on the created context (non-breaking; clarifies intent for non-HTTP scenarios)
- **`FlowInterrupt`**: Significantly expanded XML documentation covering validation failure vs. early return semantics, HTTP status code guidance, and usage examples

### Removed
- **`FlowExecutionExtensions`**: Removed separate extension class containing `ExecuteAsync(request, httpContext)` convenience methods; functionality is now a first-class overload on `FlowDefinition<TRequest, TResponse>` directly

---

## [1.0.0] - 2026-03-20

### Added
- **Core orchestration engine**: FlowDefinition, FlowContext, FlowFactory
- **Chain of Responsibility pattern**: IFlowSpecification, IFlowPolicy, IFlowHandler
- **FlowInterrupt**: Type-safe error handling without exceptions
- **Named keys**: Store multiple values of same type in FlowContext with optional string keys
- **Module system**: IFlowModule for vertical slice architecture
- **18 Roslyn analyzers**: Compile-time safety for thread-safety and DI patterns
- **ValueTask optimization**: Zero-allocation fast paths
- **Singleton architecture**: 9-10× faster than MediatR, 84% less memory allocation
- **Comprehensive benchmarks**: BenchmarkDotNet suite comparing FlowT vs MediatR/DispatchR/Wolverine/Brighter/Mediator.Net
- **Streaming support**: IAsyncEnumerable for progressive data delivery
- **File streaming**: FileStreamResponse for efficient file downloads
- **91+ unit tests**: Full test coverage with xUnit
- **Complete documentation**: README, API docs, Best Practices guide
- **NuGet package**: Published to nuget.org

### Technical Details
- **Targets**: .NET 10.0 (primary), .NET Standard 2.0 (compatibility)
- **Performance**: 30.8 ns baseline (vs MediatR 280 ns)
- **Memory**: 144 B allocation (vs MediatR 896 B)
- **Concurrent execution**: Perfect linear scaling with 100 parallel requests
- **Analyzers**: 11 errors, 6 warnings, 3 info rules
