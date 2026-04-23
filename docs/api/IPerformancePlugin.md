## IPerformancePlugin Interface

Built\-in plugin that measures and accumulates elapsed time for named operations within a flow execution\.
Use [Measure\(string\)](IPerformancePlugin.Measure.W43THU3IS803EZSGSK4BM774F.md 'FlowT\.Plugins\.IPerformancePlugin\.Measure\(string\)') in a `using` block to time a section of code; results are accessible
via [Elapsed](IPerformancePlugin.Elapsed.md 'FlowT\.Plugins\.IPerformancePlugin\.Elapsed') after the measurement completes\.

```csharp
public interface IPerformancePlugin
```

Derived  
&#8627; [PerformancePlugin](PerformancePlugin.md 'FlowT\.Plugins\.PerformancePlugin')

### Remarks
Register via `services.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>()`\.

Usage:

```csharp
var perf = context.Plugin<IPerformancePlugin>();

using (perf.Measure("db-query"))
{
    orders = await dbContext.Orders.ToListAsync();
}

logger.LogInformation("DB query took {Elapsed}", perf.Elapsed["db-query"]);
```

| Properties | |
| :--- | :--- |
| [Elapsed](IPerformancePlugin.Elapsed.md 'FlowT\.Plugins\.IPerformancePlugin\.Elapsed') | Gets the elapsed times for all completed measurements, keyed by the name passed to [Measure\(string\)](IPerformancePlugin.Measure.W43THU3IS803EZSGSK4BM774F.md 'FlowT\.Plugins\.IPerformancePlugin\.Measure\(string\)')\. When the same name is used more than once, the last measurement overwrites the previous value\. |

| Methods | |
| :--- | :--- |
| [Measure\(string\)](IPerformancePlugin.Measure.W43THU3IS803EZSGSK4BM774F.md 'FlowT\.Plugins\.IPerformancePlugin\.Measure\(string\)') | Starts timing the named operation\. The elapsed time is recorded when the returned [System\.IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable 'System\.IDisposable') is disposed \(i\.e\., at the end of the `using` block\)\. |
