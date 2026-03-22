## FlowContext\.TimerDisposable\.Dispose\(\) Method

Calculates the elapsed time and stores it in the context under the timer's key\.
Elapsed time is stored as [System\.Diagnostics\.Stopwatch](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch 'System\.Diagnostics\.Stopwatch') ticks \(not TimeSpan\) for maximum precision\.

```csharp
public void Dispose();
```