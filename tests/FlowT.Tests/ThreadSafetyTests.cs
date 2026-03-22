using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace FlowT.Tests;

public class ThreadSafetyTests
{
    [Fact]
    public void FlowContext_Set_IsThreadSafe()
    {
        var context = CreateContext();
        const int threadCount = 20;
        const int operationsPerThread = 1000;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    context.Set($"Thread-{threadId}-{j}");
                }
            });
        }

        Task.WaitAll(tasks);

        // Should complete without exceptions
        Assert.True(context.TryGet<string>(out var value));
        Assert.NotNull(value);
    }

    [Fact]
    public void FlowContext_TryGet_IsThreadSafe()
    {
        var context = CreateContext();
        context.Set("InitialValue");

        const int threadCount = 20;
        const int operationsPerThread = 1000;
        var successCount = 0;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    if (context.TryGet<string>(out var value) && value == "InitialValue")
                    {
                        Interlocked.Increment(ref successCount);
                    }
                }
            });
        }

        Task.WaitAll(tasks);

        // All reads should succeed
        Assert.Equal(threadCount * operationsPerThread, successCount);
    }

    [Fact]
    public void FlowContext_GetOrAdd_IsThreadSafe()
    {
        var context = CreateContext();
        const int threadCount = 20;
        var creationCount = 0;

        var tasks = new Task<string>[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                return context.GetOrAdd(() =>
                {
                    Interlocked.Increment(ref creationCount);
                    Thread.Sleep(1); // Increase contention
                    return "CreatedValue";
                });
            });
        }

        Task.WaitAll(tasks);

        // Factory should be called only once despite high contention
        Assert.Equal(1, creationCount);

        // All tasks should receive the same value
        var firstValue = tasks[0].Result;
        Assert.All(tasks, t => Assert.Equal(firstValue, t.Result));
    }

    [Fact]
    public void FlowContext_Push_IsThreadSafe()
    {
        var context = CreateContext();
        context.Set("Original");

        const int threadCount = 10;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                using (context.Push($"Thread-{threadId}"))
                {
                    context.TryGet<string>(out var value);
                    Assert.StartsWith("Thread-", value);
                    Thread.Sleep(10); // Hold the scope
                }
            });
        }

        Task.WaitAll(tasks);

        // Original value should be restored (or last thread's value)
        Assert.True(context.TryGet<string>(out var final));
        Assert.NotNull(final);
    }

    [Fact]
    public void FlowContext_MixedOperations_AreThreadSafe()
    {
        var context = CreateContext();
        const int threadCount = 10;
        const int operationsPerThread = 100;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    // Mix of operations
                    switch (j % 4)
                    {
                        case 0:
                            context.Set($"Value-{threadId}-{j}");
                            break;
                        case 1:
                            context.TryGet<string>(out _);
                            break;
                        case 2:
                            context.GetOrAdd(() => $"GetOrAdd-{threadId}-{j}");
                            break;
                        case 3:
                            using (context.Push($"Push-{threadId}-{j}"))
                            {
                                Thread.Sleep(1);
                            }
                            break;
                    }
                }
            });
        }

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        // Should complete without exceptions or deadlocks
        Assert.Null(exception);
    }

    [Fact]
    public void FlowContext_StartTimer_IsThreadSafe()
    {
        var context = CreateContext();
        const int threadCount = 20;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                using (context.StartTimer($"timer-{threadId}"))
                {
                    Thread.Sleep(5);
                }
            });
        }

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        Assert.Null(exception);
    }

    [Fact]
    public async Task FlowDefinition_ConcurrentExecutions_AreThreadSafe()
    {
        var services = new ServiceCollection()
            .AddTransient<ConcurrentHandler>()
            .AddSingleton<ConcurrentFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<ConcurrentFlow>();

        const int concurrentExecutions = 50;
        var contexts = new FlowContext[concurrentExecutions];
        for (int i = 0; i < concurrentExecutions; i++)
        {
            contexts[i] = new FlowContext
            {
                Services = services,
                CancellationToken = CancellationToken.None
            };
        }

        var tasks = new Task<TestResponse>[concurrentExecutions];
        for (int i = 0; i < concurrentExecutions; i++)
        {
            int index = i;
            tasks[i] = Task.Run(async () =>
            {
                var request = new TestRequest { Value = $"Request-{index}" };
                return await flow.ExecuteAsync(request, contexts[index]);
            });
        }

        var results = await Task.WhenAll(tasks);

        // All executions should succeed
        Assert.Equal(concurrentExecutions, results.Length);
        Assert.All(results, r => Assert.NotNull(r.Message));
    }

    [Fact]
    public async Task FlowDefinition_SingletonInitialization_IsThreadSafe()
    {
        var services = new ServiceCollection()
            .AddTransient<ConcurrentHandler>()
            .AddSingleton<ConcurrentFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<ConcurrentFlow>();

        // Force concurrent initialization attempts
        const int concurrentAttempts = 20;
        var tasks = new Task<TestResponse>[concurrentAttempts];
        var barrier = new Barrier(concurrentAttempts);

        for (int i = 0; i < concurrentAttempts; i++)
        {
            int index = i;
            tasks[i] = Task.Run(async () =>
            {
                barrier.SignalAndWait(); // Synchronize all threads
                var context = new FlowContext
                {
                    Services = services,
                    CancellationToken = CancellationToken.None
                };
                return await flow.ExecuteAsync(new TestRequest { Value = $"Init-{index}" }, context);
            });
        }

        var results = await Task.WhenAll(tasks);

        // All should succeed without race conditions in initialization
        Assert.Equal(concurrentAttempts, results.Length);
        Assert.All(results, r => Assert.NotNull(r.Message));
    }

    [Fact]
    public void FlowContext_HighContentionScenario_Succeeds()
    {
        var context = CreateContext();
        const int threadCount = 50;
        const int operationsPerThread = 500;

        var exceptions = new ConcurrentBag<Exception>();

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        // Rapid fire operations
                        context.Set($"Value-{threadId}-{j}");
                        context.TryGet<string>(out _);

                        if (j % 10 == 0)
                        {
                            context.GetOrAdd(() => $"Lazy-{threadId}-{j}");
                        }

                        if (j % 20 == 0)
                        {
                            using (context.Push($"Scope-{threadId}-{j}"))
                            {
                                // Nested scope
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        Task.WaitAll(tasks);

        // No exceptions should occur
        Assert.Empty(exceptions);
    }

    // Helper methods
    private static FlowContext CreateContext()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        return new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };
    }

    // Helper types
    public record TestRequest
    {
        public string Value { get; init; } = "";
    }

    public record TestResponse
    {
        public string Message { get; init; } = "";
    }

    public class ConcurrentFlow : FlowDefinition<TestRequest, TestResponse>
    {
        protected override void Configure(IFlowBuilder<TestRequest, TestResponse> flow)
        {
            flow.Handle<ConcurrentHandler>();
        }
    }

    public class ConcurrentHandler : IFlowHandler<TestRequest, TestResponse>
    {
        private static int _callCount = 0;

        public async ValueTask<TestResponse> HandleAsync(TestRequest request, FlowContext context)
        {
            Interlocked.Increment(ref _callCount);
            await Task.Delay(1); // Simulate async work
            return new TestResponse { Message = $"Handled: {request.Value} (Call #{_callCount})" };
        }
    }
}
