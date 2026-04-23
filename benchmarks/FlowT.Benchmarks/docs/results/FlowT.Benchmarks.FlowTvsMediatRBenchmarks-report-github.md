```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                          | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |----------:|---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;         |  28.60 ns | 0.616 ns |  0.733 ns |  28.30 ns |  1.00 |    0.04 | 0.0459 |     144 B |        1.00 |
| &#39;MediatR: Simple handler&#39;       | 289.69 ns | 4.372 ns |  4.090 ns | 288.50 ns | 10.13 |    0.29 | 0.2856 |     896 B |        6.22 |
| &#39;FlowT: Handler + 1 policy&#39;     |  63.99 ns | 2.209 ns |  6.514 ns |  61.97 ns |  2.24 |    0.23 | 0.0739 |     232 B |        1.61 |
| &#39;MediatR: Handler + 1 behavior&#39; | 294.05 ns | 5.623 ns | 13.472 ns | 291.58 ns | 10.29 |    0.53 | 0.3133 |     984 B |        6.83 |
| &#39;FlowT: Handler + validation&#39;   |  45.86 ns | 1.179 ns |  3.476 ns |  45.28 ns |  1.60 |    0.13 | 0.0459 |     144 B |        1.00 |
| &#39;MediatR: Handler + validation&#39; | 279.37 ns | 5.627 ns |  8.926 ns | 278.01 ns |  9.77 |    0.39 | 0.2880 |     904 B |        6.28 |
