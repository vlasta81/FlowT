## FlowContext\.TimerDisposable\.Dispose\(\) Method

Calculates the elapsed time and stores it in the context's timer dictionary under the timer's key\.
Elapsed time is stored as [System\.Diagnostics\.Stopwatch](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch 'System\.Diagnostics\.Stopwatch') ticks \(not [System\.TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan 'System\.TimeSpan')\) for maximum precision\.
Convert to [System\.TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan 'System\.TimeSpan') via [System\.Diagnostics\.Stopwatch\.GetElapsedTime\(System\.Int64\)](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch.getelapsedtime#system-diagnostics-stopwatch-getelapsedtime(system-int64) 'System\.Diagnostics\.Stopwatch\.GetElapsedTime\(System\.Int64\)') when needed\.

```csharp
public void Dispose();
```