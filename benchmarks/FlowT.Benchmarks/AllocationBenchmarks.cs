using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Allocation and throughput benchmarks for FlowT.
/// Focus on memory efficiency and high-throughput scenarios.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class AllocationBenchmarks
{
    private IServiceProvider _services = null!;
    private MinimalFlow _minimalFlow = null!;
    private FlowContext _context = null!;
    private MinimalRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MinimalFlow>();

        _services = services.BuildServiceProvider();
        _minimalFlow = _services.GetRequiredService<MinimalFlow>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _request = new MinimalRequest { Value = 42 };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    [Benchmark(Description = "Single execution")]
    public async Task<MinimalResponse> SingleExecution()
    {
        return await _minimalFlow.ExecuteAsync(_request, _context);
    }

    [Benchmark(Description = "10 sequential executions", OperationsPerInvoke = 10)]
    public async Task TenSequentialExecutions()
    {
        for (int i = 0; i < 10; i++)
        {
            await _minimalFlow.ExecuteAsync(_request, _context);
        }
    }

    [Benchmark(Description = "100 sequential executions", OperationsPerInvoke = 100)]
    public async Task HundredSequentialExecutions()
    {
        for (int i = 0; i < 100; i++)
        {
            await _minimalFlow.ExecuteAsync(_request, _context);
        }
    }

    [Benchmark(Description = "10 parallel executions", OperationsPerInvoke = 10)]
    public async Task TenParallelExecutions()
    {
        var tasks = new Task<MinimalResponse>[10];
        for (int i = 0; i < 10; i++)
        {
            // Create new context for each parallel execution
            var ctx = new FlowContext
            {
                Services = _services,
                CancellationToken = CancellationToken.None
            };
            tasks[i] = _minimalFlow.ExecuteAsync(_request, ctx).AsTask();
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Context creation only")]
    public FlowContext ContextCreation()
    {
        return new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };
    }

    [Benchmark(Description = "Context with 5 values")]
    public FlowContext ContextWithValues()
    {
        var ctx = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };
        ctx.Set("value1");
        ctx.Set(123);
        ctx.Set(true);
        ctx.Set(3.14);
        ctx.Set(new object());
        return ctx;
    }

    // Minimal types for low-allocation testing
    public record MinimalRequest
    {
        public int Value { get; init; }
    }

    public record MinimalResponse
    {
        public int Result { get; init; }
    }

    public class MinimalFlow : FlowDefinition<MinimalRequest, MinimalResponse>
    {
        protected override void Configure(IFlowBuilder<MinimalRequest, MinimalResponse> flow)
        {
            flow.Handle<MinimalHandler>();
        }
    }

    public class MinimalHandler : IFlowHandler<MinimalRequest, MinimalResponse>
    {
        public ValueTask<MinimalResponse> HandleAsync(MinimalRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new MinimalResponse { Result = request.Value * 2 });
        }
    }
}
