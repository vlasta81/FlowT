```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                              | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;StartTimer - first timer (cold, dict allocation)&#39;  | 190.58 ns | 2.550 ns | 2.260 ns |  1.00 |    0.02 | 0.1402 |     440 B |        1.00 |
| &#39;StartTimer - subsequent timer (warm, dict exists)&#39; |  45.90 ns | 0.304 ns | 0.284 ns |  0.24 |    0.00 |      - |         - |        0.00 |
| &#39;StartTimer - 2 sequential timers&#39;                  |  95.46 ns | 0.403 ns | 0.336 ns |  0.50 |    0.01 |      - |         - |        0.00 |
| &#39;StartTimer - 5 nested timers&#39;                      | 239.93 ns | 0.796 ns | 0.744 ns |  1.26 |    0.01 |      - |         - |        0.00 |
| &#39;StartTimer - overwrite same key (5 times)&#39;         |  46.30 ns | 0.169 ns | 0.150 ns |  0.24 |    0.00 |      - |         - |        0.00 |
