
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


| Method                                                                    | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| 'Plugin<T>() first access - FlowPlugin (cold)'                            | 204.43 ns | 3.519 ns | 3.291 ns |  1.00 |    0.02 | 0.1478 |     464 B |        1.00 |
| 'Plugin<T>() cached access - FlowPlugin (warm)'                           |  23.53 ns | 0.102 ns | 0.095 ns |  0.12 |    0.00 |      - |         - |        0.00 |
| 'Plugin<T>() first access - plain plugin, no Initialize (cold)'           | 203.35 ns | 3.884 ns | 3.815 ns |  0.99 |    0.02 | 0.1452 |     456 B |        0.98 |
| 'Plugin<T>() cached access - plain plugin (warm)'                         |  22.48 ns | 0.122 ns | 0.114 ns |  0.11 |    0.00 |      - |         - |        0.00 |
| '3 Plugin<T>() types - all first access (cold)'                           | 344.38 ns | 5.490 ns | 5.135 ns |  1.68 |    0.04 | 0.1655 |     520 B |        1.12 |
| '3 Plugin<T>() types - all cached (warm)'                                 |  68.58 ns | 0.334 ns | 0.312 ns |  0.34 |    0.01 |      - |         - |        0.00 |
| 'Pipeline execution - no plugin (baseline)'                               | 153.84 ns | 3.049 ns | 3.630 ns |  0.75 |    0.02 | 0.1249 |     392 B |        0.84 |
| 'Pipeline execution - policy + handler share plugin (one cold, one warm)' | 263.98 ns | 5.242 ns | 5.149 ns |  1.29 |    0.03 | 0.1783 |     560 B |        1.21 |
| 'Pipeline execution - plugin pre-warmed in context (both warm)'           |  82.49 ns | 0.879 ns | 0.822 ns |  0.40 |    0.01 | 0.0305 |      96 B |        0.21 |
