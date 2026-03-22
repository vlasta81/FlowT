using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests;

/// <summary>
/// Tests for Policy behavior through public APIs (via FlowDefinition execution)
/// </summary>
public class PolicyIntegrationTests
{
    [Fact]
    public async Task Policy_WrapHandler_AndModifiesResponse()
    {
        var services = new ServiceCollection()
            .AddTransient<SimpleHandler>()
            .AddTransient<ResponseModifyingPolicy>()
            .AddSingleton<FlowWithPolicy>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithPolicy>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new TestRequest { Value = "Test" }, context);

        Assert.Equal("Echo: Test [Modified]", result.Message);
    }

    [Fact]
    public async Task MultiplePolicies_ApplyInCorrectOrder()
    {
        var services = new ServiceCollection()
            .AddTransient<SimpleHandler>()
            .AddTransient<Policy1>()
            .AddTransient<Policy2>()
            .AddSingleton<FlowWithMultiplePolicies>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithMultiplePolicies>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new TestRequest { Value = "Test" }, context);

        // Policies wrap outer to inner: [P1 -> P2 -> Handler]
        Assert.Equal("[P1[P2Echo: Test]]", result.Message);
    }

    [Fact]
    public async Task TransactionPolicy_ExecutesCorrectly()
    {
        var services = new ServiceCollection()
            .AddTransient<SimpleHandler>()
            .AddTransient<TestTransactionPolicy>()
            .AddSingleton<FlowWithTransaction>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithTransaction>();
        var context = CreateContext(services);

        await flow.ExecuteAsync(new TestRequest { Value = "Test" }, context);

        Assert.True(context.TryGet<bool>(out var committed));
        Assert.True(committed);
    }

    [Fact]
    public async Task LoggingPolicy_CapturesExecution()
    {
        var services = new ServiceCollection()
            .AddTransient<SimpleHandler>()
            .AddTransient<TestLoggingPolicy>()
            .AddSingleton<FlowWithLogging>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithLogging>();
        var context = CreateContext(services);

        await flow.ExecuteAsync(new TestRequest { Value = "Test" }, context);

        Assert.True(context.TryGet<List<string>>(out var logs));
        Assert.Contains(logs, l => l.Contains("Before"));
        Assert.Contains(logs, l => l.Contains("After"));
    }

    [Fact]
    public async Task Policy_CanAccessAndModifyContext()
    {
        var services = new ServiceCollection()
            .AddTransient<ContextReadingHandler>()
            .AddTransient<ContextSettingPolicy>()
            .AddSingleton<FlowWithContextPolicy>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithContextPolicy>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new TestRequest { Value = "Test" }, context);

        Assert.Equal("PolicyValue", result.Message);
    }

    // Helper methods
    private static FlowContext CreateContext(IServiceProvider services)
    {
        return new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };
    }

    // Test types
    public record TestRequest
    {
        public string Value { get; init; } = "";
    }

    public record TestResponse
    {
        public string Message { get; init; } = "";
    }

    // Simple handler
    public class SimpleHandler : IFlowHandler<TestRequest, TestResponse>
    {
        public ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new TestResponse { Message = $"Echo: {request.Value}" });
        }
    }

    // Response modifying policy
    public class ResponseModifyingPolicy : FlowPolicy<TestRequest, TestResponse>
    {
        public override async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            var response = await Next.HandleAsync(request, context);
            return response with { Message = response.Message + " [Modified]" };
        }
    }

    // Policy 1
    public class Policy1 : FlowPolicy<TestRequest, TestResponse>
    {
        public override async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            var response = await Next.HandleAsync(request, context);
            return response with { Message = $"[P1{response.Message}]" };
        }
    }

    // Policy 2
    public class Policy2 : FlowPolicy<TestRequest, TestResponse>
    {
        public override async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            var response = await Next.HandleAsync(request, context);
            return response with { Message = $"[P2{response.Message}]" };
        }
    }

    // Transaction policy
    public class TestTransactionPolicy : FlowPolicy<TestRequest, TestResponse>
    {
        public override async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            context.Set(true); // Mark as committed
            return result;
        }
    }

    // Logging policy
    public class TestLoggingPolicy : FlowPolicy<TestRequest, TestResponse>
    {
        public override async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            var logs = context.GetOrAdd(() => new List<string>());
            logs.Add("Before handler");

            var result = await Next.HandleAsync(request, context);

            logs.Add("After handler");
            return result;
        }
    }

    // Context setting policy
    public class ContextSettingPolicy : FlowPolicy<TestRequest, TestResponse>
    {
        public override ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            context.Set("PolicyValue");
            return Next.HandleAsync(request, context);
        }
    }

    // Context reading handler
    public class ContextReadingHandler : IFlowHandler<TestRequest, TestResponse>
    {
        public ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            context.TryGet<string>(out var value);
            return ValueTask.FromResult(new TestResponse { Message = value ?? "NoValue" });
        }
    }

    // Flows
    public class FlowWithPolicy : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Use<ResponseModifyingPolicy>()
                .Handle<SimpleHandler>();
        }
    }

    public class FlowWithMultiplePolicies : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Use<Policy1>()
                .Use<Policy2>()
                .Handle<SimpleHandler>();
        }
    }

    public class FlowWithTransaction : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Use<TestTransactionPolicy>()
                .Handle<SimpleHandler>();
        }
    }

    public class FlowWithLogging : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Use<TestLoggingPolicy>()
                .Handle<SimpleHandler>();
        }
    }

    public class FlowWithContextPolicy : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Use<ContextSettingPolicy>()
                .Handle<ContextReadingHandler>();
        }
    }
}
