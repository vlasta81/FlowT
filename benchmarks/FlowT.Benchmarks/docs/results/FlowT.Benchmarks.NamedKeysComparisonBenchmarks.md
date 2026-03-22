```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                                        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;Set without key (baseline)&#39;                  |  21.50 ns | 0.059 ns | 0.055 ns |  1.00 |    0.00 |      - |         - |          NA |
| &#39;Set with named key&#39;                          |  27.43 ns | 0.060 ns | 0.056 ns |  1.28 |    0.00 |      - |         - |          NA |
| &#39;Set with empty string key&#39;                   |  23.87 ns | 0.059 ns | 0.055 ns |  1.11 |    0.00 |      - |         - |          NA |
| &#39;TryGet without key (baseline)&#39;               |  28.63 ns | 0.077 ns | 0.072 ns |  1.33 |    0.00 |      - |         - |          NA |
| &#39;TryGet with named key&#39;                       |  43.31 ns | 0.156 ns | 0.146 ns |  2.01 |    0.01 |      - |         - |          NA |
| &#39;Set + TryGet without key&#39;                    |  28.89 ns | 0.090 ns | 0.075 ns |  1.34 |    0.00 |      - |         - |          NA |
| &#39;Set + TryGet with named key&#39;                 |  43.33 ns | 0.046 ns | 0.038 ns |  2.02 |    0.01 |      - |         - |          NA |
| &#39;Store 5 values without keys&#39;                 | 152.82 ns | 0.388 ns | 0.363 ns |  7.11 |    0.02 | 0.0253 |      80 B |          NA |
| &#39;Store 5 values (same type) with named keys&#39;  | 148.84 ns | 0.299 ns | 0.279 ns |  6.92 |    0.02 |      - |         - |          NA |
| &#39;Single cache without key&#39;                    |  20.52 ns | 0.055 ns | 0.051 ns |  0.95 |    0.00 |      - |         - |          NA |
| &#39;3 caches with named keys&#39;                    |  79.60 ns | 0.200 ns | 0.177 ns |  3.70 |    0.01 |      - |         - |          NA |
| &#39;Multi-user scenario (without named keys)&#39;    | 115.81 ns | 0.684 ns | 0.606 ns |  5.39 |    0.03 | 0.0229 |      72 B |          NA |
| &#39;Multi-user scenario (with named keys)&#39;       | 140.27 ns | 0.441 ns | 0.413 ns |  6.52 |    0.02 |      - |         - |          NA |
| &#39;Configuration scenario (without named keys)&#39; | 113.73 ns | 0.247 ns | 0.231 ns |  5.29 |    0.02 | 0.0229 |      72 B |          NA |
| &#39;Configuration scenario (with named keys)&#39;    | 165.38 ns | 0.462 ns | 0.432 ns |  7.69 |    0.03 | 0.0229 |      72 B |          NA |
| &#39;Short key (3 chars)&#39;                         |  43.75 ns | 0.087 ns | 0.077 ns |  2.04 |    0.01 |      - |         - |          NA |
| &#39;Medium key (20 chars)&#39;                       |  63.10 ns | 0.203 ns | 0.190 ns |  2.93 |    0.01 |      - |         - |          NA |
| &#39;Long key (50 chars)&#39;                         |  98.98 ns | 0.263 ns | 0.246 ns |  4.60 |    0.02 |      - |         - |          NA |
