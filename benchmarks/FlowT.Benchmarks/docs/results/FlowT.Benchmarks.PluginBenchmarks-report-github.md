```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                                                    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| &#39;Plugin&lt;T&gt;() first access - FlowPlugin (cold)&#39;                            | 190.347 ns | 2.9640 ns | 2.6275 ns |  1.00 |    0.02 | 0.1504 |     472 B |        1.00 |
| &#39;Plugin&lt;T&gt;() cached access - FlowPlugin (warm)&#39;                           |   7.608 ns | 0.0785 ns | 0.0734 ns |  0.04 |    0.00 |      - |         - |        0.00 |
| &#39;Plugin&lt;T&gt;() first access - plain plugin, no Initialize (cold)&#39;           | 189.049 ns | 3.0280 ns | 2.6843 ns |  0.99 |    0.02 | 0.1478 |     464 B |        0.98 |
| &#39;Plugin&lt;T&gt;() cached access - plain plugin (warm)&#39;                         |   7.655 ns | 0.1441 ns | 0.1348 ns |  0.04 |    0.00 |      - |         - |        0.00 |
| &#39;3 Plugin&lt;T&gt;() types - all first access (cold)&#39;                           | 308.530 ns | 4.5140 ns | 3.7694 ns |  1.62 |    0.03 | 0.1683 |     528 B |        1.12 |
| &#39;3 Plugin&lt;T&gt;() types - all cached (warm)&#39;                                 |  22.702 ns | 0.1763 ns | 0.1563 ns |  0.12 |    0.00 |      - |         - |        0.00 |
| &#39;Pipeline execution - no plugin (baseline)&#39;                               | 138.454 ns | 1.6026 ns | 1.3382 ns |  0.73 |    0.01 | 0.1018 |     320 B |        0.68 |
| &#39;Pipeline execution - policy + handler share plugin (one cold, one warm)&#39; | 236.359 ns | 4.0308 ns | 4.3130 ns |  1.24 |    0.03 | 0.1810 |     568 B |        1.20 |
| &#39;Pipeline execution - plugin pre-warmed in context (both warm)&#39;           |  54.888 ns | 0.4880 ns | 0.4564 ns |  0.29 |    0.00 | 0.0306 |      96 B |        0.20 |
