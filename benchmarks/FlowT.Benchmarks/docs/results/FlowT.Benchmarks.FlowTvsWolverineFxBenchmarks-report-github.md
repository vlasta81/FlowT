```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;FlowT: Simple handler&#39;       |  31.13 ns | 0.558 ns | 0.686 ns |  1.00 |    0.03 | 0.0459 |     144 B |        1.00 |
| &#39;WolverineFx: Simple handler&#39; | 432.86 ns | 7.932 ns | 8.487 ns | 13.91 |    0.40 | 0.2546 |     800 B |        5.56 |
| &#39;FlowT: Handler + 1 policy&#39;   |  60.25 ns | 1.037 ns | 0.970 ns |  1.94 |    0.05 | 0.0739 |     232 B |        1.61 |
