```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                          | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;Simple handler only&#39;           |  31.44 ns | 0.644 ns | 0.602 ns |  1.00 |    0.03 | 0.0459 |     144 B |        1.00 |
| &#39;Handler + 1 specification&#39;     |  42.97 ns | 0.611 ns | 0.510 ns |  1.37 |    0.03 | 0.0459 |     144 B |        1.00 |
| &#39;Handler + 1 policy&#39;            |  60.10 ns | 0.769 ns | 0.642 ns |  1.91 |    0.04 | 0.0739 |     232 B |        1.61 |
| &#39;Handler + 1 spec + 3 policies&#39; | 127.47 ns | 0.885 ns | 0.828 ns |  4.06 |    0.08 | 0.1299 |     408 B |        2.83 |
| &#39;Create flow context&#39;           | 107.42 ns | 0.581 ns | 0.515 ns |  3.42 |    0.06 | 0.0637 |     200 B |        1.39 |
