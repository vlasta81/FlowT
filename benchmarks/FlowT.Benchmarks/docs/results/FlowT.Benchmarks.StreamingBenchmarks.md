```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                   | Mean           | Error        | StdDev       | Ratio     | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------- |---------------:|-------------:|-------------:|----------:|--------:|--------:|----------:|------------:|
| 'Buffered 100 items'     |       139.6 ns |      1.70 ns |      1.59 ns |      1.00 |    0.02 |  0.3493 |    1096 B |        1.00 |
| 'Streaming 100 items'    |    20,915.1 ns |    244.75 ns |    204.37 ns |    149.89 |    2.17 |  0.1831 |     600 B |        0.55 |
| 'Buffered 1,000 items'   |       681.7 ns |      7.91 ns |      7.40 ns |      4.89 |    0.07 |  2.6455 |    8296 B |        7.57 |
| 'Streaming 1,000 items'  |   203,164.3 ns |  2,689.68 ns |  2,384.33 ns |  1,456.00 |   22.96 |       - |     664 B |        0.61 |
| 'Buffered 10,000 items'  |     6,309.4 ns |     22.50 ns |     21.05 ns |     45.22 |    0.52 | 24.9939 |   80296 B |       73.26 |
| 'Streaming 10,000 items' | 1,980,037.3 ns | 33,296.75 ns | 27,804.29 ns | 14,190.13 |  247.08 |       - |     664 B |        0.61 |
