## IRetryStatePlugin\.ShouldRetry\(int\) Method

Determines whether another retry is allowed given the maximum number of attempts\.
Returns `true` when [AttemptNumber](IRetryStatePlugin.AttemptNumber.md 'FlowT\.Plugins\.IRetryStatePlugin\.AttemptNumber') is less than [maxAttempts](IRetryStatePlugin.ShouldRetry.2TH4BANNUE8NHZ0RIT7QBT8O.md#FlowT.Plugins.IRetryStatePlugin.ShouldRetry(int).maxAttempts 'FlowT\.Plugins\.IRetryStatePlugin\.ShouldRetry\(int\)\.maxAttempts')\.

```csharp
bool ShouldRetry(int maxAttempts);
```
#### Parameters

<a name='FlowT.Plugins.IRetryStatePlugin.ShouldRetry(int).maxAttempts'></a>

`maxAttempts` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Maximum total attempts permitted\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')