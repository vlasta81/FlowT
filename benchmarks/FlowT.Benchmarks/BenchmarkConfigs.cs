using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace FlowT.Benchmarks;

/// <summary>
/// Configuration for running benchmarks quickly during development.
/// Use this for rapid iteration without full statistical accuracy.
/// </summary>
public class DebugConfig : ManualConfig
{
    public DebugConfig()
    {
        AddJob(Job.ShortRun
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithWarmupCount(1)
            .WithIterationCount(3));
    }
}

/// <summary>
/// Configuration for production-quality benchmark results.
/// This is the default configuration with full statistical analysis.
/// </summary>
public class ProductionConfig : ManualConfig
{
    public ProductionConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(3)
            .WithIterationCount(5)
            .WithInvocationCount(1000));
    }
}
