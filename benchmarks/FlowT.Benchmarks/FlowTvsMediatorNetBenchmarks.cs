
using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Comparison benchmarks between FlowT and Mediator.Net.
/// Tests equivalent scenarios: simple handler, with middleware/pipeline.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowTvsMediatorNetBenchmarks
{
    private IServiceProvider _services = null!;

    // FlowT
    private SimpleFlowT _simpleFlowT = null!;
    private FlowTWithPolicy _flowTWithPolicy = null!;
    private FlowContext _flowContext = null!;

    // Mediator.Net
    private IMediator _mediator = null!;

    private ComparisonRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register FlowT
        services.AddTransient<SimpleFlowTHandler>();
        services.AddTransient<FlowTLoggingPolicy>();
        services.AddSingleton<SimpleFlowT>();
        services.AddSingleton<FlowTWithPolicy>();

        _services = services.BuildServiceProvider();

        // Mediator.Net setup
        var mediatorBuilder = new MediatorBuilder();
        mediatorBuilder.RegisterHandlers(typeof(FlowTvsMediatorNetBenchmarks).Assembly);
        _mediator = mediatorBuilder.Build();

        // FlowT setup
        _simpleFlowT = _services.GetRequiredService<SimpleFlowT>();
        _flowTWithPolicy = _services.GetRequiredService<FlowTWithPolicy>();
        _flowContext = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _request = new ComparisonRequest { Value = "Test" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ============================================
    // Simple Handler (No middleware/policies)
    // ============================================

    [Benchmark(Baseline = true, Description = "FlowT: Simple handler")]
    public async Task<ComparisonResponse> FlowT_SimpleHandler()
    {
        return await _simpleFlowT.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "Mediator.Net: Simple handler")]
    public async Task<MediatorNetResponse> MediatorNet_SimpleHandler()
    {
        return await _mediator.RequestAsync<MediatorNetSimpleRequest, MediatorNetResponse>(
            new MediatorNetSimpleRequest { Value = _request.Value });
    }

    // ============================================
    // With Policy/Pipeline
    // ============================================

    [Benchmark(Description = "FlowT: Handler + 1 policy")]
    public async Task<ComparisonResponse> FlowT_WithPolicy()
    {
        return await _flowTWithPolicy.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "Mediator.Net: Handler + pipeline")]
    public async Task<MediatorNetResponse> MediatorNet_WithPipeline()
    {
        return await _mediator.RequestAsync<MediatorNetRequestWithPipeline, MediatorNetResponse>(
            new MediatorNetRequestWithPipeline { Value = _request.Value });
    }

    // ============================================
    // Test Data
    // ============================================

    public record ComparisonRequest
    {
        public string Value { get; init; } = "";
    }

    public record ComparisonResponse
    {
        public string Result { get; init; } = "";
    }

    // ============================================
    // FlowT Implementations
    // ============================================

    public class SimpleFlowT : FlowDefinition<ComparisonRequest, ComparisonResponse>
    {
        protected override void Configure(IFlowBuilder<ComparisonRequest, ComparisonResponse> flow)
        {
            flow.Handle<SimpleFlowTHandler>();
        }
    }

    public class SimpleFlowTHandler : IFlowHandler<ComparisonRequest, ComparisonResponse>
    {
        public ValueTask<ComparisonResponse> HandleAsync(ComparisonRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new ComparisonResponse { Result = $"FlowT: {request.Value}" });
        }
    }

    public class FlowTWithPolicy : FlowDefinition<ComparisonRequest, ComparisonResponse>
    {
        protected override void Configure(IFlowBuilder<ComparisonRequest, ComparisonResponse> flow)
        {
            flow.Use<FlowTLoggingPolicy>()
                .Handle<SimpleFlowTHandler>();
        }
    }

    public class FlowTLoggingPolicy : FlowPolicy<ComparisonRequest, ComparisonResponse>
    {
        public override async ValueTask<ComparisonResponse> HandleAsync(ComparisonRequest request, FlowContext context)
        {
            // Simulate logging
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[Logged]{result.Result}" };
        }
    }

    // ============================================
    // Mediator.Net Implementations
    // ============================================

    public class MediatorNetSimpleRequest : IRequest
    {
        public string Value { get; set; } = "";
    }

    public class MediatorNetResponse : IResponse
    {
        public string Result { get; set; } = "";
    }

    public class MediatorNetSimpleHandler : IRequestHandler<MediatorNetSimpleRequest, MediatorNetResponse>
    {
        public Task<MediatorNetResponse> Handle(IReceiveContext<MediatorNetSimpleRequest> context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MediatorNetResponse { Result = $"Mediator.Net: {context.Message.Value}" });
        }
    }

    public class MediatorNetRequestWithPipeline : IRequest
    {
        public string Value { get; set; } = "";
    }

    public class MediatorNetHandlerWithPipeline : IRequestHandler<MediatorNetRequestWithPipeline, MediatorNetResponse>
    {
        public Task<MediatorNetResponse> Handle(IReceiveContext<MediatorNetRequestWithPipeline> context, CancellationToken cancellationToken)
        {
            var result = $"Mediator.Net: {context.Message.Value}";
            return Task.FromResult(new MediatorNetResponse { Result = $"[Logged]{result}" });
        }
    }
}
