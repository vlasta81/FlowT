# FlowT Documentation Hub

Welcome to the **FlowT** documentation! This is your central navigation point for all FlowT resources.

> 💡 **New to FlowT?** Start with the [Main README](../README.md) for a quick overview and 5-minute setup guide.

---

## 🗂️ Documentation Structure

### 🚀 Getting Started
- **[Main README](../README.md)** — Overview, quick start, key features
- **[Installation Guide](../README.md#-quick-start)** — Get FlowT running in 5 minutes
- **[Core Concepts](../README.md#-core-concepts)** — Modules, Flows, Context, Specs, Policies
- **[Sample Application](../samples/FlowT.SampleApp/README.md)** — Complete working example with CRUD operations

### 📚 API Reference
- **[Auto-Generated API Docs](api/index.md)** — Complete reference from XML comments
- **[FlowT.links](FlowT.links)** — Direct links to all types and members
- **Regenerate docs:** Run `./GenerateDefaultDocumentation.ps1` from solution root

### 📘 Guides & Best Practices
- **[Best Practices](BEST_PRACTICES.md)** — Thread-safety, performance, DI patterns
- **[FlowContext Complete Guide](FLOWCONTEXT.md)** — ⭐ Full API reference, patterns & named keys
- **[Plugin System Guide](PLUGINS.md)** — PerFlow plugins, `FlowPlugin` base class, `AddFlowPlugin`
- **[Migration Guide](MIGRATION_AddFlows.md)** — ⚠️ Migrate from deprecated `AddFlows()` to `AddFlow<>()`
- **[Namespace Collisions](NAMESPACE_COLLISIONS.md)** — Handling flows with same names in different namespaces
- **[Streaming Guide](../benchmarks/FlowT.Benchmarks/docs/Streaming-Benchmarks.md)** — IAsyncEnumerable streaming patterns

### 📊 Performance & Benchmarks
- **[Benchmark Suite](../benchmarks/FlowT.Benchmarks/README.md)** — Comprehensive performance analysis
- **[Benchmark Results](../benchmarks/FlowT.Benchmarks/docs/results/)** — Detailed measurements
- **[Run Scripts](../benchmarks/FlowT.Benchmarks/scripts/)** — Execute benchmarks locally

**Quick Comparisons:**
- [vs DispatchR](../benchmarks/FlowT.Benchmarks/docs/DispatchR-Comparison.md) — Closest competitor analysis
- [vs MediatR](../benchmarks/FlowT.Benchmarks/docs/results/MediatR-Comparison.md) — Most popular framework
- [Extreme Tests](../benchmarks/FlowT.Benchmarks/EXTREME_README.md) — Stress tests & load scenarios

### 🛡️ Code Quality & Safety
- **[Analyzer Documentation](../src/FlowT.Analyzers/README.md)** — 26 Roslyn diagnostic rules
- **[Thread Safety Patterns](BEST_PRACTICES.md#-architecture)** — Singleton safety guide

**Analyzer Categories:**
- 🔴 **14 Errors** — Build fails (thread-safety, DI anti-patterns, data leaks)
- ⚠️ **9 Warnings** — Should fix (async issues, locking problems)
- ℹ️ **3 Info** — Suggestions (cancellation, empty catch blocks)

### 🧪 Testing & Examples
- **[Unit Tests](../tests/FlowT.Tests/)** — 206+ tests with full coverage
- **[Sample Application](../samples/FlowT.SampleApp/)** — Complete working example

---

## 📋 Documentation Conventions

To maintain consistency across FlowT documentation:

### File Structure
- **README.md** — Entry point for each project/folder
- **docs/** — Detailed guides and reference material
- **api/** — Auto-generated API documentation

### Linking Patterns
- Use **relative paths** for internal links: `../folder/file.md`
- Use **descriptive link text**: `[Guide Name](path)` not `[click here](path)`
- **Benchmark results** live in `benchmarks/.../docs/results/`
- **API docs** are auto-generated — don't edit `docs/api/` manually

### Content Guidelines
- **Code examples**: Use C# syntax highlighting with ```csharp
- **Tables**: Keep benchmark tables in dedicated benchmark docs, use summaries in main README
- **Icons**: Use emoji sparingly for visual scanning (✨ new, ⚠️ warning, 🔴 error)
- **Version info**: Include "Last Updated" date in guide files

---

## 🔄 Updating Documentation

### Regenerate API Docs
```powershell
# From solution root
.\GenerateDefaultDocumentation.ps1
```

This will:
1. Build FlowT project in Release mode
2. Extract XML documentation from `src/FlowT/bin/Release/net10.0/FlowT.xml`
3. Generate markdown files in `docs/api/`
4. Create `docs/FlowT.links` with direct links

### Configuration
- **Config file:** `DefaultDocumentation.json` (solution root)
- **File naming:** `NameAndMd5Mix` (readable names with collision prevention)
- **Output:** `docs/api/`

---

## 🔗 External Resources

- **GitHub Repository:** https://github.com/vlasta81/FlowT
- **NuGet Package:** https://www.nuget.org/packages/FlowT/
- **Issues & Discussions:** https://github.com/vlasta81/FlowT/issues

---

## 💡 Quick Links

### Most Common Tasks
- [Create a new Flow](BEST_PRACTICES.md#-do-use-readonly-fields)
- [Add validation](../README.md#-specifications-iflowspecificationtrequest)
- [Add cross-cutting concern](../README.md#-policies-flowpolicytrequest-tresponse)
- [Resolve scoped services](../README.md#-context-flowcontext)
- [Handle errors with FlowInterrupt](../README.md#-specifications-iflowspecificationtrequest)
- [Use named keys](FLOWCONTEXT.md#21-named-keys---advanced-feature)
- [Stream large datasets](../benchmarks/FlowT.Benchmarks/docs/Streaming-Benchmarks.md)

### Performance Optimization
- [Benchmark results](../benchmarks/FlowT.Benchmarks/README.md)
- [Allocation analysis](../benchmarks/FlowT.Benchmarks/docs/results/Allocations.md)
- [Named keys overhead](../benchmarks/FlowT.Benchmarks/docs/results/NamedKeys.md)
- [Streaming performance](../benchmarks/FlowT.Benchmarks/docs/Streaming-Benchmarks.md)

### Troubleshooting
- [Analyzer errors](../src/FlowT.Analyzers/README.md)
- [Thread safety issues](BEST_PRACTICES.md#-dont-use-mutable-fields)
- [Captive dependency pattern](BEST_PRACTICES.md#-dont-capture-scoped-services)

---

**Last Updated:** 2026-03-24
