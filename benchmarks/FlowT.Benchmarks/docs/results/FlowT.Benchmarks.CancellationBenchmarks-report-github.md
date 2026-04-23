```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                                        | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------------------------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| &#39;ThrowIfCancellationRequested - CancellationToken.None&#39;       |   0.2608 ns | 0.0106 ns | 0.0099 ns |   1.00 |    0.05 |      - |         - |          NA |
| &#39;ThrowIfCancellationRequested - live unsignalled token&#39;       |   0.3769 ns | 0.0200 ns | 0.0187 ns |   1.45 |    0.09 |      - |         - |          NA |
| &#39;Pipeline with CT check in handler - CancellationToken.None&#39;  |   9.2186 ns | 0.1203 ns | 0.1066 ns |  35.40 |    1.34 | 0.0076 |      24 B |          NA |
| &#39;Pipeline with CT check in handler - live unsignalled token&#39;  |   9.5263 ns | 0.1185 ns | 0.1050 ns |  36.58 |    1.38 | 0.0076 |      24 B |          NA |
| &#39;ExecuteAsync(IServiceProvider, CT) - live unsignalled token&#39; | 121.0698 ns | 1.4364 ns | 1.3436 ns | 464.87 |   17.57 | 0.0789 |     248 B |          NA |
