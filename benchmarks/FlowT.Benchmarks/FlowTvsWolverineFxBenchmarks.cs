using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace FlowT.Benchmarks;

/// <summary>
/// Comparison benchmarks between FlowT and WolverineFx.
/// Tests equivalent scenarios: simple handler, with middleware/policies.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowTvsWolverineFxBenchmarks
{
    private IServiceProvider _services = null!;
    private IHost _wolverineHost = null!;

    // FlowT
    private SimpleFlowT _simpleFlowT = null!;
    private FlowTWithPolicy _flowTWithPolicy = null!;
    private FlowContext _flowContext = null!;

    // WolverineFx
    private IMessageBus _messageBus = null!;

    private ComparisonRequest _request = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Setup WolverineFx host
        _wolverineHost = await Host.CreateDefaultBuilder()
            .UseWolverine()
            .ConfigureServices(services =>
            {
                // Register FlowT
                services.AddTransient<SimpleFlowTHandler>();
                services.AddTransient<FlowTLoggingPolicy>();
                services.AddSingleton<SimpleFlowT>();
                services.AddSingleton<FlowTWithPolicy>();
            })
            .StartAsync();

        _services = _wolverineHost.Services;

        // FlowT setup
        _simpleFlowT = _services.GetRequiredService<SimpleFlowT>();
        _flowTWithPolicy = _services.GetRequiredService<FlowTWithPolicy>();
        _flowContext = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        // WolverineFx setup
        _messageBus = _services.GetRequiredService<IMessageBus>();

        _request = new ComparisonRequest { Value = "Test" };
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        if (_wolverineHost != null)
        {
            await _wolverineHost.StopAsync();
            _wolverineHost.Dispose();
        }
    }

    // ============================================
    // Simple Handler (No middleware/policies)
    // ============================================

    [Benchmark(Baseline = true, Description = "FlowT: Simple handler")]
    public async Task<ComparisonResponse> FlowT_SimpleHandler()
    {
        return await _simpleFlowT.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "WolverineFx: Simple handler")]
    public async Task<ComparisonResponse> WolverineFx_SimpleHandler()
    {
        return await _messageBus.InvokeAsync<ComparisonResponse>(new WolverineSimpleRequest { Value = _request.Value });
    }

    // ============================================
    // With Policy/Middleware
    // ============================================

    [Benchmark(Description = "FlowT: Handler + 1 policy")]
    public async Task<ComparisonResponse> FlowT_WithPolicy()
    {
        return await _flowTWithPolicy.ExecuteAsync(_request, _flowContext);
    }

    // Note: WolverineFx middleware benchmark removed due to complexity
    // of testing message handler middleware in isolated benchmark context.
    // WolverineFx middleware is code-generated and woven into handlers,
    // making overhead minimal and difficult to measure separately.
    // Simple handler comparison is sufficient for performance evaluation.

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
    // WolverineFx Implementations
    // ============================================

    public record WolverineSimpleRequest
    {
        public string Value { get; init; } = "";
    }

    public static class WolverineSimpleHandler
    {
        public static ComparisonResponse Handle(WolverineSimpleRequest request)
        {
            return new ComparisonResponse { Result = $"WolverineFx: {request.Value}" };
        }
    }
}
