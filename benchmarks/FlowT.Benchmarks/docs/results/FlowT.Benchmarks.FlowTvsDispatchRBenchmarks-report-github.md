```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                            | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;           |  29.56 ns | 0.638 ns | 0.955 ns |  1.00 |    0.04 | 0.0459 |     144 B |        1.00 |
| &#39;DispatchR: Simple handler&#39;       |  82.27 ns | 1.345 ns | 1.258 ns |  2.79 |    0.10 | 0.0331 |     104 B |        0.72 |
| &#39;FlowT: Handler + 1 policy&#39;       |  59.27 ns | 1.049 ns | 1.208 ns |  2.01 |    0.07 | 0.0739 |     232 B |        1.61 |
| &#39;DispatchR: Handler + 1 behavior&#39; | 106.08 ns | 1.706 ns | 1.596 ns |  3.59 |    0.12 | 0.0663 |     208 B |        1.44 |
| &#39;FlowT: Handler + validation&#39;     |  45.09 ns | 0.956 ns | 1.341 ns |  1.53 |    0.07 | 0.0459 |     144 B |        1.00 |
| &#39;DispatchR: Handler + validation&#39; |  82.20 ns | 1.357 ns | 1.203 ns |  2.78 |    0.10 | 0.0356 |     112 B |        0.78 |
