using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Comparison benchmarks between FlowT and Brighter.
/// Tests equivalent scenarios: simple handler, with request handler and policies.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowTvsBrighterBenchmarks
{
    private IServiceProvider _services = null!;

    // FlowT
    private SimpleFlowT _simpleFlowT = null!;
    private FlowTWithPolicy _flowTWithPolicy = null!;
    private FlowContext _flowContext = null!;

    // Brighter
    private IAmACommandProcessor _commandProcessor = null!;

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

        // Register Brighter
        services.AddBrighter()
            .AutoFromAssemblies(new[] { typeof(FlowTvsBrighterBenchmarks).Assembly });

        _services = services.BuildServiceProvider();

        // FlowT setup
        _simpleFlowT = _services.GetRequiredService<SimpleFlowT>();
        _flowTWithPolicy = _services.GetRequiredService<FlowTWithPolicy>();
        _flowContext = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        // Brighter setup
        _commandProcessor = _services.GetRequiredService<IAmACommandProcessor>();

        _request = new ComparisonRequest { Value = "Test" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ============================================
    // Simple Handler (No policies)
    // ============================================

    [Benchmark(Baseline = true, Description = "FlowT: Simple handler")]
    public async Task<ComparisonResponse> FlowT_SimpleHandler()
    {
        return await _simpleFlowT.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "Brighter: Simple handler")]
    public ComparisonResponse Brighter_SimpleHandler()
    {
        var command = new BrighterSimpleCommand("Test");
        _commandProcessor.Send(command);
        return command.Response!;
    }

    // ============================================
    // With Policy
    // ============================================

    [Benchmark(Description = "FlowT: Handler + 1 policy")]
    public async Task<ComparisonResponse> FlowT_WithPolicy()
    {
        return await _flowTWithPolicy.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "Brighter: Handler + 1 policy")]
    public ComparisonResponse Brighter_WithPolicy()
    {
        var command = new BrighterCommandWithPolicy("Test");
        _commandProcessor.Send(command);
        return command.Response!;
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
    // Brighter Implementations
    // ============================================

    public class BrighterSimpleCommand(string value) : Command(Guid.NewGuid())
    {
        public string Value { get; } = value;
        public ComparisonResponse? Response { get; set; }
    }

    public class BrighterSimpleCommandHandler : RequestHandler<BrighterSimpleCommand>
    {
        public override BrighterSimpleCommand Handle(BrighterSimpleCommand command)
        {
            command.Response = new ComparisonResponse { Result = $"Brighter: {command.Value}" };
            return base.Handle(command);
        }
    }

    public class BrighterCommandWithPolicy(string value) : Command(Guid.NewGuid())
    {
        public string Value { get; } = value;
        public ComparisonResponse? Response { get; set; }
    }

    public class BrighterCommandWithPolicyHandler : RequestHandler<BrighterCommandWithPolicy>
    {
        public override BrighterCommandWithPolicy Handle(BrighterCommandWithPolicy command)
        {
            var result = $"Brighter: {command.Value}";
            command.Response = new ComparisonResponse { Result = $"[Logged]{result}" };
            return base.Handle(command);
        }
    }
}
