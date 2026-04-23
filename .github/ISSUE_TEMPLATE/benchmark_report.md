---
name: Benchmark report
about: Share benchmark results or report a performance regression
title: '[BENCHMARK] '
labels: 'performance'
assignees: 'vlasta81'
---

## Benchmark type

- [ ] Performance regression (slower than expected)
- [ ] New benchmark results (different hardware / runtime)
- [ ] Comparison with another library
- [ ] Memory allocation anomaly

## Environment

| | |
|---|---|
| FlowT version | e.g. 1.3.0 |
| .NET version | e.g. .NET 10.0.6 |
| OS | e.g. Windows 11 |
| CPU | e.g. Intel Core i7-12700K |
| BenchmarkDotNet version | e.g. 0.15.8 |

## Results

Paste the BenchmarkDotNet markdown table output:

```
| Method | Mean | Error | StdDev | Gen0 | Allocated |
|------- |-----:|------:|-------:|-----:|----------:|
|        |      |       |        |      |           |
```

## Comparison with reference results

Reference results are in [`benchmarks/FlowT.Benchmarks/docs/results/`](../../benchmarks/FlowT.Benchmarks/docs/results/).

Describe any notable differences:

## Additional context

How were the benchmarks run? Any non-standard configuration?

```powershell
# Command used, e.g.:
.\scripts\run-standard-benchmarks.ps1 -Suite Pipeline -Export
```
