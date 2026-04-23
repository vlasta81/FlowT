## FlowSpecification\<TRequest\>\.Fail\(string, int\) Method

Returns a completed [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') wrapping a [Fail\(string, int\)](FlowInterrupt_TResponse_.Fail.TLZDJP7RUR1C0E9B7A1C2RVQ2.md 'FlowT\.FlowInterrupt\<TResponse\>\.Fail\(string, int\)') interrupt,
stopping the pipeline with the given error message and HTTP status code\.

```csharp
protected static System.Threading.Tasks.ValueTask<System.Nullable<FlowT.FlowInterrupt<object?>>> Fail(string message, int statusCode=400);
```
#### Parameters

<a name='FlowT.Abstractions.FlowSpecification_TRequest_.Fail(string,int).message'></a>

`message` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The error message describing the validation failure \(e\.g\. `"Email format is invalid"`\)\.

<a name='FlowT.Abstractions.FlowSpecification_TRequest_.Fail(string,int).statusCode'></a>

`statusCode` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The HTTP status code for this failure\. Common values:
- 400 — Bad Request (validation failure)
- 401 — Unauthorized
- 403 — Forbidden
- 404 — Not Found
- 409 — Conflict (business rule violation)

Default is 400\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[System\.Nullable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.nullable-1 'System\.Nullable\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')