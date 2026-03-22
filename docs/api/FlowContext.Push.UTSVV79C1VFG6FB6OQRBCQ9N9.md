## FlowContext\.Push\<T\>\(T, string\) Method

Temporarily replaces a value in the context's shared state\.
Returns a disposable that restores the previous value \(or removes the entry\) when disposed\.
Useful for scoped overrides in nested flows or policies\.

```csharp
public FlowT.FlowContext.ScopeReverter Push<T>(T value, string? key=null);
```
#### Type parameters

<a name='FlowT.FlowContext.Push_T_(T,string).T'></a>

`T`

The type of value to push\.
#### Parameters

<a name='FlowT.FlowContext.Push_T_(T,string).value'></a>

`value` [T](FlowContext.Push.UTSVV79C1VFG6FB6OQRBCQ9N9.md#FlowT.FlowContext.Push_T_(T,string).T 'FlowT\.FlowContext\.Push\<T\>\(T, string\)\.T')

The value to temporarily store\.

<a name='FlowT.FlowContext.Push_T_(T,string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Optional string key for storing multiple values of the same type\.

#### Returns
[ScopeReverter](FlowContext.ScopeReverter.md 'FlowT\.FlowContext\.ScopeReverter')  
A [ScopeReverter](FlowContext.ScopeReverter.md 'FlowT\.FlowContext\.ScopeReverter') that restores the original state when disposed\.