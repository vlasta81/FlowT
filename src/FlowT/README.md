# FlowT - Core Library

The main FlowT orchestration library for .NET.

---

## 📦 Package Information

**Package:** FlowT  
**NuGet:** https://www.nuget.org/packages/FlowT/  
**Targets:**
- ✅ .NET 10.0 (Primary)
- ✅ .NET Standard 2.0 (Compatibility)

---

## 📚 Documentation

- **[Main README](../../README.md)** - Quick start and overview
- **[API Reference](../../docs/api/index.md)** - Complete API documentation
- **[Best Practices](../../docs/BEST_PRACTICES.md)** - Thread-safety and performance
- **[Tests](../../tests/FlowT.Tests/)** - 112+ unit tests with examples

---

## 🛡️ Thread Safety

FlowT components are **registered as singletons** for performance. Use **[FlowT.Analyzers](../FlowT.Analyzers/README.md)** to catch threading issues at compile-time (20 diagnostic rules).

---

## 📄 License

MIT License - see [LICENSE](../../LICENSE) file for details.
