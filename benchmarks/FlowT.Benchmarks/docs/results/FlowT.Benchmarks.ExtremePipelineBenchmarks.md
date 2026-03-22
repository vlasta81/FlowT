```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                                                | Mean            | Error        | StdDev       | Ratio     | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------------------------------ |----------------:|-------------:|-------------:|----------:|--------:|--------:|----------:|------------:|
| &#39;EXTREME: 10 specs + 10 policies + 10 named keys&#39;     |        958.7 ns |      6.73 ns |      5.62 ns |      1.00 |    0.01 |  0.1469 |     464 B |        1.00 |
| &#39;EXTREME: Large payload (10 MB + 10k items)&#39;          |  4,582,497.4 ns | 26,418.67 ns | 23,419.46 ns |  4,780.28 |   35.79 |       - |     208 B |        0.45 |
| &#39;EXTREME: 100 concurrent executions&#39;                  | 15,523,419.8 ns |  7,508.97 ns |  6,656.51 ns | 16,193.40 |   91.37 | 15.6250 |   68248 B |      147.09 |
| &#39;EXTREME: Deep nesting (10 policies + 10 MB payload)&#39; |  4,587,174.4 ns | 20,677.26 ns | 18,329.86 ns |  4,785.15 |   32.66 |       - |     304 B |        0.66 |
