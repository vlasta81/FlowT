```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                        | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| &#39;Set without key (baseline)&#39;                  |  20.91 ns | 0.199 ns | 0.186 ns |  1.00 |    0.01 |      - |         - |          NA |
| &#39;Set with named key&#39;                          |  27.96 ns | 0.159 ns | 0.133 ns |  1.34 |    0.01 |      - |         - |          NA |
| &#39;Set with empty string key&#39;                   |  23.48 ns | 0.187 ns | 0.175 ns |  1.12 |    0.01 |      - |         - |          NA |
| &#39;TryGet without key (baseline)&#39;               |  42.91 ns | 0.318 ns | 0.297 ns |  2.05 |    0.02 |      - |         - |          NA |
| &#39;TryGet with named key&#39;                       |  55.75 ns | 0.267 ns | 0.250 ns |  2.67 |    0.03 |      - |         - |          NA |
| &#39;Set + TryGet without key&#39;                    |  42.33 ns | 0.248 ns | 0.232 ns |  2.02 |    0.02 |      - |         - |          NA |
| &#39;Set + TryGet with named key&#39;                 |  52.78 ns | 0.353 ns | 0.330 ns |  2.52 |    0.03 |      - |         - |          NA |
| &#39;Store 5 values without keys&#39;                 | 147.94 ns | 1.234 ns | 1.155 ns |  7.08 |    0.08 | 0.0253 |      80 B |          NA |
| &#39;Store 5 values (same type) with named keys&#39;  | 144.39 ns | 1.407 ns | 1.316 ns |  6.91 |    0.08 |      - |         - |          NA |
| &#39;Single cache without key&#39;                    |  20.29 ns | 0.107 ns | 0.100 ns |  0.97 |    0.01 |      - |         - |          NA |
| &#39;3 caches with named keys&#39;                    |  78.96 ns | 0.868 ns | 0.812 ns |  3.78 |    0.05 |      - |         - |          NA |
| &#39;Multi-user scenario (without named keys)&#39;    | 144.62 ns | 1.597 ns | 1.494 ns |  6.92 |    0.09 | 0.0229 |      72 B |          NA |
| &#39;Multi-user scenario (with named keys)&#39;       | 177.74 ns | 1.519 ns | 1.421 ns |  8.50 |    0.10 |      - |         - |          NA |
| &#39;Configuration scenario (without named keys)&#39; | 146.31 ns | 1.658 ns | 1.551 ns |  7.00 |    0.09 | 0.0229 |      72 B |          NA |
| &#39;Configuration scenario (with named keys)&#39;    | 182.33 ns | 1.689 ns | 1.497 ns |  8.72 |    0.10 | 0.0229 |      72 B |          NA |
| &#39;Short key (3 chars)&#39;                         |  50.65 ns | 0.355 ns | 0.332 ns |  2.42 |    0.03 |      - |         - |          NA |
| &#39;Medium key (20 chars)&#39;                       |  72.66 ns | 0.783 ns | 0.732 ns |  3.47 |    0.05 |      - |         - |          NA |
| &#39;Long key (50 chars)&#39;                         | 108.86 ns | 0.932 ns | 0.872 ns |  5.21 |    0.06 |      - |         - |          NA |
