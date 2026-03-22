## RetryStatePlugin\.ShouldRetry\(int\) Method

Determines whether another retry is allowed given the maximum number of attempts\.
Returns `true` when [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') is less than [maxAttempts](RetryStatePlugin.ShouldRetry.MD22918TZKO7C11L1AJFQ9XY3.md#FlowT.Plugins.RetryStatePlugin.ShouldRetry(int).maxAttempts 'FlowT\.Plugins\.RetryStatePlugin\.ShouldRetry\(int\)\.maxAttempts')\.

```csharp
public bool ShouldRetry(int maxAttempts);
```
#### Parameters

<a name='FlowT.Plugins.RetryStatePlugin.ShouldRetry(int).maxAttempts'></a>

`maxAttempts` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Maximum total attempts permitted\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')