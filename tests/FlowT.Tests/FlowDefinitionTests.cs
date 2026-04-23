using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests;

public class FlowDefinitionTests
{
    [Fact]
    public async Task ExecuteAsync_CallsHandler_WhenNoSpecifications()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<SimpleFlow>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        Assert.Equal("Handled: Test", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_StopsAtSpecification_WhenValidationFails()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithFailingSpec>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Invalid" }, context);

        Assert.Equal("Validation failed", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ContinuesToHandler_WhenSpecificationPasses()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithPassingSpec>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Valid" }, context);

        Assert.Equal("Handled: Valid", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_AppliesPolicies_InCorrectOrder()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithPolicies>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        // Policies wrap from outer to inner: [Policy1 -> Policy2 -> Handler]
        Assert.Equal("[Policy1[Policy2[Handled: Test]]]", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_OnInterrupt_MapsSpecificationFailure()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithInterruptMapper>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Invalid" }, context);

        Assert.Equal("Mapped error: Validation failed", result.Message);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_InitializesOnlyOnce()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<SimpleFlow>();
        var context = CreateContext(services);

        // First call
        await flow.ExecuteAsync(new SimpleRequest { Value = "First" }, context);

        // Second call (should reuse initialized pipeline)
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Second" }, context);

        Assert.Equal("Handled: Second", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleSpecifications_ExecuteInOrder()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithMultipleSpecs>();
        var context = CreateContext(services);

        // First spec should fail
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Fail1" }, context);
        Assert.Equal("Spec1 failed", result.Message);

        // First spec passes, second fails
        var result2 = await flow.ExecuteAsync(new SimpleRequest { Value = "Fail2" }, context);
        Assert.Equal("Spec2 failed", result2.Message);

        // Both specs pass
        var result3 = await flow.ExecuteAsync(new SimpleRequest { Value = "Valid" }, context);
        Assert.Equal("Handled: Valid", result3.Message);
    }

    [Fact]
    public void Configure_ThrowsException_WhenHandlerNotSet()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithoutHandler>();
        var context = CreateContext(services);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context));
    }

    [Fact]
    public async Task ExecuteAsync_PassesContextToHandler()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowUsingContext>();
        var context = CreateContext(services);
        context.Set("ContextValue");

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        Assert.Equal("Handler received: ContextValue", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_EarlyReturn_SkipsHandler()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithEarlyReturn>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "EarlyReturn" }, context);

        Assert.Equal("Early result", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceProvider_ExecutesFlow()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<SimpleFlow>();

        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "ViaServiceProvider" },
            services,
            CancellationToken.None);

        Assert.Equal("Handled: ViaServiceProvider", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceProvider_ThrowsOnCancellation()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<CancellableFlow>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            flow.ExecuteAsync(new SimpleRequest { Value = "test" }, services, cts.Token).AsTask());
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenInterruptHasNoMapperAndResponseNotCastable()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithNoMapperNoCompatibleResponse>();
        var context = CreateContext(services);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            flow.ExecuteAsync(new SimpleRequest { Value = "Invalid" }, context).AsTask());
    }

    // Helper methods
    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();

        // Register all handlers, specs, policies
        services.AddTransient<SimpleHandler>();
        services.AddTransient<FailingSpecification>();
        services.AddTransient<PassingSpecification>();
        services.AddTransient<TestPolicy1>();
        services.AddTransient<TestPolicy2>();
        services.AddTransient<ContextAwareHandler>();
        services.AddTransient<EarlyReturnSpecification>();
        services.AddTransient<OrderedSpec1>();
        services.AddTransient<OrderedSpec2>();
        services.AddTransient<CancellableHandler>();

        // Register flows
        services.AddSingleton<SimpleFlow>();
        services.AddSingleton<FlowWithFailingSpec>();
        services.AddSingleton<FlowWithPassingSpec>();
        services.AddSingleton<FlowWithPolicies>();
        services.AddSingleton<FlowWithInterruptMapper>();
        services.AddSingleton<FlowWithMultipleSpecs>();
        services.AddSingleton<FlowWithoutHandler>();
        services.AddSingleton<FlowUsingContext>();
        services.AddSingleton<FlowWithEarlyReturn>();
        services.AddSingleton<CancellableFlow>();
        services.AddSingleton<FlowWithNoMapperNoCompatibleResponse>();

        return services.BuildServiceProvider();
    }

    private static FlowContext CreateContext(IServiceProvider services)
    {
        return new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };
    }

    // Test data types
    public record SimpleRequest
    {
        public string Value { get; init; } = "";
    }

    public record SimpleResponse
    {
        public string Message { get; init; } = "";
        public int StatusCode { get; init; } = 200;
    }

    // Simple flow
    public class SimpleFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<SimpleHandler>();
        }
    }

    public class SimpleHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new SimpleResponse { Message = $"Handled: {request.Value}" });
        }
    }

    // Flow with failing specification
    public class FlowWithFailingSpec : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<FailingSpecification>()
                .OnInterrupt(interrupt => new SimpleResponse { Message = interrupt.Message ?? "Error" })
                .Handle<SimpleHandler>();
        }
    }

    public class FailingSpecification : IFlowSpecification<SimpleRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SimpleRequest request, FlowContext context)
        {
            if (request.Value == "Invalid")
            {
                var interrupt = FlowInterrupt<object?>.Fail("Validation failed");
                return ValueTask.FromResult<FlowInterrupt<object?>?>(interrupt);
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Flow with passing specification
    public class FlowWithPassingSpec : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<PassingSpecification>()
                .Handle<SimpleHandler>();
        }
    }

    public class PassingSpecification : IFlowSpecification<SimpleRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SimpleRequest request, FlowContext context)
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Flow with policies
    public class FlowWithPolicies : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Use<TestPolicy1>()
                .Use<TestPolicy2>()
                .Handle<SimpleHandler>();
        }
    }

    public class TestPolicy1 : FlowPolicy<SimpleRequest, SimpleResponse>
    {
        public override async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Message = $"[Policy1{result.Message}]" };
        }
    }

    public class TestPolicy2 : FlowPolicy<SimpleRequest, SimpleResponse>
    {
        public override async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Message = $"[Policy2[{result.Message}]]" };
        }
    }

    // Flow with interrupt mapper
    public class FlowWithInterruptMapper : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<FailingSpecification>()
                .OnInterrupt(interrupt => new SimpleResponse
                {
                    Message = $"Mapped error: {interrupt.Message}",
                    StatusCode = interrupt.StatusCode
                })
                .Handle<SimpleHandler>();
        }
    }

    // Flow with multiple specifications
    public class FlowWithMultipleSpecs : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<OrderedSpec1>()
                .Check<OrderedSpec2>()
                .OnInterrupt(interrupt => new SimpleResponse { Message = interrupt.Message ?? "Error" })
                .Handle<SimpleHandler>();
        }
    }

    public class OrderedSpec1 : IFlowSpecification<SimpleRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SimpleRequest request, FlowContext context)
        {
            if (request.Value == "Fail1")
            {
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Fail("Spec1 failed"));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    public class OrderedSpec2 : IFlowSpecification<SimpleRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SimpleRequest request, FlowContext context)
        {
            if (request.Value == "Fail2")
            {
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Fail("Spec2 failed"));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Flow without handler (should throw)
    public class FlowWithoutHandler : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<PassingSpecification>();
            // No Handle() call - should throw
        }
    }

    // Flow using context
    public class FlowUsingContext : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<ContextAwareHandler>();
        }
    }

    public class ContextAwareHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            context.TryGet<string>(out var value);
            return ValueTask.FromResult(new SimpleResponse { Message = $"Handler received: {value}" });
        }
    }

    // Flow with early return
    public class FlowWithEarlyReturn : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<EarlyReturnSpecification>()
                .OnInterrupt(interrupt => interrupt.Response as SimpleResponse ?? new SimpleResponse())
                .Handle<SimpleHandler>();
        }
    }

    public class EarlyReturnSpecification : IFlowSpecification<SimpleRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SimpleRequest request, FlowContext context)
        {
            if (request.Value == "EarlyReturn")
            {
                var response = new SimpleResponse { Message = "Early result" };
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Stop(response));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Flow that checks cancellation in handler
    public class CancellableFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<CancellableHandler>();
        }
    }

    public class CancellableHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            context.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new SimpleResponse { Message = "Handled" });
        }
    }

    // Flow with a failing spec but no interrupt mapper and response type is not castable from interrupt response
    public class FlowWithNoMapperNoCompatibleResponse : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Check<FailingSpecification>()
                .Handle<SimpleHandler>();
            // No OnInterrupt mapper; FailingSpecification returns Fail(message) with null Response
        }
    }
}
