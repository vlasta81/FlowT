using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for the FlowT plugin system.
/// Measures Plugin&lt;T&gt;() resolution cost on cold (first access per FlowContext) and warm
/// (cached lookup) paths, the overhead of FlowPlugin context binding via Initialize,
/// and the end-to-end cost of a pipeline where multiple stages share a single plugin instance.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class PluginBenchmarks
{
    private IServiceProvider _services = null!;

    /// <summary>Pre-seeded context: all three plugin types already cached.</summary>
    private FlowContext _warmContext = null!;

    private PluginAwareFlow _pluginAwareFlow = null!;
    private PlainFlow _plainFlow = null!;
    private BenchmarkPluginRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // FlowPlugin subclasses (get Initialize called)
        services.AddFlowPlugin<IMetricPlugin, MetricPlugin>();
        services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();

        // Plain plugin — no FlowPlugin base, no Initialize call
        services.AddFlowPlugin<IPlainPlugin, PlainPlugin>();

        // Pipeline flows
        services.AddSingleton<PluginAwareFlow>();
        services.AddSingleton<PlainFlow>();

        _services = services.BuildServiceProvider();
        _request = new BenchmarkPluginRequest { Value = "benchmark" };
        _pluginAwareFlow = _services.GetRequiredService<PluginAwareFlow>();
        _plainFlow = _services.GetRequiredService<PlainFlow>();

        // Warm context: all plugins pre-resolved so every warm benchmark hits the cache
        _warmContext = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        _ = _warmContext.Plugin<IMetricPlugin>();
        _ = _warmContext.Plugin<IAuditPlugin>();
        _ = _warmContext.Plugin<IPlainPlugin>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ── FlowPlugin subclass — cold vs. warm ──────────────────────────────────

    /// <summary>
    /// Baseline: first Plugin&lt;T&gt;() call on a fresh FlowContext.
    /// Cost includes: DI resolution, is-check, Initialize call, and dict write.
    /// </summary>
    [Benchmark(Baseline = true, Description = "Plugin<T>() first access - FlowPlugin (cold)")]
    public IMetricPlugin Plugin_FlowPlugin_Cold()
    {
        var ctx = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        return ctx.Plugin<IMetricPlugin>();
    }

    /// <summary>
    /// Warm path: plugin already in the PerFlow cache — only a locked dict lookup.
    /// </summary>
    [Benchmark(Description = "Plugin<T>() cached access - FlowPlugin (warm)")]
    public IMetricPlugin Plugin_FlowPlugin_Warm()
    {
        return _warmContext.Plugin<IMetricPlugin>();
    }

    // ── Plain plugin (no FlowPlugin base) — cold vs. warm ───────────────────

    /// <summary>
    /// First Plugin&lt;T&gt;() call for a plain plugin — no Initialize overhead.
    /// Compares with FlowPlugin cold path to isolate the Initialize cost.
    /// </summary>
    [Benchmark(Description = "Plugin<T>() first access - plain plugin, no Initialize (cold)")]
    public IPlainPlugin Plugin_Plain_Cold()
    {
        var ctx = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        return ctx.Plugin<IPlainPlugin>();
    }

    /// <summary>
    /// Warm cached lookup for a plain plugin.
    /// </summary>
    [Benchmark(Description = "Plugin<T>() cached access - plain plugin (warm)")]
    public IPlainPlugin Plugin_Plain_Warm()
    {
        return _warmContext.Plugin<IPlainPlugin>();
    }

    // ── Multiple distinct plugin types — cold vs. warm ───────────────────────

    /// <summary>
    /// Resolves three distinct plugin types on a single fresh FlowContext.
    /// Represents the typical startup cost at the beginning of a flow execution.
    /// </summary>
    [Benchmark(Description = "3 Plugin<T>() types - all first access (cold)")]
    public int Plugin_ThreeDistinctTypes_Cold()
    {
        var ctx = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        _ = ctx.Plugin<IMetricPlugin>();
        _ = ctx.Plugin<IAuditPlugin>();
        _ = ctx.Plugin<IPlainPlugin>();
        return 3;
    }

    /// <summary>
    /// Three cached lookups on the same warm context.
    /// Represents repeated plugin access across pipeline stages of a single flow.
    /// </summary>
    [Benchmark(Description = "3 Plugin<T>() types - all cached (warm)")]
    public int Plugin_ThreeDistinctTypes_Warm()
    {
        _ = _warmContext.Plugin<IMetricPlugin>();
        _ = _warmContext.Plugin<IAuditPlugin>();
        _ = _warmContext.Plugin<IPlainPlugin>();
        return 3;
    }

    // ── Pipeline integration — with and without plugin ───────────────────────

    /// <summary>
    /// Full flow execution without any plugin usage.
    /// Baseline for pipeline-level plugin overhead comparisons.
    /// </summary>
    [Benchmark(Description = "Pipeline execution - no plugin (baseline)")]
    public async Task<BenchmarkPluginResponse> Pipeline_WithoutPlugin()
    {
        var ctx = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        return await _plainFlow.ExecuteAsync(_request, ctx);
    }

    /// <summary>
    /// Full flow execution where both the policy and the handler call Plugin&lt;T&gt;().
    /// The policy gets a cold resolution; the handler gets a warm (cached) resolution.
    /// Represents a realistic real-world PerFlow plugin usage pattern.
    /// </summary>
    [Benchmark(Description = "Pipeline execution - policy + handler share plugin (one cold, one warm)")]
    public async Task<BenchmarkPluginResponse> Pipeline_WithPlugin()
    {
        var ctx = new FlowContext { Services = _services, CancellationToken = CancellationToken.None };
        return await _pluginAwareFlow.ExecuteAsync(_request, ctx);
    }

    /// <summary>
    /// Pipeline execution with a pre-warmed context.
    /// Both plugin accesses (policy + handler) are cache hits.
    /// Simulates later stages in a multi-stage flow where the plugin was already resolved upstream.
    /// </summary>
    [Benchmark(Description = "Pipeline execution - plugin pre-warmed in context (both warm)")]
    public async Task<BenchmarkPluginResponse> Pipeline_WithPlugin_WarmContext()
    {
        return await _pluginAwareFlow.ExecuteAsync(_request, _warmContext);
    }

    // ── Plugin types ─────────────────────────────────────────────────────────

    public interface IMetricPlugin
    {
        void RecordQuery(long elapsed);
    }

    /// <summary>FlowPlugin subclass — binds FlowContext via Initialize.</summary>
    public class MetricPlugin : FlowPlugin, IMetricPlugin
    {
        private long _totalElapsed;

        public void RecordQuery(long elapsed) => _totalElapsed += elapsed;
    }

    public interface IAuditPlugin
    {
        void Record(string action);
    }

    /// <summary>Second FlowPlugin subclass for multi-type benchmarks.</summary>
    public class AuditPlugin : FlowPlugin, IAuditPlugin
    {
        private int _count;

        public void Record(string action) => _count++;
    }

    public interface IPlainPlugin
    {
        string GetValue();
    }

    /// <summary>Plain plugin — no FlowPlugin base, no Initialize overhead.</summary>
    public class PlainPlugin : IPlainPlugin
    {
        public string GetValue() => "value";
    }

    // ── Request / Response ───────────────────────────────────────────────────

    public record BenchmarkPluginRequest
    {
        public string Value { get; init; } = "";
    }

    public record BenchmarkPluginResponse
    {
        public string Result { get; init; } = "";
    }

    // ── Flow definitions ─────────────────────────────────────────────────────

    /// <summary>Flow where policy and handler both access the same plugin instance.</summary>
    public class PluginAwareFlow : FlowDefinition<BenchmarkPluginRequest, BenchmarkPluginResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkPluginRequest, BenchmarkPluginResponse> flow)
        {
            flow.Use<MetricPolicy>().Handle<MetricHandler>();
        }
    }

    /// <summary>Baseline flow — no plugin usage.</summary>
    public class PlainFlow : FlowDefinition<BenchmarkPluginRequest, BenchmarkPluginResponse>
    {
        protected override void Configure(IFlowBuilder<BenchmarkPluginRequest, BenchmarkPluginResponse> flow)
        {
            flow.Handle<PlainHandler>();
        }
    }

    public class MetricPolicy : FlowPolicy<BenchmarkPluginRequest, BenchmarkPluginResponse>
    {
        public override async ValueTask<BenchmarkPluginResponse> HandleAsync(
            BenchmarkPluginRequest request, FlowContext context)
        {
            // First access per context — cold resolution
            context.Plugin<IMetricPlugin>().RecordQuery(1);
            return await Next.HandleAsync(request, context);
        }
    }

    public class MetricHandler : IFlowHandler<BenchmarkPluginRequest, BenchmarkPluginResponse>
    {
        public ValueTask<BenchmarkPluginResponse> HandleAsync(
            BenchmarkPluginRequest request, FlowContext context)
        {
            // Second access per context — warm (cached) resolution
            context.Plugin<IMetricPlugin>().RecordQuery(2);
            return ValueTask.FromResult(new BenchmarkPluginResponse { Result = request.Value });
        }
    }

    public class PlainHandler : IFlowHandler<BenchmarkPluginRequest, BenchmarkPluginResponse>
    {
        public ValueTask<BenchmarkPluginResponse> HandleAsync(
            BenchmarkPluginRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new BenchmarkPluginResponse { Result = request.Value });
        }
    }
}
