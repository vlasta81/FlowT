```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                          | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;         |  30.32 ns | 0.331 ns | 0.293 ns |  1.00 |    0.01 | 0.0459 |     144 B |        1.00 |
| &#39;MediatR: Simple handler&#39;       | 280.06 ns | 3.152 ns | 2.794 ns |  9.24 |    0.12 | 0.2856 |     896 B |        6.22 |
| &#39;FlowT: Handler + 1 policy&#39;     |  60.04 ns | 1.162 ns | 1.383 ns |  1.98 |    0.05 | 0.0739 |     232 B |        1.61 |
| &#39;MediatR: Handler + 1 behavior&#39; | 308.86 ns | 6.232 ns | 9.884 ns | 10.19 |    0.33 | 0.3133 |     984 B |        6.83 |
| &#39;FlowT: Handler + validation&#39;   |  46.57 ns | 0.615 ns | 0.576 ns |  1.54 |    0.02 | 0.0459 |     144 B |        1.00 |
| &#39;MediatR: Handler + validation&#39; | 299.50 ns | 5.910 ns | 9.875 ns |  9.88 |    0.33 | 0.2880 |     904 B |        6.28 |
