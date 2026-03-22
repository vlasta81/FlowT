## IRetryStatePlugin\.RegisterAttempt\(\) Method

Atomically registers a new attempt, incrementing [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') by one\.
Thread\-safe via [System\.Threading\.Interlocked\.Increment\(System\.Int32@\)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.increment#system-threading-interlocked-increment(system-int32@) 'System\.Threading\.Interlocked\.Increment\(System\.Int32@\)')\.

```csharp
void RegisterAttempt();
```