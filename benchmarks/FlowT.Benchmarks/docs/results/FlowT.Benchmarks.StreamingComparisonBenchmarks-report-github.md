```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                | Mean         | Error       | StdDev      | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |-------------:|------------:|------------:|-------:|--------:|-------:|----------:|------------:|
| &#39;Buffered (List&lt;T&gt;)&#39;                  |     680.2 ns |    13.52 ns |    19.81 ns |   1.00 |    0.04 | 2.6455 |    8296 B |        1.00 |
| &#39;Streaming (Sync - no Task.Yield)&#39;    |  17,719.2 ns |    36.27 ns |    28.31 ns |  26.07 |    0.73 | 0.0916 |     336 B |        0.04 |
| &#39;Streaming (Async - with Task.Yield)&#39; | 200,074.3 ns | 3,958.79 ns | 8,521.71 ns | 294.39 |   14.93 |      - |     592 B |        0.07 |
