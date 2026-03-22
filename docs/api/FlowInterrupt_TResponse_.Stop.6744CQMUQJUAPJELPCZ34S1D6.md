## FlowInterrupt\<TResponse\>\.Stop\(TResponse, int\) Method

Creates an early return interrupt with a successful response value\.
This allows specifications or policies to short\-circuit the pipeline while returning a valid result,
useful for caching, pre\-computed results, or conditional logic\.

```csharp
public static FlowT.FlowInterrupt<TResponse> Stop(TResponse earlyReturn, int statusCode=200);
```
#### Parameters

<a name='FlowT.FlowInterrupt_TResponse_.Stop(TResponse,int).earlyReturn'></a>

`earlyReturn` [TResponse](FlowInterrupt_TResponse_.md#FlowT.FlowInterrupt_TResponse_.TResponse 'FlowT\.FlowInterrupt\<TResponse\>\.TResponse')

The response value to return immediately, bypassing remaining pipeline steps\.

<a name='FlowT.FlowInterrupt_TResponse_.Stop(TResponse,int).statusCode'></a>

`statusCode` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The HTTP status code for this early return\. Common values:
            
- 200 - OK (successful response)
- 201 - Created (resource created)
- 204 - No Content (successful with no response body)
- 304 - Not Modified (cached response)

            Default is 200 \(OK\)\.

#### Returns
[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[TResponse](FlowInterrupt_TResponse_.md#FlowT.FlowInterrupt_TResponse_.TResponse 'FlowT\.FlowInterrupt\<TResponse\>\.TResponse')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')  
A [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') representing an early successful return\.

### Remarks
Use [Stop\(TResponse, int\)](FlowInterrupt_TResponse_.Stop.6744CQMUQJUAPJELPCZ34S1D6.md 'FlowT\.FlowInterrupt\<TResponse\>\.Stop\(TResponse, int\)') for optimization scenarios like returning cached data:

```csharp
if (context.TryGet<UserResponse>(out var cached))
{
    return FlowInterrupt<UserResponse>.Stop(
        cached,
        StatusCodes.Status200OK
    );
}
```