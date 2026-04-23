## FlowSpecification\<TRequest\>\.Stop\(object, int\) Method

Returns a completed [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') wrapping a [Stop\(TResponse, int\)](FlowInterrupt_TResponse_.Stop.6744CQMUQJUAPJELPCZ34S1D6.md 'FlowT\.FlowInterrupt\<TResponse\>\.Stop\(TResponse, int\)') interrupt,
short\-circuiting the pipeline with a successful early response\.

```csharp
protected static System.Threading.Tasks.ValueTask<System.Nullable<FlowT.FlowInterrupt<object?>>> Stop(object? earlyReturn, int statusCode=200);
```
#### Parameters

<a name='FlowT.Abstractions.FlowSpecification_TRequest_.Stop(object,int).earlyReturn'></a>

`earlyReturn` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The response value to return immediately, bypassing remaining pipeline steps\.

<a name='FlowT.Abstractions.FlowSpecification_TRequest_.Stop(object,int).statusCode'></a>

`statusCode` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The HTTP status code for this early return\. Common values:
- 200 — OK
- 201 — Created
- 204 — No Content
- 304 — Not Modified (cached response)

Default is 200\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')