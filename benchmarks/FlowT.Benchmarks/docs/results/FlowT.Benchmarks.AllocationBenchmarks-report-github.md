```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v3


```
| Method                                        | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------------------------------------- |-----------:|----------:|----------:|-------:|----------:|
| &#39;Single execution&#39;                            |  21.145 ns | 0.4703 ns | 0.4830 ns | 0.0306 |      96 B |
| &#39;10 sequential executions&#39;                    |   5.933 ns | 0.0796 ns | 0.0705 ns | 0.0076 |      24 B |
| &#39;100 sequential executions&#39;                   |   5.159 ns | 0.1026 ns | 0.1404 ns | 0.0076 |      24 B |
| &#39;10 parallel executions&#39;                      | 145.597 ns | 2.2990 ns | 2.0380 ns | 0.1146 |     360 B |
| &#39;Context creation only&#39;                       | 112.087 ns | 1.9753 ns | 2.3514 ns | 0.0714 |     224 B |
| &#39;Context with 5 values&#39;                       | 330.695 ns | 6.6699 ns | 7.9401 ns | 0.2499 |     784 B |
| &#39;Context creation + StartTimer (first timer)&#39; | 193.455 ns | 3.9060 ns | 3.6537 ns | 0.1402 |     440 B |
| &#39;Fresh context per call, execute flow&#39;        | 141.142 ns | 2.1103 ns | 1.9739 ns | 0.1018 |     320 B |
