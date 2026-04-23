```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                                       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| &#39;PublishAsync - 0 handlers&#39;                                  |  32.13 ns |  0.235 ns |  0.209 ns |  1.00 |    0.01 |      - |         - |          NA |
| &#39;PublishAsync - 1 handler&#39;                                   |  43.73 ns |  0.477 ns |  0.446 ns |  1.36 |    0.02 | 0.0102 |      32 B |          NA |
| &#39;PublishAsync - 5 handlers&#39;                                  |  61.01 ns |  0.853 ns |  0.798 ns |  1.90 |    0.03 | 0.0101 |      32 B |          NA |
| &#39;PublishInBackground scheduling cost - 1 handler (no await)&#39; | 875.90 ns |  8.541 ns |  7.571 ns | 27.26 |    0.28 | 0.1488 |     464 B |          NA |
| &#39;PublishInBackground + await - 1 handler&#39;                    | 892.07 ns | 14.757 ns | 13.804 ns | 27.76 |    0.45 | 0.1488 |     464 B |          NA |
| &#39;PublishInBackground + await - 5 handlers&#39;                   | 871.61 ns |  9.382 ns |  8.317 ns | 27.13 |    0.30 | 0.1488 |     464 B |          NA |
