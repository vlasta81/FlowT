## IPerformancePlugin\.Measure\(string\) Method

Starts timing the named operation\. The elapsed time is recorded when the returned
[System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable') is disposed \(i\.e\., at the end of the `using` block\)\.

```csharp
System.IDisposable Measure(string name);
```
#### Parameters

<a name='FlowT.Plugins.IPerformancePlugin.Measure(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

A unique label for the operation being timed\.

#### Returns
[System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable')  
A disposable that stops the timer and records the elapsed time when disposed\.