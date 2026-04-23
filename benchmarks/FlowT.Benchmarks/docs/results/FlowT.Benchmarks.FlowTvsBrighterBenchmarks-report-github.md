```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                         | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;        |    29.58 ns |  0.639 ns |  0.627 ns |  1.00 |    0.03 | 0.0459 |     144 B |        1.00 |
| &#39;Brighter: Simple handler&#39;     | 2,338.44 ns | 46.732 ns | 51.942 ns | 79.08 |    2.35 | 1.5106 |    4777 B |       33.17 |
| &#39;FlowT: Handler + 1 policy&#39;    |    57.11 ns |  1.169 ns |  1.601 ns |  1.93 |    0.07 | 0.0739 |     232 B |        1.61 |
| &#39;Brighter: Handler + 1 policy&#39; | 2,422.14 ns | 37.902 ns | 33.599 ns | 81.91 |    1.99 | 1.5869 |    4993 B |       34.67 |
