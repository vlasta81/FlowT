```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                      | Mean       | Error     | StdDev    | Gen0   | Allocated |
|---------------------------- |-----------:|----------:|----------:|-------:|----------:|
| &#39;Single execution&#39;          |  21.693 ns | 0.2496 ns | 0.2213 ns | 0.0306 |      96 B |
| &#39;10 sequential executions&#39;  |   5.945 ns | 0.0581 ns | 0.0485 ns | 0.0076 |      24 B |
| &#39;100 sequential executions&#39; |   5.384 ns | 0.0659 ns | 0.0617 ns | 0.0076 |      24 B |
| &#39;10 parallel executions&#39;    | 150.201 ns | 1.3303 ns | 1.2444 ns | 0.1070 |     336 B |
| &#39;Context creation only&#39;     | 108.137 ns | 0.6073 ns | 0.5680 ns | 0.0637 |     200 B |
| &#39;Context with 5 values&#39;     | 332.754 ns | 2.0671 ns | 1.8324 ns | 0.2422 |     760 B |
