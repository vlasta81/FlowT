## Description

Briefly describe what this PR changes and why.

Closes # (issue number, if applicable)

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Performance improvement
- [ ] New / updated Roslyn analyzer rule
- [ ] Documentation update
- [ ] Benchmark update
- [ ] Refactoring (no behavior change)

## Checklist

### Code
- [ ] Code follows the existing style and conventions
- [ ] No new `public` members without XML doc comments
- [ ] No direct `IServiceProvider` access in handlers/policies/specs (use `context.Service<T>()`)
- [ ] Async methods end with `Async` suffix and accept `CancellationToken`
- [ ] No `async void`

### Tests
- [ ] New tests added for changed / new behavior
- [ ] All existing tests pass (`dotnet test`)
- [ ] Tests follow Arrange-Act-Assert pattern

### Specifications / Policies
- [ ] Specifications inherit from `FlowSpecification<TRequest>` (preferred) or implement `IFlowSpecification<TRequest>` directly
- [ ] Passing specs return `Continue()` — not `ValueTask.FromResult(null)`

### Benchmarks (if performance-related)
- [ ] Benchmark added or updated in `benchmarks/FlowT.Benchmarks/`
- [ ] Results compared against `docs/results/` reference

### Documentation
- [ ] README updated if public API changed
- [ ] XML docs added/updated for new public members

## Testing notes

Describe how you tested this change, including any edge cases covered.
