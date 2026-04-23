```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                                 | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;IFlowSpecification — ValueTask.FromResult(null)&#39;      | 44.59 ns | 0.442 ns | 0.414 ns |  1.00 |    0.01 | 0.0408 |     128 B |        1.00 |
| &#39;FlowSpecification — Continue() cached&#39;                | 40.78 ns | 0.842 ns | 0.865 ns |  0.91 |    0.02 | 0.0408 |     128 B |        1.00 |
| &#39;IFlowSpecification — ValueTask.FromResult(Fail(...))&#39; | 26.94 ns | 0.438 ns | 0.366 ns |  0.60 |    0.01 | 0.0306 |      96 B |        0.75 |
| &#39;FlowSpecification — Fail(...) helper&#39;                 | 24.90 ns | 0.545 ns | 0.510 ns |  0.56 |    0.01 | 0.0306 |      96 B |        0.75 |
