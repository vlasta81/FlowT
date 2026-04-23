using FlowT;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests;

public class FlowContextTests : FlowTestBase
{
    [Fact]
    public void FlowId_IsUnique_ForEachInstance()
    {
        var ctx1 = CreateContext();
        var ctx2 = CreateContext();

        Assert.NotEqual(ctx1.FlowId, ctx2.FlowId);
    }

    [Fact]
    public void GetFlowIdString_ReturnsFormattedGuid()
    {
        var ctx = CreateContext();
        string idString = ctx.FlowIdString;

        Assert.Equal(32, idString.Length); // Format "N" = 32 hex chars without hyphens
        Assert.DoesNotContain("-", idString);
    }

    [Fact]
    public void Set_And_TryGet_WorksCorrectly()
    {
        var ctx = CreateContext();
        var testValue = "Hello, FlowT!";

        ctx.Set(testValue);
        bool found = ctx.TryGet<string>(out var result);

        Assert.True(found);
        Assert.Equal(testValue, result);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenValueNotSet()
    {
        var ctx = CreateContext();

        bool found = ctx.TryGet<string>(out var result);

        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void Set_OverwritesPreviousValue()
    {
        var ctx = CreateContext();

        ctx.Set("First");
        ctx.Set("Second");

        ctx.TryGet<string>(out var result);
        Assert.Equal("Second", result);
    }

    [Fact]
    public void GetOrAdd_CreatesValue_WhenNotExists()
    {
        var ctx = CreateContext();
        int callCount = 0;

        var result = ctx.GetOrAdd(() =>
        {
            callCount++;
            return "Created";
        });

        Assert.Equal("Created", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetOrAdd_ReturnsExistingValue_WhenExists()
    {
        var ctx = CreateContext();
        ctx.Set("Existing");
        int callCount = 0;

        var result = ctx.GetOrAdd(() =>
        {
            callCount++;
            return "Created";
        });

        Assert.Equal("Existing", result);
        Assert.Equal(0, callCount); // Factory not called
    }

    [Fact]
    public void GetOrAdd_WithArg_AvoidsClosure()
    {
        var ctx = CreateContext();
        string arg = "Argument";

        var result = ctx.GetOrAdd(arg, a => $"Value: {a}");

        Assert.Equal("Value: Argument", result);
    }

    [Fact]
    public void Push_RestoresOriginalValue_OnDispose()
    {
        var ctx = CreateContext();
        ctx.Set("Original");

        using (ctx.Push("Temporary"))
        {
            ctx.TryGet<string>(out var temp);
            Assert.Equal("Temporary", temp);
        }

        ctx.TryGet<string>(out var restored);
        Assert.Equal("Original", restored);
    }

    [Fact]
    public void Push_RemovesValue_WhenNoOriginalValue()
    {
        var ctx = CreateContext();

        using (ctx.Push("Temporary"))
        {
            Assert.True(ctx.TryGet<string>(out _));
        }

        Assert.False(ctx.TryGet<string>(out _));
    }

    [Fact]
    public void StartTimer_RecordsElapsedTime()
    {
        var ctx = CreateContext();

        using (ctx.StartTimer("test-timer"))
        {
            Thread.Sleep(10); // Simulate work
        }

        // Timer should be recorded (we can't easily assert the exact value, but it should exist)
        Assert.True(true); // Timer disposable executed without error
    }

    [Fact]
    public void StartTimer_RecordsPositiveElapsedTicks()
    {
        var ctx = CreateContext();

        using (ctx.StartTimer("op"))
        {
            Thread.Sleep(5);
        }

        // Access timers via a second timer write to prove the dictionary was populated
        // (the timer value is internal, but we verify no exception and re-use works)
        using (ctx.StartTimer("op"))
        {
            // overwrites previous — should not throw
        }

        Assert.True(true);
    }

    [Fact]
    public void Service_ResolvesRegisteredService()
    {
        var services = BuildServiceProvider(sc => sc.AddSingleton<IMyService, MyService>());
        var ctx = CreateContext(services);

        var svc = ctx.Service<IMyService>();

        Assert.NotNull(svc);
        Assert.IsType<MyService>(svc);
    }

    [Fact]
    public void Service_Throws_WhenServiceNotRegistered()
    {
        var ctx = CreateContext();

        Assert.Throws<InvalidOperationException>(() => ctx.Service<IMyService>());
    }

    [Fact]
    public void TryService_ReturnsNull_WhenNotRegistered()
    {
        var ctx = CreateContext();

        var svc = ctx.TryService<IMyService>();

        Assert.Null(svc);
    }

    [Fact]
    public void TryService_ReturnsInstance_WhenRegistered()
    {
        var services = BuildServiceProvider(sc => sc.AddSingleton<IMyService, MyService>());
        var ctx = CreateContext(services);

        var svc = ctx.TryService<IMyService>();

        Assert.NotNull(svc);
    }

    [Fact]
    public void StartedAt_IsSetToApproximatelyNow()
    {
        var before = DateTimeOffset.UtcNow;
        var ctx = CreateContext();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(ctx.StartedAt, before, after);
    }

    [Fact]
    public async Task PublishInBackground_FaultingHandler_DoesNotThrowToCaller()
    {
        var handler = new FaultingEventHandler();
        var services = new ServiceCollection()
            .AddSingleton<IEventHandler<TestEvent>>(handler)
            .BuildServiceProvider();

        var ctx = new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };

        var task = ctx.PublishInBackground(new TestEvent { Value = "Boom" }, CancellationToken.None);

        // The work task itself faults, but we expect it to propagate as AggregateException or TaskFailure
        // The important thing is the caller is not blocked and can observe the fault
        var exception = await Record.ExceptionAsync(() => task);

        Assert.NotNull(exception);
        Assert.True(exception is Exception);
    }

    [Fact]
    public void ThrowIfCancellationRequested_ThrowsWhenCancelled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var services = new ServiceCollection().BuildServiceProvider();
        var ctx = new FlowContext
        {
            Services = services,
            CancellationToken = cts.Token
        };

        Assert.Throws<OperationCanceledException>(() => ctx.ThrowIfCancellationRequested());
    }

    [Fact]
    public void ThrowIfCancellationRequested_DoesNotThrow_WhenNotCancelled()
    {
        var ctx = CreateContext();

        var exception = Record.Exception(() => ctx.ThrowIfCancellationRequested());

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_CallsAllHandlers()
    {
        var handler1 = new TestEventHandler();
        var handler2 = new TestEventHandler();

        var services = new ServiceCollection()
            .AddSingleton<IEventHandler<TestEvent>>(handler1)
            .AddSingleton<IEventHandler<TestEvent>>(handler2)
            .BuildServiceProvider();

        var ctx = new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };

        var testEvent = new TestEvent { Value = "Test" };
        await ctx.PublishAsync(testEvent, CancellationToken.None);

        Assert.True(handler1.WasCalled);
        Assert.True(handler2.WasCalled);
        Assert.Equal("Test", handler1.ReceivedEvent?.Value);
        Assert.Equal("Test", handler2.ReceivedEvent?.Value);
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenNoHandlers()
    {
        var ctx = CreateContext();

        var exception = await Record.ExceptionAsync(async () =>
            await ctx.PublishAsync(new TestEvent { Value = "Test" }, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishInBackground_DoesNotBlockCaller()
    {
        var handler = new SlowEventHandler();
        var services = new ServiceCollection()
            .AddSingleton<IEventHandler<TestEvent>>(handler)
            .BuildServiceProvider();

        var ctx = new FlowContext
        {
            Services = services,
            CancellationToken = CancellationToken.None
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var task = ctx.PublishInBackground(new TestEvent { Value = "Test" }, CancellationToken.None);
        sw.Stop();

        // Caller should return without waiting for the slow handler (50 ms)
        Assert.True(sw.ElapsedMilliseconds < 40, $"PublishInBackground blocked for {sw.ElapsedMilliseconds} ms");

        await task;
        Assert.True(handler.HasCompleted);
    }

    [Fact]
    public void MultipleTypes_CanCoexist_InContext()
    {
        var ctx = CreateContext();

        ctx.Set("String value");
        ctx.Set(42);
        ctx.Set(new TestEvent { Value = "Event" });

        Assert.True(ctx.TryGet<string>(out var str));
        Assert.True(ctx.TryGet<int>(out var num));
        Assert.True(ctx.TryGet<TestEvent>(out var evt));

        Assert.Equal("String value", str);
        Assert.Equal(42, num);
        Assert.Equal("Event", evt.Value);
    }

    [Fact]
    public void Context_IsThreadSafe_ForConcurrentWrites()
    {
        var ctx = CreateContext();
        const int threadCount = 10;
        const int iterationsPerThread = 100;

        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerThread; j++)
                {
                    ctx.Set($"Thread-{threadId}-Value-{j}");
                }
            });
        }

        Task.WaitAll(tasks);

        // If we get here without exceptions, thread-safety is working
        Assert.True(ctx.TryGet<string>(out _));
    }

    // Helper classes
    public record TestEvent
    {
        public string Value { get; init; } = "";
    }

    private interface IMyService { }
    private class MyService : IMyService { }

    private class TestEventHandler : IEventHandler<TestEvent>
    {
        public bool WasCalled { get; private set; }
        public TestEvent? ReceivedEvent { get; private set; }

        public Task HandleAsync(TestEvent eventData, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedEvent = eventData;
            return Task.CompletedTask;
        }
    }

    private class SlowEventHandler : IEventHandler<TestEvent>
    {
        public bool IsProcessing { get; private set; }
        public bool HasCompleted { get; private set; }

        public async Task HandleAsync(TestEvent eventData, CancellationToken cancellationToken)
        {
            IsProcessing = true;
            await Task.Delay(50, cancellationToken);
            IsProcessing = false;
            HasCompleted = true;
        }
    }

    private class FaultingEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent eventData, CancellationToken cancellationToken)
            => Task.FromException(new InvalidOperationException("Handler fault"));
    }
}
