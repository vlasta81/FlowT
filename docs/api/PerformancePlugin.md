## PerformancePlugin Class

Default implementation of [IPerformancePlugin](IPerformancePlugin.md 'FlowT\.Plugins\.IPerformancePlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class PerformancePlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IPerformancePlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; PerformancePlugin

Implements [IPerformancePlugin](IPerformancePlugin.md 'FlowT\.Plugins\.IPerformancePlugin')

| Properties | |
| :--- | :--- |
| [Elapsed](PerformancePlugin.Elapsed.md 'FlowT\.Plugins\.PerformancePlugin\.Elapsed') | Gets the elapsed times for all completed measurements, keyed by the name passed to [Measure\(string\)](IPerformancePlugin.Measure.W43THU3IS803EZSGSK4BM774F.md 'FlowT\.Plugins\.IPerformancePlugin\.Measure\(string\)')\. When the same name is used more than once, the last measurement overwrites the previous value\. |

| Methods | |
| :--- | :--- |
| [Measure\(string\)](PerformancePlugin.Measure.NAOATR8OHSI8XP6NLYV5DLVI8.md 'FlowT\.Plugins\.PerformancePlugin\.Measure\(string\)') | Starts timing the named operation\. The elapsed time is recorded when the returned [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable') is disposed \(i\.e\., at the end of the `using` block\)\. |
