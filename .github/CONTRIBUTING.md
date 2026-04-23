# Contributing to FlowT

Thank you for your interest in contributing! FlowT is a high-performance orchestration library for .NET and every improvement matters.

## Table of Contents

- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Code Guidelines](#code-guidelines)
- [Tests](#tests)
- [Benchmarks](#benchmarks)
- [Pull Request Process](#pull-request-process)

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [PowerShell 7+](https://github.com/PowerShell/PowerShell) (for benchmark scripts)

### Fork and clone

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/<your-username>/FlowT.git
cd FlowT
dotnet build
dotnet test
```

---

## How to Contribute

### Reporting bugs

Use the [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) issue template.  
Always include a minimal code example and your environment details.

### Suggesting features

Use the [Feature Request](.github/ISSUE_TEMPLATE/feature_request.md) issue template.  
Describe the motivation and a proposed API before implementing.

### Submitting benchmark results

Use the [Benchmark Report](.github/ISSUE_TEMPLATE/benchmark_report.md) issue template.  
Reference results are in [`benchmarks/FlowT.Benchmarks/docs/results/`](benchmarks/FlowT.Benchmarks/docs/results/).

---

## Code Guidelines

### Specifications

Prefer the abstract base class over direct interface implementation:

```csharp
// Preferred — Continue() returns a cached static ValueTask (zero alloc)
public class MySpec : FlowSpecification<MyRequest>
{
    public override ValueTask<FlowInterrupt<object?>?> CheckAsync(MyRequest request, FlowContext context)
    {
        if (!IsValid(request))
            return Fail("Validation failed");
        return Continue();
    }
}

// Also valid — but allocates a new ValueTask wrapper on every call
public class MySpec : IFlowSpecification<MyRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(MyRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}
```

### General rules

- All async methods must end with `Async` and accept `CancellationToken`
- No `async void` — use `async Task` or `async ValueTask`
- No direct `IServiceProvider` in handlers/policies — use `context.Service<T>()`
- Public members require XML doc comments
- Follow existing naming and formatting conventions
- No new abstractions/interfaces unless necessary for DI or testing

---

## Tests

All changes to public API require tests. The test project is `tests/FlowT.Tests/`.

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

### Test conventions

- One behavior per test method
- Name: `WhenConditionThenExpectedOutcome` pattern
- Follow Arrange-Act-Assert structure
- No disk I/O without randomized paths
- Test only through public APIs

---

## Benchmarks

Performance changes should include benchmark evidence.

```powershell
# Run relevant suite
.\benchmarks\FlowT.Benchmarks\scripts\run-standard-benchmarks.ps1 -Suite Pipeline -Export -Quick

# Full standard suite
.\benchmarks\FlowT.Benchmarks\scripts\run-standard-benchmarks.ps1 -Export
```

Results are saved to `BenchmarkDotNet.Artifacts/runs/<timestamp>/results/`.  
Compare against the reference results in [`benchmarks/FlowT.Benchmarks/docs/results/`](benchmarks/FlowT.Benchmarks/docs/results/).

---

## Pull Request Process

1. Create a feature branch from `master`:
   ```bash
   git checkout -b feature/your-change
   ```
2. Make your changes with tests
3. Verify build and tests pass:
   ```bash
   dotnet build
   dotnet test
   ```
4. Open a PR against `master` — the [PR template](.github/PULL_REQUEST_TEMPLATE.md) will guide you
5. Address review feedback
6. PR is merged by the maintainer after approval

Direct pushes to `master` are blocked — all changes must go through a PR.

---

## Questions?

Open a [Discussion](https://github.com/vlasta81/FlowT/discussions) or file an issue.
