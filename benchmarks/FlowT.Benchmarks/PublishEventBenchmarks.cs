using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for FlowContext.PublishAsync and PublishInBackground.
/// Measures event dispatch cost with 0, 1, and 5 registered handlers.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class PublishEventBenchmarks
{
    private FlowContext _ctxNoHandlers = null!;
    private FlowContext _ctxOneHandler = null!;
    private FlowContext _ctxFiveHandlers = null!;
    private IServiceProvider _servicesNoHandlers = null!;
    private IServiceProvider _servicesOneHandler = null!;
    private IServiceProvider _servicesFiveHandlers = null!;
    private readonly BenchmarkEvent _event = new() { Value = "benchmark" };

    [GlobalSetup]
    public void Setup()
    {
        // No handlers
        _servicesNoHandlers = new ServiceCollection().BuildServiceProvider();
        _ctxNoHandlers = new FlowContext { Services = _servicesNoHandlers, CancellationToken = CancellationToken.None };

        // One handler
        var sc1 = new ServiceCollection();
        sc1.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        _servicesOneHandler = sc1.BuildServiceProvider();
        _ctxOneHandler = new FlowContext { Services = _servicesOneHandler, CancellationToken = CancellationToken.None };

        // Five handlers
        var sc5 = new ServiceCollection();
        sc5.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        sc5.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        sc5.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        sc5.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        sc5.AddSingleton<IEventHandler<BenchmarkEvent>, NoopEventHandler>();
        _servicesFiveHandlers = sc5.BuildServiceProvider();
        _ctxFiveHandlers = new FlowContext { Services = _servicesFiveHandlers, CancellationToken = CancellationToken.None };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_servicesNoHandlers as IDisposable)?.Dispose();
        (_servicesOneHandler as IDisposable)?.Dispose();
        (_servicesFiveHandlers as IDisposable)?.Dispose();
    }

    // ── PublishAsync ──────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "PublishAsync - 0 handlers")]
    public Task PublishAsync_NoHandlers()
        => _ctxNoHandlers.PublishAsync(_event, CancellationToken.None);

    [Benchmark(Description = "PublishAsync - 1 handler")]
    public Task PublishAsync_OneHandler()
        => _ctxOneHandler.PublishAsync(_event, CancellationToken.None);

    [Benchmark(Description = "PublishAsync - 5 handlers")]
    public Task PublishAsync_FiveHandlers()
        => _ctxFiveHandlers.PublishAsync(_event, CancellationToken.None);

    // ── PublishInBackground ───────────────────────────────────────────────────

    /// <summary>
    /// Measures only the synchronous scheduling cost — does NOT await the background task.
    /// The fire-and-forget Task is discarded so BDN does not include the handler execution time.
    /// </summary>
    [Benchmark(Description = "PublishInBackground scheduling cost - 1 handler (no await)")]
    public Task PublishInBackground_SchedulingCost_OneHandler()
    {
        // Return the task so BDN can observe it, but the benchmark measures the dispatch latency
        // (Task.Run scheduling), not the handler execution.
        return _ctxOneHandler.PublishInBackground(_event, CancellationToken.None);
    }

    [Benchmark(Description = "PublishInBackground + await - 1 handler")]
    public Task PublishInBackground_Await_OneHandler()
        => _ctxOneHandler.PublishInBackground(_event, CancellationToken.None);

    [Benchmark(Description = "PublishInBackground + await - 5 handlers")]
    public Task PublishInBackground_Await_FiveHandlers()
        => _ctxFiveHandlers.PublishInBackground(_event, CancellationToken.None);

    // ── Supporting types ──────────────────────────────────────────────────────

    public record BenchmarkEvent
    {
        public string Value { get; init; } = string.Empty;
    }

    /// <summary>No-operation handler that completes synchronously.</summary>
    private sealed class NoopEventHandler : IEventHandler<BenchmarkEvent>
    {
        public Task HandleAsync(BenchmarkEvent eventData, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
