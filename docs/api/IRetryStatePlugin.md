## IRetryStatePlugin Interface

Built\-in plugin that tracks retry attempt count across all pipeline stages within a single flow execution\.
Because the plugin is PerFlow, the counter is shared between the retry policy and any other stage that inspects it\.

```csharp
public interface IRetryStatePlugin
```

Derived  
&#8627; [RetryStatePlugin](RetryStatePlugin.md 'FlowT\.Plugins\.RetryStatePlugin')

### Remarks
Register via `services.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>()`\.

Typical usage in a retry policy:

```csharp
public class RetryPolicy : FlowPolicy<TRequest, TResponse>
{
    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        var retry = context.Plugin<IRetryStatePlugin>();
        while (retry.ShouldRetry(maxAttempts: 3))
        {
            retry.RegisterAttempt();
            try   { return await Next!.HandleAsync(request, context); }
            catch { if (!retry.ShouldRetry(3)) throw; }
        }
        return await Next!.HandleAsync(request, context);
    }
}
```

| Properties | |
| :--- | :--- |
| [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') | Gets the number of attempts registered so far\. Starts at zero before any call to [RegisterAttempt\(\)](IRetryStatePlugin.RegisterAttempt().md 'FlowT\.Plugins\.IRetryStatePlugin\.RegisterAttempt\(\)')\. |

| Methods | |
| :--- | :--- |
| [RegisterAttempt\(\)](IRetryStatePlugin.RegisterAttempt().md 'FlowT\.Plugins\.IRetryStatePlugin\.RegisterAttempt\(\)') | Atomically registers a new attempt, incrementing [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') by one\. Thread\-safe via [System\.Threading\.Interlocked\.Increment\(System\.Int32@\)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.increment#system-threading-interlocked-increment(system-int32@) 'System\.Threading\.Interlocked\.Increment\(System\.Int32@\)')\. |
| [ShouldRetry\(int\)](IRetryStatePlugin.ShouldRetry.2TH4BANNUE8NHZ0RIT7QBT8O.md 'FlowT\.Plugins\.IRetryStatePlugin\.ShouldRetry\(int\)') | Determines whether another retry is allowed given the maximum number of attempts\. Returns `true` when [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') is less than [maxAttempts](IRetryStatePlugin.ShouldRetry.2TH4BANNUE8NHZ0RIT7QBT8O.md#FlowT.Plugins.IRetryStatePlugin.ShouldRetry(int).maxAttempts 'FlowT\.Plugins\.IRetryStatePlugin\.ShouldRetry\(int\)\.maxAttempts')\. |
