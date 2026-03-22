# Changelog

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
