## IPerformancePlugin\.Elapsed Property

Gets the elapsed times for all completed measurements, keyed by the name passed to [Measure\(string\)](IPerformancePlugin.Measure.W43THU3IS803EZSGSK4BM774F.md 'FlowT\.Plugins\.IPerformancePlugin\.Measure\(string\)')\.
When the same name is used more than once, the last measurement overwrites the previous value\.

```csharp
System.Collections.Generic.IReadOnlyDictionary<string,System.TimeSpan> Elapsed { get; }
```

#### Property Value
[System\.Collections\.Generic\.IReadOnlyDictionary&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[,](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')[System\.TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan 'System\.TimeSpan')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2 'System\.Collections\.Generic\.IReadOnlyDictionary\`2')