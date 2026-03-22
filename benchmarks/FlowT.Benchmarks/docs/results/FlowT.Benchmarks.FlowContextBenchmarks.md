```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
Intel Core i5-7600K CPU 3.80GHz (Kaby Lake), 1 CPU, 4 logical and 4 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                                | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------------------------------- |----------:|---------:|---------:|-------:|----------:|
| &#39;Set&lt;string&gt; single value&#39;            |  21.20 ns | 0.067 ns | 0.063 ns |      - |         - |
| &#39;Set&lt;int&gt; single value&#39;               |  25.83 ns | 0.108 ns | 0.101 ns | 0.0076 |      24 B |
| &#39;Set&lt;ComplexObject&gt; single value&#39;     |  21.48 ns | 0.076 ns | 0.071 ns |      - |         - |
| &#39;Set + TryGet&lt;string&gt; success&#39;        |  28.53 ns | 0.110 ns | 0.103 ns |      - |         - |
| &#39;Set + TryGet&lt;string&gt; (returns bool)&#39; |  28.31 ns | 0.072 ns | 0.067 ns |      - |         - |
| &#39;GetOrAdd&lt;List&lt;string&gt;&gt; (cold)&#39;       | 151.79 ns | 1.041 ns | 0.923 ns | 0.0994 |     312 B |
| &#39;GetOrAdd&lt;List&lt;string&gt;&gt; (warm)&#39;       |  20.30 ns | 0.069 ns | 0.065 ns |      - |         - |
| &#39;Multiple types interleaved&#39;          | 131.11 ns | 0.547 ns | 0.512 ns | 0.0076 |      24 B |
| &#39;Push/Pop scope&#39;                      |  88.29 ns | 0.237 ns | 0.210 ns |      - |         - |
| &#39;10 different types Set+TryGet&#39;       |  26.82 ns | 0.064 ns | 0.060 ns | 0.0047 |      15 B |
| &#39;Set&lt;string&gt; with named key&#39;          |  25.86 ns | 0.063 ns | 0.059 ns |      - |         - |
| &#39;Set + TryGet with named key&#39;         |  42.54 ns | 0.186 ns | 0.174 ns |      - |         - |
| &#39;Multiple named keys same type&#39;       | 138.51 ns | 0.435 ns | 0.407 ns |      - |         - |
| &#39;GetOrAdd with named key (warm)&#39;      |  25.37 ns | 0.049 ns | 0.044 ns |      - |         - |
| &#39;Push/Pop with named key&#39;             | 110.84 ns | 0.323 ns | 0.302 ns |      - |         - |
| &#39;3 caches with named keys&#39;            |  88.45 ns | 0.239 ns | 0.212 ns |      - |         - |
| &#39;Default vs Named key comparison&#39;     |  17.03 ns | 0.053 ns | 0.050 ns |      - |         - |
