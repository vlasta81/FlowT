using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace FlowT.Tests;

/// <summary>
/// Tests to verify that FlowContext data is properly isolated between concurrent requests.
/// These tests ensure that the singleton pattern for handlers does not cause data leaks
/// between different flow executions happening in parallel.
/// </summary>
public class ConcurrencyAndIsolationTests : FlowTestBase
{
    /// <summary>
    /// Verifies that each flow execution gets a unique FlowId,
    /// even when executed concurrently.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_HaveUniqueFlowIds()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<FlowIdCapturingHandler>()
            .AddSingleton<FlowIdCapturingFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowIdCapturingFlow>();
        var flowIds = new ConcurrentBag<Guid>();

        // Act - Execute 100 flows in parallel
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            var result = await flow.ExecuteAsync(new SimpleRequest { Value = $"Request-{i}" }, httpContext);
            flowIds.Add(result.FlowId);
        });

        await Task.WhenAll(tasks);

        // Assert - All FlowIds must be unique
        Assert.Equal(100, flowIds.Count);
        Assert.Equal(100, flowIds.Distinct().Count());
    }

    /// <summary>
    /// Verifies that FlowContext.Set/TryGet data is isolated between concurrent requests.
    /// Data stored in one flow must not be visible in another flow.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_HaveIsolatedContextData()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<ContextDataHandler>()
            .AddSingleton<ContextDataFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<ContextDataFlow>();
        var results = new ConcurrentBag<SimpleResponse>();

        // Act - Each request stores and retrieves its own unique value
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            var result = await flow.ExecuteAsync(new SimpleRequest { Value = $"Value-{i}" }, httpContext);
            results.Add(result);
        });

        await Task.WhenAll(tasks);

        // Assert - Each flow must have retrieved its own value
        Assert.Equal(100, results.Count);
        for (int i = 0; i < 100; i++)
        {
            var expected = $"Retrieved: Value-{i}";
            Assert.Contains(results, r => r.Message == expected);
        }
    }

    /// <summary>
    /// Verifies that HttpContext data is isolated between concurrent requests.
    /// Each request must see only its own HttpContext, not others.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_HaveIsolatedHttpContextData()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<HttpContextDataHandler>()
            .AddSingleton<HttpContextDataFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<HttpContextDataFlow>();
        var results = new ConcurrentBag<SimpleResponse>();

        // Act - Each request has a unique header value
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            httpContext.Request.Headers["X-Request-Id"] = $"Request-{i}";

            var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);
            results.Add(result);
        });

        await Task.WhenAll(tasks);

        // Assert - Each flow must have read its own header value
        Assert.Equal(100, results.Count);
        for (int i = 0; i < 100; i++)
        {
            var expected = $"Header: Request-{i}";
            Assert.Contains(results, r => r.Message == expected);
        }
    }

    /// <summary>
    /// Verifies that authenticated user data is isolated between concurrent requests.
    /// Each request must see only its own user principal, not others.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_HaveIsolatedUserData()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<UserDataHandler>()
            .AddSingleton<UserDataFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<UserDataFlow>();
        var results = new ConcurrentBag<SimpleResponse>();

        // Act - Each request has a different authenticated user
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            httpContext.User = CreateUser($"user-{i}", $"user{i}@example.com");

            var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);
            results.Add(result);
        });

        await Task.WhenAll(tasks);

        // Assert - Each flow must have read its own user data
        Assert.Equal(100, results.Count);
        for (int i = 0; i < 100; i++)
        {
            var expected = $"UserId: user-{i}";
            Assert.Contains(results, r => r.Message == expected);
        }
    }

    /// <summary>
    /// Stress test with high concurrency to detect race conditions.
    /// Verifies that handlers (which are singletons) don't share mutable state
    /// that could cause data corruption under load.
    /// </summary>
    [Fact]
    public async Task StressTest_HighConcurrency_NoDataCorruption()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<StressTestHandler>()
            .AddSingleton<StressTestFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<StressTestFlow>();
        var results = new ConcurrentBag<SimpleResponse>();
        const int RequestCount = 1000;

        // Act - Execute 1000 flows concurrently
        var tasks = Enumerable.Range(0, RequestCount).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            httpContext.Request.Headers["X-Request-Number"] = i.ToString();
            httpContext.User = CreateUser($"user-{i}", $"user{i}@example.com");

            // Simulate some processing time
            await Task.Delay(Random.Shared.Next(0, 5));

            var result = await flow.ExecuteAsync(
                new SimpleRequest { Value = $"Request-{i}" },
                httpContext);

            results.Add(result);
        });

        await Task.WhenAll(tasks);

        // Assert - All requests must complete successfully with correct data
        Assert.Equal(RequestCount, results.Count);

        // Verify no data corruption - each result must match its input
        for (int i = 0; i < RequestCount; i++)
        {
            var expected = $"Processed-{i}|user-{i}|Request-{i}";
            Assert.Contains(results, r => r.Message.Contains(expected));
        }
    }

    /// <summary>
    /// Verifies that sub-flows share the parent's FlowContext correctly
    /// even when parent flows are executed concurrently.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_WithSubFlows_ShareParentContext()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<ParentWithSubFlowHandler>()
            .AddSingleton<SubFlowHandler>()
            .AddSingleton<ParentWithSubFlow>()
            .AddSingleton<SubFlow>()
            .BuildServiceProvider();

        var parentFlow = services.GetRequiredService<ParentWithSubFlow>();
        var results = new ConcurrentBag<SimpleResponse>();

        // Act - Each parent flow stores a value and calls sub-flow
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            var httpContext = CreateHttpContext(services);
            var result = await parentFlow.ExecuteAsync(
                new SimpleRequest { Value = $"Parent-{i}" },
                httpContext);
            results.Add(result);
        });

        await Task.WhenAll(tasks);

        // Assert - Each sub-flow must have seen its parent's value
        Assert.Equal(50, results.Count);
        for (int i = 0; i < 50; i++)
        {
            var expected = $"Parent-{i}|SubFlow-Parent-{i}";
            Assert.Contains(results, r => r.Message == expected);
        }
    }

    /// <summary>
    /// Verifies that CancellationToken is properly isolated between requests.
    /// Cancelling one request must not affect other concurrent requests.
    /// </summary>
    [Fact]
    public async Task ParallelFlows_WithCancellation_IsolatesCancellationTokens()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<CancellableHandler>()
            .AddSingleton<CancellableFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<CancellableFlow>();
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();
        var cts3 = new CancellationTokenSource();

        // Act - Start 3 flows, cancel only the second one
        var task1 = flow.ExecuteAsync(
            new SimpleRequest { Value = "Request-1" },
            services,
            cts1.Token);

        var task2 = flow.ExecuteAsync(
            new SimpleRequest { Value = "Request-2" },
            services,
            cts2.Token);

        var task3 = flow.ExecuteAsync(
            new SimpleRequest { Value = "Request-3" },
            services,
            cts3.Token);

        // Cancel only the second request
        cts2.Cancel();

        await Task.Delay(100); // Give it time to process

        // Assert
        var result1 = await task1;
        Assert.Equal("Completed: Request-1", result1.Message);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task2);

        var result3 = await task3;
        Assert.Equal("Completed: Request-3", result3.Message);
    }

    #region Test Helpers

    private static DefaultHttpContext CreateHttpContext(IServiceProvider services)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        httpContext.Request.Protocol = "HTTP/1.1";
        httpContext.Request.Method = "GET";
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/api/test";

        return httpContext;
    }

    private static ClaimsPrincipal CreateUser(string userId, string email, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    #endregion

    #region Test Flows and Handlers

    public class FlowIdCapturingFlow : FlowDefinition<SimpleRequest, FlowIdResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, FlowIdResponse> flow)
        {
            flow.Handle<FlowIdCapturingHandler>();
        }
    }

    public class FlowIdCapturingHandler : IFlowHandler<SimpleRequest, FlowIdResponse>
    {
        public ValueTask<FlowIdResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            return new ValueTask<FlowIdResponse>(new FlowIdResponse
            {
                FlowId = context.FlowId,
                Message = $"FlowId: {context.FlowId}"
            });
        }
    }

    public class ContextDataFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<ContextDataHandler>();
        }
    }

    public class ContextDataHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Store the request value
            context.Set(request.Value);

            // Simulate some processing
            Task.Delay(Random.Shared.Next(1, 10)).GetAwaiter().GetResult();

            // Retrieve the value
            context.TryGet<string>(out var retrieved);

            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Retrieved: {retrieved}"
            });
        }
    }

    public class HttpContextDataFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<HttpContextDataHandler>();
        }
    }

    public class HttpContextDataHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Read the unique header for this request
            var requestId = context.GetHeader("X-Request-Id");

            // Simulate some processing
            Task.Delay(Random.Shared.Next(1, 10)).GetAwaiter().GetResult();

            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Header: {requestId}"
            });
        }
    }

    public class UserDataFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<UserDataHandler>();
        }
    }

    public class UserDataHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Read the unique user for this request
            var userId = context.GetUserId();

            // Simulate some processing
            Task.Delay(Random.Shared.Next(1, 10)).GetAwaiter().GetResult();

            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"UserId: {userId}"
            });
        }
    }

    public class StressTestFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<StressTestHandler>();
        }
    }

    public class StressTestHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Read unique data for this request
            var requestNumber = context.GetHeader("X-Request-Number");
            var userId = context.GetUserId();
            var value = request.Value;

            // Store in context
            context.Set(requestNumber, "requestNumber");
            context.Set(userId, "userId");
            context.Set(value, "value");

            // Simulate async processing
            await Task.Delay(Random.Shared.Next(1, 5));

            // Retrieve and verify
            context.TryGet<string>(out var retrievedNumber, "requestNumber");
            context.TryGet<string>(out var retrievedUserId, "userId");
            context.TryGet<string>(out var retrievedValue, "value");

            // Verify data integrity
            if (retrievedNumber != requestNumber || retrievedUserId != userId || retrievedValue != value)
            {
                throw new InvalidOperationException("DATA CORRUPTION DETECTED!");
            }

            return new SimpleResponse
            {
                Message = $"Processed-{requestNumber}|{userId}|{value}"
            };
        }
    }

    public class ParentWithSubFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<ParentWithSubFlowHandler>();
        }
    }

    public class ParentWithSubFlowHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        private readonly SubFlow _subFlow;

        public ParentWithSubFlowHandler(SubFlow subFlow)
        {
            _subFlow = subFlow;
        }

        public async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Parent stores a value
            context.Set(request.Value, "parentValue");

            // Simulate some processing
            await Task.Delay(Random.Shared.Next(1, 5));

            // Call sub-flow with same context
            var subResult = await _subFlow.ExecuteAsync(
                new SimpleRequest { Value = request.Value },
                context);

            return new SimpleResponse
            {
                Message = $"{request.Value}|{subResult.Message}"
            };
        }
    }

    public class SubFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<SubFlowHandler>();
        }
    }

    public class SubFlowHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Sub-flow reads parent's value
            context.TryGet<string>(out var parentValue, "parentValue");

            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"SubFlow-{parentValue}"
            });
        }
    }

    public class CancellableFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<CancellableHandler>();
        }
    }

    public class CancellableHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            // Simulate long-running operation
            for (int i = 0; i < 10; i++)
            {
                context.ThrowIfCancellationRequested();
                await Task.Delay(50, context.CancellationToken);
            }

            return new SimpleResponse
            {
                Message = $"Completed: {request.Value}"
            };
        }
    }

    public class FlowIdResponse
    {
        public Guid FlowId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
