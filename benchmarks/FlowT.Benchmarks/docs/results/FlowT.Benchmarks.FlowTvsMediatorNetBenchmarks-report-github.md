```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                             | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;            |    28.42 ns |  0.547 ns |  0.630 ns |  1.00 |    0.03 | 0.0459 |     144 B |        1.00 |
| &#39;Mediator.Net: Simple handler&#39;     | 1,513.47 ns |  9.710 ns |  8.108 ns | 53.29 |    1.17 | 0.6828 |    2144 B |       14.89 |
| &#39;FlowT: Handler + 1 policy&#39;        |    56.75 ns |  1.140 ns |  1.066 ns |  2.00 |    0.06 | 0.0739 |     232 B |        1.61 |
| &#39;Mediator.Net: Handler + pipeline&#39; | 1,604.08 ns | 25.216 ns | 22.353 ns | 56.48 |    1.43 | 0.7076 |    2224 B |       15.44 |
