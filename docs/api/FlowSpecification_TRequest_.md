## FlowSpecification\<TRequest\> Class

Optional abstract base class for implementing flow specifications\.
Provides the [Continue\(\)](FlowSpecification_TRequest_.Continue().md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Continue\(\)'), [Fail\(string, int\)](FlowSpecification_TRequest_.Fail.589T1QQR653WCCGCXHV5Y2NM5.md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Fail\(string, int\)'), and [Stop\(object, int\)](FlowSpecification_TRequest_.Stop.MHKQRHPAVHV9WNGC3IEG2VOJ7.md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Stop\(object, int\)') helpers
to avoid verbose `ValueTask.FromResult<FlowInterrupt<object?>?>(...)` boilerplate\.

```csharp
public abstract class FlowSpecification<TRequest> : FlowT.Contracts.IFlowSpecification<TRequest>
```
#### Type parameters

<a name='FlowT.Abstractions.FlowSpecification_TRequest_.TRequest'></a>

`TRequest`

The type of request this specification validates\.

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowSpecification\<TRequest\>

Implements [FlowT\.Contracts\.IFlowSpecification&lt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>')[TRequest](FlowSpecification_TRequest_.md#FlowT.Abstractions.FlowSpecification_TRequest_.TRequest 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.TRequest')[&gt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>')

### Remarks

Implementing [IFlowSpecification&lt;TRequest&gt;](IFlowSpecification_TRequest_.md 'FlowT\.Contracts\.IFlowSpecification\<TRequest\>') directly is always valid.
Inherit from this class only when the helper methods simplify your implementation.

[Continue\(\)](FlowSpecification_TRequest_.Continue().md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Continue\(\)') is backed by a `static readonly` cached field — calling it never allocates.\<example\>
  \<code\>\<\!\[CDATA\[
             public class ValidateEmailSpec : FlowSpecification\<CreateUserRequest\>
             \{
                 public override ValueTask\<FlowInterrupt\<object?\>?\> CheckAsync\(
                     CreateUserRequest request, FlowContext context\)
                 \{
                     if \(\!IsValidEmail\(request\.Email\)\)
                         return Fail\("Email format is invalid", 400\);
            
                     return Continue\(\);
                 \}
             \}
            
             public class CheckCacheSpec : FlowSpecification\<GetUserRequest\>
             \{
                 public override ValueTask\<FlowInterrupt\<object?\>?\> CheckAsync\(
                     GetUserRequest request, FlowContext context\)
                 \{
                     if \(context\.TryGet\<UserResponse\>\(out var cached\)\)
                         return Stop\(cached, 200\);
            
                     return Continue\(\);
                 \}
             \}
             \]\]\>\</code\>
\</example\>

| Methods | |
| :--- | :--- |
| [CheckAsync\(TRequest, FlowContext\)](FlowSpecification_TRequest_.CheckAsync.QTHZIQ37Y78AUXMZKAI84M628.md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.CheckAsync\(TRequest, FlowT\.FlowContext\)') | Checks whether the request satisfies this specification\. |
| [Continue\(\)](FlowSpecification_TRequest_.Continue().md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Continue\(\)') | Returns a completed [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') signalling that validation passed and the pipeline should continue to the next step\. This value is cached — calling this method never allocates\. |
| [Fail\(string, int\)](FlowSpecification_TRequest_.Fail.589T1QQR653WCCGCXHV5Y2NM5.md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Fail\(string, int\)') | Returns a completed [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') wrapping a [Fail\(string, int\)](FlowInterrupt_TResponse_.Fail.TLZDJP7RUR1C0E9B7A1C2RVQ2.md 'FlowT\.FlowInterrupt\<TResponse\>\.Fail\(string, int\)') interrupt, stopping the pipeline with the given error message and HTTP status code\. |
| [Stop\(object, int\)](FlowSpecification_TRequest_.Stop.MHKQRHPAVHV9WNGC3IEG2VOJ7.md 'FlowT\.Abstractions\.FlowSpecification\<TRequest\>\.Stop\(object, int\)') | Returns a completed [System\.Threading\.Tasks\.ValueTask](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask 'System\.Threading\.Tasks\.ValueTask') wrapping a [Stop\(TResponse, int\)](FlowInterrupt_TResponse_.Stop.6744CQMUQJUAPJELPCZ34S1D6.md 'FlowT\.FlowInterrupt\<TResponse\>\.Stop\(TResponse, int\)') interrupt, short\-circuiting the pipeline with a successful early response\. |
