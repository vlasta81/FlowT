using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks comparing <see cref="IFlowSpecification{TRequest}"/> (manual ValueTask.FromResult)
/// against <see cref="FlowSpecification{TRequest}"/> (cached Continue/Fail helpers).
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowSpecificationBenchmarks
{
    private IServiceProvider _services = null!;
    private FlowWithInterfaceSpec _interfaceFlow = null!;
    private FlowWithBaseSpec _baseFlow = null!;
    private FlowContext _context = null!;
    private SpecRequest _validRequest = null!;
    private SpecRequest _invalidRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<FlowWithInterfaceSpec>();
        services.AddSingleton<FlowWithBaseSpec>();
        _services = services.BuildServiceProvider();

        _interfaceFlow = _services.GetRequiredService<FlowWithInterfaceSpec>();
        _baseFlow = _services.GetRequiredService<FlowWithBaseSpec>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _validRequest = new SpecRequest { Value = "ok" };
        _invalidRequest = new SpecRequest { Value = "bad" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ── Continue path ─────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "IFlowSpecification — ValueTask.FromResult(null)")]
    public async Task<SpecResponse> Interface_Continue()
    {
        return await _interfaceFlow.ExecuteAsync(_validRequest, _context);
    }

    [Benchmark(Description = "FlowSpecification — Continue() cached")]
    public async Task<SpecResponse> Base_Continue()
    {
        return await _baseFlow.ExecuteAsync(_validRequest, _context);
    }

    // ── Fail path ─────────────────────────────────────────────────────────────

    [Benchmark(Description = "IFlowSpecification — ValueTask.FromResult(Fail(...))")]
    public async Task<SpecResponse> Interface_Fail()
    {
        return await _interfaceFlow.ExecuteAsync(_invalidRequest, _context);
    }

    [Benchmark(Description = "FlowSpecification — Fail(...) helper")]
    public async Task<SpecResponse> Base_Fail()
    {
        return await _baseFlow.ExecuteAsync(_invalidRequest, _context);
    }

    // ── Data ──────────────────────────────────────────────────────────────────

    public record SpecRequest
    {
        public string Value { get; init; } = "";
    }

    public record SpecResponse
    {
        public string Result { get; init; } = "";
    }

    // ── Flows ─────────────────────────────────────────────────────────────────

    public class FlowWithInterfaceSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<InterfaceSpec>()
                .OnInterrupt(i => new SpecResponse { Result = i.Message ?? "Error" })
                .Handle<SpecHandler>();
        }
    }

    public class FlowWithBaseSpec : FlowDefinition<SpecRequest, SpecResponse>
    {
        protected override void Configure(IFlowBuilder<SpecRequest, SpecResponse> flow)
        {
            flow.Check<BaseSpec>()
                .OnInterrupt(i => new SpecResponse { Result = i.Message ?? "Error" })
                .Handle<SpecHandler>();
        }
    }

    // ── Spec implementations ──────────────────────────────────────────────────

    /// <summary>Manual implementation — allocates a new ValueTask wrapper on every call.</summary>
    public class InterfaceSpec : IFlowSpecification<SpecRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
        {
            if (request.Value == "bad")
                return ValueTask.FromResult<FlowInterrupt<object?>?>(FlowInterrupt<object?>.Fail("Invalid value"));
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    /// <summary>Base class implementation — Continue() returns a static cached ValueTask (zero alloc).</summary>
    public class BaseSpec : FlowSpecification<SpecRequest>
    {
        public override ValueTask<FlowInterrupt<object?>?> CheckAsync(SpecRequest request, FlowContext context)
        {
            if (request.Value == "bad")
                return Fail("Invalid value");
            return Continue();
        }
    }

    public class SpecHandler : IFlowHandler<SpecRequest, SpecResponse>
    {
        public ValueTask<SpecResponse> HandleAsync(SpecRequest request, FlowContext context)
            => ValueTask.FromResult(new SpecResponse { Result = $"ok:{request.Value}" });
    }
}
