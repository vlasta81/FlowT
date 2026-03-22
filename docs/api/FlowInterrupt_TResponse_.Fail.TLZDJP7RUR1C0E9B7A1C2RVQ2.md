## FlowInterrupt\<TResponse\>\.Fail\(string, int\) Method

Creates a failure interrupt representing a validation error or business rule violation\.
This is the primary method for capturing errors from specifications without throwing exceptions\.

```csharp
public static FlowT.FlowInterrupt<TResponse> Fail(string message, int statusCode=400);
```
#### Parameters

<a name='FlowT.FlowInterrupt_TResponse_.Fail(string,int).message'></a>

`message` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The error message describing why the flow was interrupted \(e\.g\., "Email format is invalid", "User already exists"\)\.

<a name='FlowT.FlowInterrupt_TResponse_.Fail(string,int).statusCode'></a>

`statusCode` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The HTTP status code for this failure\. Common values:
            
- 400 - Bad Request (validation failure)
- 401 - Unauthorized (authentication required)
- 403 - Forbidden (insufficient permissions)
- 404 - Not Found (resource doesn't exist)
- 409 - Conflict (duplicate data, business rule violation)
- 422 - Unprocessable Entity (semantic validation error)

            Default is 400 \(Bad Request\)\.

#### Returns
[FlowT\.FlowInterrupt&lt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')[TResponse](FlowInterrupt_TResponse_.md#FlowT.FlowInterrupt_TResponse_.TResponse 'FlowT\.FlowInterrupt\<TResponse\>\.TResponse')[&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>')  
A [FlowInterrupt&lt;TResponse&gt;](FlowInterrupt_TResponse_.md 'FlowT\.FlowInterrupt\<TResponse\>') representing a failure that will stop pipeline execution\.

### Remarks
Use [Fail\(string, int\)](FlowInterrupt_TResponse_.Fail.TLZDJP7RUR1C0E9B7A1C2RVQ2.md 'FlowT\.FlowInterrupt\<TResponse\>\.Fail\(string, int\)') in specifications to return validation errors without exceptions:

```csharp
if (!IsValidEmail(request.Email))
{
    return FlowInterrupt<UserResponse>.Fail(
        "Email format is invalid",
        StatusCodes.Status400BadRequest
    );
}
```