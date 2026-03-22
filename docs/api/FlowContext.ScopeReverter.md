## FlowContext\.ScopeReverter Struct

Disposable struct that restores a previous value in the context when disposed\.
Returned by [Push&lt;T&gt;\(T, string\)](FlowContext.Push.UTSVV79C1VFG6FB6OQRBCQ9N9.md 'FlowT\.FlowContext\.Push\<T\>\(T, string\)')\.

```csharp
public readonly struct FlowContext.ScopeReverter : System.IDisposable
```

Implements [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable')

| Methods | |
| :--- | :--- |
| [Dispose\(\)](FlowContext.ScopeReverter.Dispose().md 'FlowT\.FlowContext\.ScopeReverter\.Dispose\(\)') | Restores the previous value \(or removes the entry if there was no previous value\)\. |
