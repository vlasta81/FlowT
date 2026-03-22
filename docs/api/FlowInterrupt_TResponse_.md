## FlowInterrupt\<TResponse\> Struct

Represents an interruption of flow execution, primarily used for capturing validation failures and business rule violations
from specifications without throwing exceptions\. Also supports early returns with successful results\.

```csharp
public readonly struct FlowInterrupt<TResponse>
```
#### Type parameters

<a name='FlowT.FlowInterrupt_TResponse_.TResponse'></a>

`TResponse`

The type of response that can be returned as an early result\.

### Example

```csharp
// Validation failure example
public class ValidateEmailSpec : IFlowSpecification<CreateUserRequest>
{
    public ValueTask<FlowInterrupt<UserResponse>?> CheckAsync(
        CreateUserRequest request, FlowContext context)
    {
        if (!IsValidEmail(request.Email))
        {
            return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(
                FlowInterrupt<UserResponse>.Fail(
                    "Email format is invalid",
                    StatusCodes.Status400BadRequest
                )
            );
        }
        
        return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(null);
    }
}

// Early return example
public class CheckCacheSpec : IFlowSpecification<GetUserRequest>
{
    public ValueTask<FlowInterrupt<UserResponse>?> CheckAsync(
        GetUserRequest request, FlowContext context)
    {
        var cached = context.TryGet<UserResponse>(out var cachedData);
        if (cached)
        {
            return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(
                FlowInterrupt<UserResponse>.Stop(cachedData!, StatusCodes.Status200OK)
            );
        }
        
        return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(null);
    }
}
```

### Remarks

[FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') provides a type-safe, exception-free mechanism for handling errors in flow pipelines.
            It is returned by [IFlowSpecification&lt;TRequest&gt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>') implementations to signal:
- <strong>Validation failures</strong> - Invalid input data (e.g., bad email format, missing required fields)
- <strong>Business rule violations</strong> - Domain constraints (e.g., duplicate email, insufficient permissions)
- <strong>Early successful returns</strong> - Short-circuit pipeline with a valid result (e.g., cached data)

<strong>Benefits over exceptions:</strong>
- No performance overhead of try-catch blocks
- Explicit error flow in method signatures
- Type-safe with compiler guarantees
- Supports HTTP status codes (400, 401, 409, etc.)

| Properties | |
| :--- | :--- |
| [IsEarlyReturn](FlowInterrupt_TResponse_.IsEarlyReturn.md 'FlowT\.FlowInterrupt\<TResponse\>\.IsEarlyReturn') | Gets a value indicating whether this interrupt represents an early return \(has a response but no error\)\. |
| [IsFailure](FlowInterrupt_TResponse_.IsFailure.md 'FlowT\.FlowInterrupt\<TResponse\>\.IsFailure') | Gets a value indicating whether this interrupt represents a failure \(has an error message\)\. |
| [Message](FlowInterrupt_TResponse_.Message.md 'FlowT\.FlowInterrupt\<TResponse\>\.Message') | Gets the error message for a failure scenario\. This is `null` if the interrupt represents an early return\. |
| [Response](FlowInterrupt_TResponse_.Response.md 'FlowT\.FlowInterrupt\<TResponse\>\.Response') | Gets the response value for an early return scenario\. This is `null` if the interrupt represents a failure\. |
| [StatusCode](FlowInterrupt_TResponse_.StatusCode.md 'FlowT\.FlowInterrupt\<TResponse\>\.StatusCode') | Gets the HTTP\-like status code for this interrupt\. Default is 400 for failures and 200 for early returns\. |

| Methods | |
| :--- | :--- |
| [Fail\(string, int\)](FlowInterrupt_TResponse_.Fail.TLZDJP7RUR1C0E9B7A1C2RVQ2.md 'FlowT\.FlowInterrupt\<TResponse\>\.Fail\(string, int\)') | Creates a failure interrupt representing a validation error or business rule violation\. This is the primary method for capturing errors from specifications without throwing exceptions\. |
| [Stop\(TResponse, int\)](FlowInterrupt_TResponse_.Stop.6744CQMUQJUAPJELPCZ34S1D6.md 'FlowT\.FlowInterrupt\<TResponse\>\.Stop\(TResponse, int\)') | Creates an early return interrupt with a successful response value\. This allows specifications or policies to short\-circuit the pipeline while returning a valid result, useful for caching, pre\-computed results, or conditional logic\. |
