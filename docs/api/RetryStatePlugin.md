## RetryStatePlugin Class

Default implementation of [IRetryStatePlugin](IRetryStatePlugin.md 'FlowT\.Plugins\.IRetryStatePlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class RetryStatePlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IRetryStatePlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; RetryStatePlugin

Implements [IRetryStatePlugin](IRetryStatePlugin.md 'FlowT\.Plugins\.IRetryStatePlugin')

| Properties | |
| :--- | :--- |
| [AttemptNumber](RetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.RetryStatePlugin\.AttemptNumber') | Gets the number of attempts registered so far\. Starts at zero before any call to [RegisterAttempt\(\)](IRetryStatePlugin.RegisterAttempt().md 'FlowT\.Plugins\.IRetryStatePlugin\.RegisterAttempt\(\)')\. |

| Methods | |
| :--- | :--- |
| [RegisterAttempt\(\)](RetryStatePlugin.RegisterAttempt().md 'FlowT\.Plugins\.RetryStatePlugin\.RegisterAttempt\(\)') | Atomically registers a new attempt, incrementing [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') by one\. Thread\-safe via [System\.Threading\.Interlocked\.Increment\(System\.Int32@\)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.increment#system-threading-interlocked-increment(system-int32@) 'System\.Threading\.Interlocked\.Increment\(System\.Int32@\)')\. |
| [ShouldRetry\(int\)](RetryStatePlugin.ShouldRetry.MD22918TZKO7C11L1AJFQ9XY3.md 'FlowT\.Plugins\.RetryStatePlugin\.ShouldRetry\(int\)') | Determines whether another retry is allowed given the maximum number of attempts\. Returns `true` when [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') is less than [maxAttempts](RetryStatePlugin.ShouldRetry.MD22918TZKO7C11L1AJFQ9XY3.md#FlowT.Plugins.RetryStatePlugin.ShouldRetry(int).maxAttempts 'FlowT\.Plugins\.RetryStatePlugin\.ShouldRetry\(int\)\.maxAttempts')\. |
