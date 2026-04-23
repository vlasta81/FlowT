```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;Simple handler only&#39;                         |  29.77 ns | 0.576 ns | 0.511 ns |  1.00 |    0.02 | 0.0459 |     144 B |        1.00 |
| &#39;Handler + 1 specification&#39;                   |  37.77 ns | 0.714 ns | 0.929 ns |  1.27 |    0.04 | 0.0459 |     144 B |        1.00 |
| &#39;Handler + 1 policy&#39;                          |  65.45 ns | 1.263 ns | 1.598 ns |  2.20 |    0.06 | 0.0739 |     232 B |        1.61 |
| &#39;Handler + 1 spec + 3 policies&#39;               | 124.21 ns | 1.372 ns | 1.284 ns |  4.17 |    0.08 | 0.1299 |     408 B |        2.83 |
| &#39;Handler + 5 specifications (all pass)&#39;       |  77.31 ns | 0.586 ns | 0.549 ns |  2.60 |    0.05 | 0.0459 |     144 B |        1.00 |
| &#39;Spec fails - early interrupt, no handler&#39;    |  24.36 ns | 0.412 ns | 0.365 ns |  0.82 |    0.02 | 0.0306 |      96 B |        0.67 |
| &#39;ExecuteAsync(IServiceProvider, CT) overload&#39; | 144.51 ns | 1.571 ns | 1.393 ns |  4.86 |    0.09 | 0.1173 |     368 B |        2.56 |
| &#39;Create flow context&#39;                         | 111.03 ns | 1.366 ns | 1.066 ns |  3.73 |    0.07 | 0.0714 |     224 B |        1.56 |
