## FlowFactory\.Create\<TRequest,TResponse\>\(IServiceProvider, FlowDefinition\<TRequest,TResponse\>\) Method

Creates and initializes a flow instance from a flow definition\.

```csharp
public static FlowT.Contracts.IFlow<TRequest,TResponse> Create<TRequest,TResponse>(System.IServiceProvider serviceProvider, FlowT.Abstractions.FlowDefinition<TRequest,TResponse> definition)
    where TRequest : notnull
    where TResponse : notnull;
```
#### Type parameters

<a name='FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TRequest'></a>

`TRequest`

The type of request handled by the flow\. Must be a non\-null reference type\.

<a name='FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TResponse'></a>

`TResponse`

The type of response returned by the flow\. Must be a non\-null reference type\.
#### Parameters

<a name='FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).serviceProvider'></a>

`serviceProvider` [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider')

The service provider used to resolve dependencies during pipeline initialization\.
This is typically the scoped or root service provider from the DI container\.

<a name='FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).definition'></a>

`definition` [FlowT\.Abstractions\.FlowDefinition&lt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')[TRequest](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md#FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TRequest 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)\.TRequest')[,](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')[TResponse](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md#FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TResponse 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)\.TResponse')[&gt;](FlowDefinition_TRequest,TResponse_.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>')

The flow definition to initialize\. This contains the pipeline configuration \(specifications, policies, handler\)
defined in the [Configure\(IFlowBuilder&lt;TRequest,TResponse&gt;\)](FlowDefinition_TRequest,TResponse_.Configure.G5IBOHYAF75GAD6HUZKJ4HLW1.md 'FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\.Configure\(FlowT\.Contracts\.IFlowBuilder\<TRequest,TResponse\>\)') method\.

#### Returns
[FlowT\.Contracts\.IFlow&lt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')[TRequest](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md#FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TRequest 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)\.TRequest')[,](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')[TResponse](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md#FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).TResponse 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)\.TResponse')[&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')  
An initialized [IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>') instance ready for execution\.
The returned instance is the same as the input [definition](FlowFactory.Create.U2EH8NIBQQNREET0UO8WC21Z3.md#FlowT.FlowFactory.Create_TRequest,TResponse_(System.IServiceProvider,FlowT.Abstractions.FlowDefinition_TRequest,TResponse_).definition 'FlowT\.FlowFactory\.Create\<TRequest,TResponse\>\(System\.IServiceProvider, FlowT\.Abstractions\.FlowDefinition\<TRequest,TResponse\>\)\.definition'), but with the pipeline initialized\.

### Remarks

This method calls [FlowT\.Abstractions\.FlowDefinition&lt;&gt;\.InitializePipeline\(System\.IServiceProvider\)](https://learn.microsoft.com/en-us/dotnet/api/flowt.abstractions.flowdefinition-2.initializepipeline#flowt-abstractions-flowdefinition-2-initializepipeline(system-iserviceprovider) 'FlowT\.Abstractions\.FlowDefinition\`2\.InitializePipeline\(System\.IServiceProvider\)') which builds the execution pipeline
by chaining together specifications, policies, and the handler. The initialization happens only once per flow instance
using lazy initialization with thread-safe double-check locking.

<strong>Performance:</strong> Pipeline initialization is expensive (reflection, expression compilation), but it only
            happens once per flow type. Subsequent executions use the cached pipeline, making FlowT 9-10x faster than alternatives
            that rebuild pipelines on every request.\<example\>
            This method is typically called automatically by the DI system:
            \<code\>
            services\.AddSingleton&lt;IFlow&lt;CreateUserRequest, CreateUserResponse&gt;&gt;\(sp =&gt;
            \{
                var definition = sp\.GetRequiredService&lt;CreateUserFlow&gt;\(\);
                return FlowFactory\.Create\(sp, definition\);
            \}\);
            \</code\>\</example\>