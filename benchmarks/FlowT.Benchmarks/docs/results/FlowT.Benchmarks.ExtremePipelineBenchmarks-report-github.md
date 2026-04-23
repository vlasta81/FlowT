```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                                | Mean          | Error      | StdDev     | Ratio     | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------ |--------------:|-----------:|-----------:|----------:|--------:|-------:|----------:|------------:|
| &#39;EXTREME: 10 specs + 10 policies + 10 named keys&#39;     |      1.000 μs |  0.0080 μs |  0.0067 μs |      1.00 |    0.01 | 0.1469 |     464 B |        1.00 |
| &#39;EXTREME: Large payload (10 MB + 10k items)&#39;          |  4,599.671 μs | 89.4210 μs | 95.6795 μs |  4,599.59 |   97.74 |      - |     208 B |        0.45 |
| &#39;EXTREME: 100 concurrent executions&#39;                  | 15,753.334 μs | 53.2152 μs | 49.7775 μs | 15,753.07 |  112.01 |      - |   70648 B |      152.26 |
| &#39;EXTREME: Deep nesting (10 policies + 10 MB payload)&#39; |  4,583.333 μs | 73.6885 μs | 65.3229 μs |  4,583.26 |   69.64 |      - |     304 B |        0.66 |
