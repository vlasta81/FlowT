```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                                | Mean         | Error       | StdDev      | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |-------------:|------------:|------------:|-------:|--------:|-------:|----------:|------------:|
| 'Buffered (List<T>)'                  |     726.4 ns |    11.95 ns |     9.98 ns |   1.00 |    0.02 | 2.6455 |    8296 B |        1.00 |
| 'Streaming (Sync - no Task.Yield)'    |  18,036.5 ns |   111.36 ns |   104.16 ns |  24.83 |    0.35 | 0.0916 |     336 B |        0.04 |
| 'Streaming (Async - with Task.Yield)' | 220,863.4 ns | 1,886.81 ns | 1,764.93 ns | 304.11 |    4.59 |      - |     592 B |        0.07 |
