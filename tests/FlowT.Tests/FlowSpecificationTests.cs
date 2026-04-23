using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests;

public class FlowSpecificationTests
{
    // ── Unit tests for the helper methods ────────────────────────────────────

    [Fact]
    public async Task Continue_ReturnsNullInterrupt()
    {
        var spec = new AlwaysContinueSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task Continue_ReturnsSameInstanceOnMultipleCalls()
    {
        var spec = new AlwaysContinueSpec();
        var context = CreateContext();
        var request = new SpecRequest { Value = "any" };

        var first = await spec.CheckAsync(request, context);
        var second = await spec.CheckAsync(request, context);

        // Both must be null — the cached path returns the same logical result
        Assert.Null(first);
        Assert.Null(second);
    }

    [Fact]
    public async Task Fail_ReturnsInterruptWithMessageAndDefaultStatusCode()
    {
        var spec = new FailWithDefaultStatusSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.NotNull(result);
        Assert.Equal("Validation failed", result!.Value.Message);
        Assert.Equal(400, result.Value.StatusCode);
    }

    [Fact]
    public async Task Fail_ReturnsInterruptWithCustomStatusCode()
    {
        var spec = new FailWithCustomStatusSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.NotNull(result);
        Assert.Equal("Not found", result!.Value.Message);
        Assert.Equal(404, result.Value.StatusCode);
    }

    [Fact]
    public async Task Stop_ReturnsInterruptWithEarlyReturnAndDefaultStatusCode()
    {
        var spec = new StopWithDefaultStatusSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.NotNull(result);
        Assert.Equal("cached-value", result!.Value.Response);
        Assert.Equal(200, result.Value.StatusCode);
    }

    [Fact]
    public async Task Stop_ReturnsInterruptWithEarlyReturnAndCustomStatusCode()
    {
        var spec = new StopWithCustomStatusSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.NotNull(result);
        Assert.Equal("created-value", result!.Value.Response);
        Assert.Equal(201, result.Value.StatusCode);
    }

    [Fact]
    public async Task Stop_WithNullEarlyReturn_ReturnsInterruptWithNullValue()
    {
        var spec = new StopWithNullValueSpec();
        var result = await spec.CheckAsync(new SpecRequest { Value = "any" }, CreateContext());

        Assert.NotNull(result);
        Assert.Null(result!.Value.Response);
        Assert.Equal(204, result.Value.StatusCode);
    }

    // ── Type system tests ────────────────────────────────────────────────────

    [Fact]
    public void FlowSpecification_ImplementsIFlowSpecification()
    {
        var spec = new AlwaysContinueSpec();
        Assert.IsAssignableFrom<IFlowSpecification<SpecRequest>>(spec);
    }

    // ── Pipeline integration tests ───────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ContinuesToHandler_WhenSpecificationCallsContinue()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithContinueSpec>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SpecRequest { Value = "hello" }, context);

        Assert.Equal("Handled: hello", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_StopsPipeline_WhenSpecificationCallsFail()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithFailSpec>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SpecRequest { Value = "bad" }, context);

        Assert.Equal("Mapped: Request is invalid", result.Message);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_StopsPipeline_WhenSpecificationCallsStop()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithStopSpec>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SpecRequest { Value = "cached" }, context);

        Assert.Equal("early-response", result.Message);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ConditionalSpec_ContinuesOrFails_BasedOnRequest()
    {
        var services = CreateServices();
        var flow = services.GetRequiredService<FlowWithConditionalSpec>();
        var context = CreateContext(services);

        var passing = await flow.ExecuteAsync(new SpecRequest { Value = "valid" }, context);
        var failing = await flow.ExecuteAsync(new SpecRequest { Value = "invalid" }, context);

        Assert.Equal("Handled: valid", passing.Message);
        Assert.Equal("Mapped: Value is invalid", failing.Message);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();

        services.AddTransient<SpecHandler>();
        services.AddTransient<AlwaysContinueSpec>();
        services.AddTransient<AlwaysFailSpec>();
        services.AddTransient<AlwaysStopSpec>();
        services.AddTransient<ConditionalSpec>();

        services.AddSingleton<FlowWithContinueSpec>();
        services.AddSingleton<FlowWithFailSpec>();
        services.AddSingleton<FlowWithStopSpec>();
        services.AddSingleton<FlowWithConditionalSpec>();

        return services.BuildServiceProvider();
    }

    private static FlowContext CreateContext(IServiceProvider? services = null) =>
        new FlowContext
        {
            Services = services ?? new ServiceCollection().BuildServiceProvider(),
            CancellationToken = CancellationToken.None
        };

    // ── Test data types ──────────────────────────────────────────────────────

    public record SpecRequest
    {
        public string Value { get; init; } = "";
    }

    public record SpecResponse
    {
        public string Message { get; init; } = "";
        public int StatusCode { get; init; } = 200;
    }

    // ── Unit-level test specifications ───────────────────────────────────────

    public class AlwaysContinueSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Continue();
    }

    public class FailWithDefaultStatusSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Fail("Validation failed");
    }

    public class FailWithCustomStatusSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Fail("Not found", 404);
    }

    public class StopWithDefaultStatusSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Stop("cached-value");
    }

    public class StopWithCustomStatusSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Stop("created-value", 201);
    }

    public class StopWithNullValueSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Stop(null, 204);
    }

    // ── Pipeline-level test specifications ───────────────────────────────────

    public class AlwaysContinueSpec2 : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Continue();
    }

    public class AlwaysFailSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Fail("Request is invalid", 400);
    }

    public class AlwaysStopSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
            => Stop(new SpecResponse { Message = "early-response", StatusCode = 200 });
    }

    public class ConditionalSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
        {
            if (request.Value == "invalid")
                return Fail("Value is invalid", 400);

            return Continue();
        }
    }

    // ── Pipeline-level test handler ───────────────────────────────────────────

    public class SpecHandler : IFlowHandler<SpecRequest, SpecResponse>
    {
        public ValueTask<SpecResponse> HandleAsync(SpecRequest request, FlowContext context)
            => ValueTask.FromResult(new SpecResponse { Message = $"Handled: {request.Value}" });
    }

    // ── Pipeline-level test flows ─────────────────────────────────────────────

    public class FlowWithContinueSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<AlwaysContinueSpec>()
                .Handle<SpecHandler>();
        }
    }

    public class FlowWithFailSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<AlwaysFailSpec>()
                .OnInterrupt(i => new SpecResponse { Message = $"Mapped: {i.Message}", StatusCode = i.StatusCode })
                .Handle<SpecHandler>();
        }
    }

    public class FlowWithStopSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<AlwaysStopSpec>()
                .OnInterrupt(i => i.Response is SpecResponse r ? r : new SpecResponse { Message = i.Message ?? "" })
                .Handle<SpecHandler>();
        }
    }

    public class FlowWithConditionalSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<ConditionalSpec>()
                .OnInterrupt(i => new SpecResponse { Message = $"Mapped: {i.Message}", StatusCode = i.StatusCode })
                .Handle<SpecHandler>();
        }
    }
}
