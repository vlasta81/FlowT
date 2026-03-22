# FlowT Documentation Hub

Welcome to the **FlowT** documentation! This is your central navigation point for all FlowT resources.

---

## 📖 Documentation Categories

### 🚀 Getting Started
- **[Main README](../README.md)** - Overview, quick start, key features
- **[Installation & Setup](../README.md#-quick-start)** - Get FlowT running in 5 minutes
- **[Core Concepts](../README.md#-core-concepts)** - Modules, Flows, Context, Specs, Policies

### 📚 API Reference
- **[API Documentation](api/index.md)** - Complete auto-generated API reference
- **[FlowT.links](FlowT.links)** - Direct links to all types and members
- **Generate:** Run `.\GenerateDefaultDocumentation.ps1` from solution root

### 📘 Guides & Best Practices
- **[Best Practices](BEST_PRACTICES.md)** - Thread-safety, performance, DI patterns
- **[FlowContext Complete Guide](FLOWCONTEXT.md)** - ⭐ Full API reference, patterns & named keys
- **[Plugin System Guide](PLUGINS.md)** - PerFlow plugins, `FlowPlugin` base class, `AddFlowPlugin`
- **[Migration Guide: AddFlows() → AddFlow<>()](MIGRATION_AddFlows.md)** - ⚠️ Migrate from deprecated AddFlows()
- **[Namespace Collisions](NAMESPACE_COLLISIONS.md)** - Flows with same names in different namespaces
- **[Streaming Guide](../benchmarks/FlowT.Benchmarks/docs/Streaming-Benchmarks.md)** - IAsyncEnumerable streaming patterns

### 📊 Performance & Benchmarks
- **[Benchmark Suite](../benchmarks/FlowT.Benchmarks/README.md)** - Comprehensive performance analysis
- **[Benchmark Results](../benchmarks/FlowT.Benchmarks/docs/results/)** - Detailed measurements
- **[Run Scripts](../benchmarks/FlowT.Benchmarks/scripts/)** - Execute benchmarks locally

**Key Comparisons:**
- [DispatchR Comparison](../benchmarks/FlowT.Benchmarks/docs/DispatchR-Comparison.md) - FlowT vs closest competitor
- [MediatR Comparison](../benchmarks/FlowT.Benchmarks/docs/results/MediatR-Comparison.md) - FlowT vs most popular
- [Extreme Benchmarks](../benchmarks/FlowT.Benchmarks/EXTREME_README.md) - Stress tests & load scenarios

### 🛡️ Code Quality & Safety
- **[Analyzer Documentation](../src/FlowT.Analyzers/README.md)** - 20 Roslyn diagnostic rules
- **[Thread Safety Patterns](BEST_PRACTICES.md#-architecture)** - Singleton safety guide

**Analyzer Categories:**
- **11 Errors** - Build fails (thread-safety, DI anti-patterns, data leaks)
- **6 Warnings** - Should fix (async issues, locking problems)
- **3 Info** - Suggestions (cancellation, empty catch blocks)

### 🧪 Testing & Examples
- **[Unit Tests](../tests/FlowT.Tests/)** - 112+ tests with full coverage
- **[Sample Application](../samples/FlowT.SampleApp/)** - Complete working example
  - ⚠️ **Note:** Sample app is outdated, will be updated soon

---

## 📂 Documentation Structure

```
docs/
├── index.md (this file)          # Documentation hub & navigation
├── BEST_PRACTICES.md             # Thread-safety & performance guide
├── FLOWCONTEXT.md                # Complete FlowContext guide (includes named keys)
├── MIGRATION_AddFlows.md         # Migration guide
├── NAMESPACE_COLLISIONS.md       # Namespace collision handling
├── PLUGINS.md                    # Plugin system guide (FlowPlugin, AddFlowPlugin)
├── FlowT.links                   # Direct API links (auto-generated)
└── api/                          # Auto-generated API reference
    ├── index.md                  # API entry point
    ├── FlowT.md                  # Root namespace
    ├── FlowT.Abstractions.md     # Base classes
    ├── FlowT.Contracts.md        # Interfaces
    └── ...                       # Individual type documentation
```

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

**Last Updated:** 2025-01-16
