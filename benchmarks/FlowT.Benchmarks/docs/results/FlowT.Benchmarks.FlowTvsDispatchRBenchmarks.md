```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                            | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;           | 30.78 ns | 0.151 ns | 0.134 ns |  1.00 |    0.01 | 0.0459 |     144 B |        1.00 |
| &#39;DispatchR: Simple handler&#39;       | 86.37 ns | 0.422 ns | 0.395 ns |  2.81 |    0.02 | 0.0331 |     104 B |        0.72 |
| &#39;FlowT: Handler + 1 policy&#39;       | 60.88 ns | 0.397 ns | 0.371 ns |  1.98 |    0.01 | 0.0739 |     232 B |        1.61 |
| &#39;DispatchR: Handler + 1 behavior&#39; | 98.33 ns | 1.986 ns | 2.125 ns |  3.19 |    0.07 | 0.0663 |     208 B |        1.44 |
| &#39;FlowT: Handler + validation&#39;     | 45.29 ns | 0.205 ns | 0.192 ns |  1.47 |    0.01 | 0.0459 |     144 B |        1.00 |
| &#39;DispatchR: Handler + validation&#39; | 82.75 ns | 0.374 ns | 0.331 ns |  2.69 |    0.02 | 0.0356 |     112 B |        0.78 |
