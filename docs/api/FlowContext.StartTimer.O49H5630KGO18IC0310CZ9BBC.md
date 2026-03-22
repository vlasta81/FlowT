## FlowContext\.StartTimer\(string\) Method

Starts a high\-precision timer for measuring elapsed time of an operation\.
Returns a disposable that records the elapsed time when disposed\.
Timers are stored in the context and can be retrieved for performance analysis\.

```csharp
public FlowT.FlowContext.TimerDisposable StartTimer(string key);
```
#### Parameters

<a name='FlowT.FlowContext.StartTimer(string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

A unique string key to identify this timer\.

#### Returns
[TimerDisposable](FlowContext.TimerDisposable.md 'FlowT\.FlowContext\.TimerDisposable')  
A [TimerDisposable](FlowContext.TimerDisposable.md 'FlowT\.FlowContext\.TimerDisposable') that records elapsed time when disposed\.