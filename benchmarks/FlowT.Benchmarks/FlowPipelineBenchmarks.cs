using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for FlowDefinition pipeline execution.
/// Tests handler execution, specifications, policies, and full pipelines.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowPipelineBenchmarks
{
    private IServiceProvider _services = null!;
    private SimpleFlow _simpleFlow = null!;
    private FlowWithSpec _flowWithSpec = null!;
    private FlowWithPolicy _flowWithPolicy = null!;
    private ComplexFlow _complexFlow = null!;
    private FlowContext _context = null!;
    private BenchmarkRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register flows
        services.AddSingleton<SimpleFlow>();
        services.AddSingleton<FlowWithSpec>();
        services.AddSingleton<FlowWithPolicy>();
        services.AddSingleton<ComplexFlow>();

        _services = services.BuildServiceProvider();
        _simpleFlow = _services.GetRequiredService<SimpleFlow>();
        _flowWithSpec = _services.GetRequiredService<FlowWithSpec>();
        _flowWithPolicy = _services.GetRequiredService<FlowWithPolicy>();
        _complexFlow = _services.GetRequiredService<ComplexFlow>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _request = new BenchmarkRequest { Value = "Test" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    [Benchmark(Baseline = true, Description = "Simple handler only")]
    public async Task<BenchmarkResponse> SimpleFlow_Execute()
    {
        return await _simpleFlow.ExecuteAsync(_request, _context);
    }

    [Benchmark(Description = "Handler + 1 specification")]
    public async Task<BenchmarkResponse> FlowWithSpec_Execute()
    {
        return await _flowWithSpec.ExecuteAsync(_request, _context);
    }

    [Benchmark(Description = "Handler + 1 policy")]
    public async Task<BenchmarkResponse> FlowWithPolicy_Execute()
    {
        return await _flowWithPolicy.ExecuteAsync(_request, _context);
    }

    [Benchmark(Description = "Handler + 1 spec + 3 policies")]
    public async Task<BenchmarkResponse> ComplexFlow_Execute()
    {
        return await _complexFlow.ExecuteAsync(_request, _context);
    }

    [Benchmark(Description = "Create flow context")]
    public FlowContext CreateContext()
    {
        return new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };
    }

    // Test data
    public record BenchmarkRequest
    {
        public string Value { get; init; } = "";
    }

    public record BenchmarkResponse
    {
        public string Result { get; init; } = "";
    }

    // Simple flow - baseline
    public class SimpleFlow : FlowDefinition<BenchmarkRequest, BenchmarkResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkRequest, BenchmarkResponse> flow)
        {
            flow.Handle<SimpleHandler>();
        }
    }

    public class SimpleHandler : IFlowHandler<BenchmarkRequest, BenchmarkResponse>
    {
        public ValueTask<BenchmarkResponse> HandleAsync(BenchmarkRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new BenchmarkResponse { Result = $"Handled: {request.Value}" });
        }
    }

    // Flow with specification
    public class FlowWithSpec : FlowDefinition<BenchmarkRequest, BenchmarkResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkRequest, BenchmarkResponse> flow)
        {
            flow.Check<PassingSpec>()
                .Handle<SimpleHandler>();
        }
    }

    public class PassingSpec : IFlowSpecification<BenchmarkRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(BenchmarkRequest request, FlowContext context)
        {
            // Always pass
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Flow with policy
    public class FlowWithPolicy : FlowDefinition<BenchmarkRequest, BenchmarkResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkRequest, BenchmarkResponse> flow)
        {
            flow.Use<BenchmarkPolicy>()
                .Handle<SimpleHandler>();
        }
    }

    public class BenchmarkPolicy : FlowPolicy<BenchmarkRequest, BenchmarkResponse>
    {
        public override async ValueTask<BenchmarkResponse> HandleAsync(BenchmarkRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[Policy]{result.Result}" };
        }
    }

    // Complex flow with multiple components
    public class ComplexFlow : FlowDefinition<BenchmarkRequest, BenchmarkResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkRequest, BenchmarkResponse> flow)
        {
            flow.Check<PassingSpec>()
                .Use<Policy1>()
                .Use<Policy2>()
                .Use<Policy3>()
                .Handle<SimpleHandler>();
        }
    }

    public class Policy1 : FlowPolicy<BenchmarkRequest, BenchmarkResponse>
    {
        public override async ValueTask<BenchmarkResponse> HandleAsync(BenchmarkRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[P1]{result.Result}" };
        }
    }

    public class Policy2 : FlowPolicy<BenchmarkRequest, BenchmarkResponse>
    {
        public override async ValueTask<BenchmarkResponse> HandleAsync(BenchmarkRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[P2]{result.Result}" };
        }
    }

    public class Policy3 : FlowPolicy<BenchmarkRequest, BenchmarkResponse>
    {
        public override async ValueTask<BenchmarkResponse> HandleAsync(BenchmarkRequest request, FlowContext context)
        {
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[P3]{result.Result}" };
        }
    }
}
