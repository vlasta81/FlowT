using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for cancellation-related overhead in FlowContext and FlowDefinition.
/// Measures ThrowIfCancellationRequested on a live (not-cancelled) token, the marginal
/// cost of propagating a CancellationToken through the pipeline, and the cost of a flow
/// that checks cancellation in its handler.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class CancellationBenchmarks
{
    private IServiceProvider _services = null!;
    private CancellationCheckFlow _checkFlow = null!;

    /// <summary>Context whose token is CancellationToken.None — cheapest possible check.</summary>
    private FlowContext _ctxNone = null!;

    /// <summary>Context backed by an unsignalled CancellationTokenSource — real registration overhead.</summary>
    private FlowContext _ctxLive = null!;

    private CancellationTokenSource _cts = null!;
    private readonly CancellationCheckRequest _request = new() { Value = "bench" };

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<CancellationCheckFlow>();
        _services = services.BuildServiceProvider();
        _checkFlow = _services.GetRequiredService<CancellationCheckFlow>();

        _ctxNone = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };

        _cts = new CancellationTokenSource();
        _ctxLive = new FlowContext { Services = _services, CancellationToken = _cts.Token };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cts.Dispose();
        (_services as IDisposable)?.Dispose();
    }

    // ── ThrowIfCancellationRequested overhead ─────────────────────────────────

    /// <summary>
    /// Baseline: CancellationToken.None — IsCancellationRequested is always false with no allocation.
    /// </summary>
    [Benchmark(Baseline = true, Description = "ThrowIfCancellationRequested - CancellationToken.None")]
    public void ThrowIfCancelled_None()
        => _ctxNone.ThrowIfCancellationRequested();

    /// <summary>
    /// Live unsignalled token — tests the real check path (reads CancellationToken state).
    /// </summary>
    [Benchmark(Description = "ThrowIfCancellationRequested - live unsignalled token")]
    public void ThrowIfCancelled_LiveToken()
        => _ctxLive.ThrowIfCancellationRequested();

    // ── Pipeline with cancellation check in handler ───────────────────────────

    /// <summary>
    /// Full pipeline execution with CancellationToken.None — no check overhead.
    /// </summary>
    [Benchmark(Description = "Pipeline with CT check in handler - CancellationToken.None")]
    public ValueTask<CancellationCheckResponse> Pipeline_CancellationCheck_None()
        => _checkFlow.ExecuteAsync(_request, _ctxNone);

    /// <summary>
    /// Full pipeline execution with a live unsignalled token — adds real token read.
    /// </summary>
    [Benchmark(Description = "Pipeline with CT check in handler - live unsignalled token")]
    public ValueTask<CancellationCheckResponse> Pipeline_CancellationCheck_LiveToken()
        => _checkFlow.ExecuteAsync(_request, _ctxLive);

    /// <summary>
    /// Measures the overhead of passing a live CancellationToken through ExecuteAsync(IServiceProvider, CT).
    /// </summary>
    [Benchmark(Description = "ExecuteAsync(IServiceProvider, CT) - live unsignalled token")]
    public ValueTask<CancellationCheckResponse> ExecuteAsync_ServiceProvider_LiveToken()
        => _checkFlow.ExecuteAsync(_request, _services, _cts.Token);

    // ── Supporting types ──────────────────────────────────────────────────────

    public record CancellationCheckRequest
    {
        public string Value { get; init; } = string.Empty;
    }

    public record CancellationCheckResponse
    {
        public string Result { get; init; } = string.Empty;
    }

    public class CancellationCheckFlow : FlowDefinition<CancellationCheckRequest, CancellationCheckResponse>
    {
        protected override void Configure(IFlowBuilder<CancellationCheckRequest, CancellationCheckResponse> flow)
        {
            flow.Handle<CancellationCheckHandler>();
        }
    }

    /// <summary>
    /// Handler that calls ThrowIfCancellationRequested — models the defensive pattern
    /// recommended for long-running or frequently-invoked handlers.
    /// </summary>
    private sealed class CancellationCheckHandler
        : IFlowHandler<CancellationCheckRequest, CancellationCheckResponse>
    {
        public ValueTask<CancellationCheckResponse> HandleAsync(
            CancellationCheckRequest request, FlowContext context)
        {
            context.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new CancellationCheckResponse { Result = request.Value });
        }
    }
}
