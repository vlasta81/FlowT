using BenchmarkDotNet.Attributes;
using FlowT;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks for FlowContext.StartTimer.
/// Measures the cost of starting a timer (Stopwatch.GetTimestamp), recording elapsed
/// time on Dispose, and accessing stored timer values.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class TimerBenchmarks
{
    private FlowContext _context = null!;

    [GlobalSetup]
    public void Setup()
    {
        _context = new FlowContext { Services = null!, CancellationToken = CancellationToken.None };

        // Pre-warm: ensure the timers dictionary is already allocated in the warm context
        using (_context.StartTimer("warm-up"))
        {
            // intentionally empty
        }
    }

    // ── Single timer ─────────────────────────────────────────────────────────

    /// <summary>
    /// Cold path: first timer written into a freshly created context (timers dict must be allocated).
    /// </summary>
    [Benchmark(Baseline = true, Description = "StartTimer - first timer (cold, dict allocation)")]
    public void StartTimer_Cold()
    {
        var ctx = new FlowContext { Services = null!, CancellationToken = CancellationToken.None };
        using (ctx.StartTimer("op"))
        {
            // zero-work body
        }
    }

    /// <summary>
    /// Warm path: timer written into a context that already has the timers dict allocated.
    /// </summary>
    [Benchmark(Description = "StartTimer - subsequent timer (warm, dict exists)")]
    public void StartTimer_Warm()
    {
        using (_context.StartTimer("op"))
        {
            // zero-work body
        }
    }

    /// <summary>
    /// Two distinct timers on the same context — measures additive cost of a second entry.
    /// </summary>
    [Benchmark(Description = "StartTimer - 2 sequential timers")]
    public void StartTimer_Two_Sequential()
    {
        using (_context.StartTimer("phase1"))
        {
            // intentionally empty
        }
        using (_context.StartTimer("phase2"))
        {
            // intentionally empty
        }
    }

    /// <summary>
    /// Five nested timers on the same context — models layered stage timing in a pipeline.
    /// </summary>
    [Benchmark(Description = "StartTimer - 5 nested timers")]
    public void StartTimer_Five_Nested()
    {
        using (_context.StartTimer("t1"))
        using (_context.StartTimer("t2"))
        using (_context.StartTimer("t3"))
        using (_context.StartTimer("t4"))
        using (_context.StartTimer("t5"))
        {
            // intentionally empty
        }
    }

    /// <summary>
    /// Overwriting the same key repeatedly — models a retry loop that re-times each attempt.
    /// </summary>
    [Benchmark(Description = "StartTimer - overwrite same key (5 times)", OperationsPerInvoke = 5)]
    public void StartTimer_Overwrite_SameKey()
    {
        for (int i = 0; i < 5; i++)
        {
            using (_context.StartTimer("attempt"))
            {
                // intentionally empty
            }
        }
    }
}
