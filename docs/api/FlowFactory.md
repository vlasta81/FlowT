## FlowFactory Class

Factory class responsible for creating and initializing flow instances from flow definitions\.

```csharp
public static class FlowFactory
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowFactory

### Remarks

This factory is used internally by the dependency injection system to convert [FlowDefinition&lt;TRequest,TResponse&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')
instances into [IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>') implementations. The factory ensures that the flow pipeline
is properly initialized with all specifications, policies, and handlers before the flow is used.

The initialization is performed lazily and uses double-check locking to ensure thread-safety while maintaining
high performance. Once initialized, the pipeline is cached and reused across all executions.

| Methods | |
| :--- | :--- |
| [Create&lt;TRequest,TResponse&gt;\(IServiceProvider, FlowDefinition&lt;TRequest,TResponse&gt;\)](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)') | Creates and initializes a flow instance from a flow definition\. |
