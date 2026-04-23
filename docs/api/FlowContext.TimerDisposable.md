## FlowContext\.TimerDisposable Struct

Disposable struct that measures and records elapsed time when disposed\.
Returned by [StartTimer\(string\)](FlowContext.StartTimer.O49H5630KGO18IC0310CZ9BBC.md 'FlowT\.FlowContext\.StartTimer\(string\)')\.
Uses high\-precision [System\.Diagnostics\.Stopwatch\.GetTimestamp](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.gettimestamp 'System\.Diagnostics\.Stopwatch\.GetTimestamp') for accurate measurements\.

```csharp
public readonly struct FlowContext.TimerDisposable : System.IDisposable
```

Implements [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable')

| Methods | |
| :--- | :--- |
| [Dispose\(\)](FlowContext.TimerDisposable.Dispose().md 'FlowT\.FlowContext\.TimerDisposable\.Dispose\(\)') | Calculates the elapsed time and stores it in the context's timer dictionary under the timer's key\. Elapsed time is stored as [System\.Diagnostics\.Stopwatch](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch 'System\.Diagnostics\.Stopwatch') ticks \(not [System\.TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan 'System\.TimeSpan')\) for maximum precision\. Convert to [System\.TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan 'System\.TimeSpan') via [System\.Diagnostics\.Stopwatch\.GetElapsedTime\(System\.Int64\)](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.getelapsedtime#system-diagnostics-stopwatch-getelapsedtime(system-int64) 'System\.Diagnostics\.Stopwatch\.GetElapsedTime\(System\.Int64\)') when needed\. |
