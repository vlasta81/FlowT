using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace FlowT.Benchmarks;

/// <summary>
/// Extreme benchmarks testing FlowT performance under heavy load conditions.
/// Tests include: 10 specifications, 10 policies, 10 named keys, large payloads, and concurrent execution.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class ExtremePipelineBenchmarks
{
    private IServiceProvider _services = null!;
    
    // Flows
    private ExtremeFlow _extremeFlow = null!;
    private LargePayloadFlow _largePayloadFlow = null!;
    private ConcurrentFlow _concurrentFlow = null!;
    private DeepNestingFlow _deepNestingFlow = null!;
    
    private FlowContext _context = null!;
    private ExtremeRequest _extremeRequest = null!;
    private LargePayloadRequest _largePayloadRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register flows as singletons (production pattern)
        services.AddSingleton<ExtremeFlow>();
        services.AddSingleton<LargePayloadFlow>();
        services.AddSingleton<ConcurrentFlow>();
        services.AddSingleton<DeepNestingFlow>();

        _services = services.BuildServiceProvider();
        
        _extremeFlow = _services.GetRequiredService<ExtremeFlow>();
        _largePayloadFlow = _services.GetRequiredService<LargePayloadFlow>();
        _concurrentFlow = _services.GetRequiredService<ConcurrentFlow>();
        _deepNestingFlow = _services.GetRequiredService<DeepNestingFlow>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        // Pre-populate context with 10 named keys
        _context.Set("Value1", "key1");
        _context.Set(42, "key2");
        _context.Set(true, "key3");
        _context.Set(DateTime.UtcNow, "key4");
        _context.Set(new List<string> { "A", "B", "C" }, "key5");
        _context.Set(3.14159, "key6");
        _context.Set(Guid.NewGuid(), "key7");
        _context.Set(new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 }, "key8");
        _context.Set(TimeSpan.FromSeconds(100), "key9");
        _context.Set(new byte[] { 1, 2, 3, 4, 5 }, "key10");

        _extremeRequest = new ExtremeRequest 
        { 
            Value = "BenchmarkTest",
            Counter = 100
        };

        // Create large payload (10 MB of data)
        var largeString = new StringBuilder(10_000_000);
        for (int i = 0; i < 1_000_000; i++)
        {
            largeString.Append("Data");
            largeString.Append(i);
        }
        
        _largePayloadRequest = new LargePayloadRequest
        {
            LargeData = largeString.ToString(),
            Items = Enumerable.Range(0, 10_000).Select(i => $"Item_{i}").ToList()
        };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ============================================
    // EXTREME: 10 Specs + 10 Policies + 10 Keys
    // ============================================

    [Benchmark(Baseline = true, Description = "EXTREME: 10 specs + 10 policies + 10 named keys")]
    public Task<ExtremeResponse> Extreme_10Specs_10Policies_10Keys()
    {
        return _extremeFlow.ExecuteAsync(_extremeRequest, _context).AsTask();
    }

    // ============================================
    // LARGE PAYLOAD: Process 10 MB of data
    // ============================================

    [Benchmark(Description = "EXTREME: Large payload (10 MB + 10k items)")]
    public Task<LargePayloadResponse> Extreme_LargePayload()
    {
        return _largePayloadFlow.ExecuteAsync(_largePayloadRequest, _context).AsTask();
    }

    // ============================================
    // CONCURRENT: 100 parallel requests
    // ============================================

    [Benchmark(Description = "EXTREME: 100 concurrent executions")]
    public async Task Extreme_Concurrent100()
    {
        var tasks = new Task<ExtremeResponse>[100];
        
        for (int i = 0; i < 100; i++)
        {
            var localContext = new FlowContext
            {
                Services = _services,
                CancellationToken = CancellationToken.None
            };
            
            var request = new ExtremeRequest { Value = $"Test_{i}", Counter = i };
            tasks[i] = _concurrentFlow.ExecuteAsync(request, localContext).AsTask();
        }

        await Task.WhenAll(tasks);
    }

    // ============================================
    // DEEP NESTING: 10 policies + large payload
    // ============================================

    [Benchmark(Description = "EXTREME: Deep nesting (10 policies + 10 MB payload)")]
    public Task<LargePayloadResponse> Extreme_DeepNesting()
    {
        return _deepNestingFlow.ExecuteAsync(_largePayloadRequest, _context).AsTask();
    }
}

// ============================================
// Named Keys Constants
// ============================================

public static class ExtremeKeys
{
    public const string Key1 = "key1";
    public const string Key2 = "key2";
    public const string Key3 = "key3";
    public const string Key4 = "key4";
    public const string Key5 = "key5";
    public const string Key6 = "key6";
    public const string Key7 = "key7";
    public const string Key8 = "key8";
    public const string Key9 = "key9";
    public const string Key10 = "key10";
}

// ============================================
// Requests & Responses
// ============================================

public record ExtremeRequest
{
    public string Value { get; init; } = string.Empty;
    public int Counter { get; init; }
}

public record ExtremeResponse
{
    public string Result { get; init; } = string.Empty;
    public int ProcessedCount { get; init; }
    public bool AllChecksOk { get; init; }
}

public record LargePayloadRequest
{
    public string LargeData { get; init; } = string.Empty;
    public List<string> Items { get; init; } = new();
}

public record LargePayloadResponse
{
    public int DataLength { get; init; }
    public int ItemsProcessed { get; init; }
    public string Summary { get; init; } = string.Empty;
}

// ============================================
// 10 Specifications
// ============================================

public class ExtremeSpec1 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec2 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec3 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec4 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec5 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec6 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec7 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec8 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec9 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(null);
}

public class ExtremeSpec10 : IFlowSpecification<ExtremeRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(ExtremeRequest request, FlowContext context)
        => ValueTask.FromResult<FlowInterrupt<object?>?>(request.Counter < 0 
            ? FlowInterrupt<object?>.Fail("Counter must be non-negative") 
            : null);
}

// ============================================
// 10 Policies for ExtremeFlow
// ============================================

public class ExtremePolicy1 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set($"Policy1_{request.Value}", ExtremeKeys.Key1);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy2 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(request.Counter + 1, ExtremeKeys.Key2);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy3 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(true, ExtremeKeys.Key3);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy4 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(DateTime.UtcNow, ExtremeKeys.Key4);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy5 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        if (context.TryGet<List<string>>(out var list, ExtremeKeys.Key5))
        {
            list.Add("Policy5");
        }
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy6 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(Math.PI * request.Counter, ExtremeKeys.Key6);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy7 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(Guid.NewGuid(), ExtremeKeys.Key7);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy8 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        if (context.TryGet<Dictionary<string, int>>(out var dict, ExtremeKeys.Key8))
        {
            dict["policy8"] = request.Counter;
        }
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy9 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        context.Set(TimeSpan.FromMilliseconds(request.Counter), ExtremeKeys.Key9);
        return await Next.HandleAsync(request, context);
    }
}

public class ExtremePolicy10 : FlowPolicy<ExtremeRequest, ExtremeResponse>
{
    public override async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        var bytes = new byte[request.Counter % 100 + 1];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)(i % 256);
        context.Set(bytes, ExtremeKeys.Key10);
        return await Next.HandleAsync(request, context);
    }
}

// ============================================
// 10 Policies for Large Payload Flow
// ============================================

public class LargePayloadPolicy1 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override async ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
    {
        var length = request.LargeData.Length;
        context.Set($"Processed_{length}_bytes", "payload_key1");
        return await Next.HandleAsync(request, context);
    }
}

public class LargePayloadPolicy2 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override async ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
    {
        context.Set(request.Items.Count, "payload_key2");
        return await Next.HandleAsync(request, context);
    }
}

public class LargePayloadPolicy3 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy4 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy5 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy6 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy7 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy8 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy9 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

public class LargePayloadPolicy10 : FlowPolicy<LargePayloadRequest, LargePayloadResponse>
{
    public override ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
        => Next.HandleAsync(request, context);
}

// ============================================
// Handlers
// ============================================

public class ExtremeHandler : IFlowHandler<ExtremeRequest, ExtremeResponse>
{
    public ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        // Read all 10 named keys
        var key1 = context.TryGet<string>(out var v1, ExtremeKeys.Key1) ? v1 : "";
        var key2 = context.TryGet<int>(out var v2, ExtremeKeys.Key2) ? v2 : 0;
        var key3 = context.TryGet<bool>(out var v3, ExtremeKeys.Key3) && v3;
        var key4 = context.TryGet<DateTime>(out var v4, ExtremeKeys.Key4) ? v4 : DateTime.MinValue;
        var key5 = context.TryGet<List<string>>(out var v5, ExtremeKeys.Key5) ? v5.Count : 0;
        var key6 = context.TryGet<double>(out var v6, ExtremeKeys.Key6) ? v6 : 0;
        var key7 = context.TryGet<Guid>(out var v7, ExtremeKeys.Key7) ? v7 : Guid.Empty;
        var key8 = context.TryGet<Dictionary<string, int>>(out var v8, ExtremeKeys.Key8) ? v8.Count : 0;
        var key9 = context.TryGet<TimeSpan>(out var v9, ExtremeKeys.Key9) ? v9 : TimeSpan.Zero;
        var key10 = context.TryGet<byte[]>(out var v10, ExtremeKeys.Key10) ? v10.Length : 0;

        var response = new ExtremeResponse
        {
            Result = $"{request.Value}_Processed_{key1}",
            ProcessedCount = request.Counter + key2 + key5 + key8 + key10,
            AllChecksOk = key3 && key4 > DateTime.MinValue && key6 > 0 && key7 != Guid.Empty && key9 > TimeSpan.Zero
        };

        return ValueTask.FromResult(response);
    }
}

public class LargePayloadHandler : IFlowHandler<LargePayloadRequest, LargePayloadResponse>
{
    public ValueTask<LargePayloadResponse> HandleAsync(LargePayloadRequest request, FlowContext context)
    {
        // Process large data
        var dataLength = request.LargeData.Length;
        var itemsProcessed = 0;

        // Simulate processing each item
        foreach (var item in request.Items)
        {
            if (item.StartsWith("Item_"))
                itemsProcessed++;
        }

        var response = new LargePayloadResponse
        {
            DataLength = dataLength,
            ItemsProcessed = itemsProcessed,
            Summary = $"Processed {dataLength} bytes and {itemsProcessed} items"
        };

        return ValueTask.FromResult(response);
    }
}

public class ConcurrentHandler : IFlowHandler<ExtremeRequest, ExtremeResponse>
{
    public async ValueTask<ExtremeResponse> HandleAsync(ExtremeRequest request, FlowContext context)
    {
        // Simulate some async work
        await Task.Delay(1, context.CancellationToken);

        return new ExtremeResponse
        {
            Result = $"Concurrent_{request.Value}",
            ProcessedCount = request.Counter,
            AllChecksOk = true
        };
    }
}

// ============================================
// Flow Definitions
// ============================================

/// <summary>
/// Extreme flow with 10 specifications, 10 policies, reading 10 named keys
/// </summary>
public class ExtremeFlow : FlowDefinition<ExtremeRequest, ExtremeResponse>
{
    protected override void Configure(IFlowBuilder<ExtremeRequest, ExtremeResponse> flow)
    {
        flow
            // Add 10 specifications
            .Check<ExtremeSpec1>()
            .Check<ExtremeSpec2>()
            .Check<ExtremeSpec3>()
            .Check<ExtremeSpec4>()
            .Check<ExtremeSpec5>()
            .Check<ExtremeSpec6>()
            .Check<ExtremeSpec7>()
            .Check<ExtremeSpec8>()
            .Check<ExtremeSpec9>()
            .Check<ExtremeSpec10>()
            // Add 10 policies
            .Use<ExtremePolicy1>()
            .Use<ExtremePolicy2>()
            .Use<ExtremePolicy3>()
            .Use<ExtremePolicy4>()
            .Use<ExtremePolicy5>()
            .Use<ExtremePolicy6>()
            .Use<ExtremePolicy7>()
            .Use<ExtremePolicy8>()
            .Use<ExtremePolicy9>()
            .Use<ExtremePolicy10>()
            // Add handler
            .Handle<ExtremeHandler>();
    }
}

/// <summary>
/// Flow processing large payloads (10 MB data + 10k items)
/// </summary>
public class LargePayloadFlow : FlowDefinition<LargePayloadRequest, LargePayloadResponse>
{
    protected override void Configure(IFlowBuilder<LargePayloadRequest, LargePayloadResponse> flow)
    {
        flow.Handle<LargePayloadHandler>();
    }
}

/// <summary>
/// Simple flow for concurrent testing
/// </summary>
public class ConcurrentFlow : FlowDefinition<ExtremeRequest, ExtremeResponse>
{
    protected override void Configure(IFlowBuilder<ExtremeRequest, ExtremeResponse> flow)
    {
        flow.Handle<ConcurrentHandler>();
    }
}

/// <summary>
/// Deep nesting: 10 policies wrapping large payload processing
/// </summary>
public class DeepNestingFlow : FlowDefinition<LargePayloadRequest, LargePayloadResponse>
{
    protected override void Configure(IFlowBuilder<LargePayloadRequest, LargePayloadResponse> flow)
    {
        flow
            .Use<LargePayloadPolicy1>()
            .Use<LargePayloadPolicy2>()
            .Use<LargePayloadPolicy3>()
            .Use<LargePayloadPolicy4>()
            .Use<LargePayloadPolicy5>()
            .Use<LargePayloadPolicy6>()
            .Use<LargePayloadPolicy7>()
            .Use<LargePayloadPolicy8>()
            .Use<LargePayloadPolicy9>()
            .Use<LargePayloadPolicy10>()
            .Handle<LargePayloadHandler>();
    }
}
