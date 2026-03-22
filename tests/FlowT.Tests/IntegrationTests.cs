using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Tests;

public class IntegrationTests : FlowTestBase
{
    [Fact]
    public async Task CompleteFlow_WithSpecsAndPolicies_ExecutesCorrectly()
    {
        var services = ConfigureServices();
        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Name = "John Doe"
        };

        var result = await flow.ExecuteAsync(request, context);

        Assert.NotNull(result);
        Assert.Equal("User created successfully", result.Message);
        Assert.NotEqual(Guid.Empty, result.UserId);

        // Verify logging policy ran
        context.TryGet<List<string>>(out var logs);
        Assert.NotNull(logs);
        Assert.Contains(logs, l => l.Contains("Executing"));
        Assert.Contains(logs, l => l.Contains("Completed"));
    }

    [Fact]
    public async Task Flow_WithInvalidEmail_FailsAtSpecification()
    {
        var services = ConfigureServices();
        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        var request = new CreateUserRequest
        {
            Email = "invalid-email",
            Name = "John Doe"
        };

        var result = await flow.ExecuteAsync(request, context);

        Assert.Equal("Invalid email format", result.Message);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Flow_WithExistingEmail_FailsAtUniqueCheck()
    {
        var services = ConfigureServices();
        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        // Simulate existing user
        var repository = services.GetRequiredService<UserRepository>();
        repository.AddUser("existing@example.com", "Existing User");

        var request = new CreateUserRequest
        {
            Email = "existing@example.com",
            Name = "New User"
        };

        var result = await flow.ExecuteAsync(request, context);

        Assert.Equal("Email already exists", result.Message);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task SubFlow_SharesContext_WithParentFlow()
    {
        var services = ConfigureServices();
        var parentFlow = services.GetRequiredService<ParentFlow>();
        var context = CreateContext(services);

        // Parent sets a value
        context.Set("SharedValue");

        var result = await parentFlow.ExecuteAsync(new ParentRequest { Value = "Test" }, context);

        // Sub-flow should have access to shared value
        Assert.Equal("Parent processed, Sub received: SharedValue", result.Message);
    }

    [Fact]
    public async Task Flow_WithTransactionPolicy_CommitsOnSuccess()
    {
        var services = ConfigureServices();
        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Name = "John Doe"
        };

        await flow.ExecuteAsync(request, context);

        // Verify transaction was committed
        var repository = services.GetRequiredService<UserRepository>();
        var user = repository.FindByEmail("test@example.com");

        Assert.NotNull(user);
        Assert.Equal("John Doe", user.Name);
    }

    [Fact]
    public async Task Flow_WithRetryPolicy_RetriesOnFailure()
    {
        var services = new ServiceCollection()
            .AddSingleton<UserRepository>()
            .AddSingleton<UnstableHandler>()
            //.AddTransient<UnstableHandler>()
            .AddTransient<RetryPolicy>()
            .AddSingleton<FlowWithRetry>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithRetry>();
        var context = CreateContext(services);

        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        // Handler fails first 2 times, succeeds on 3rd
        var handler = services.GetRequiredService<UnstableHandler>();
        Assert.Equal(3, handler.AttemptCount);
        Assert.Equal("Success after retries", result.Message);
    }

    [Fact]
    public async Task Flow_WithCaching_ReusesResult()
    {
        var services = new ServiceCollection()
            .AddSingleton<UserRepository>()
            .AddSingleton<ExpensiveHandler>()
            .AddTransient<CachingPolicy>()
            .AddSingleton<FlowWithCaching>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithCaching>();
        var context = CreateContext(services);

        // First call - cache miss
        var result1 = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        // Second call - cache hit
        var result2 = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context);

        var handler = services.GetRequiredService<ExpensiveHandler>();
        Assert.Equal(1, handler.ExecutionCount); // Handler called only once

        Assert.Equal(result1.Message, result2.Message);
    }

    [Fact]
    public async Task Flow_WithEventPublishing_InvokesHandlers()
    {
        var eventHandler = new TestEventHandler();
        var services = new ServiceCollection()
            .AddSingleton<UserRepository>()
            .AddTransient<CreateUserHandler>()
            .AddTransient<EmailValidationSpec>()
            .AddTransient<UniqueEmailSpec>()
            .AddTransient<LoggingPolicy>()
            .AddTransient<TransactionPolicy>()
            .AddSingleton<CreateUserFlow>()
            .AddSingleton<IEventHandler<UserCreatedEvent>>(eventHandler)
            .BuildServiceProvider();

        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Name = "John Doe"
        };

        await flow.ExecuteAsync(request, context);

        // Event should have been published
        Assert.True(eventHandler.WasCalled);
        Assert.Equal("test@example.com", eventHandler.ReceivedEvent?.Email);
    }

    [Fact]
    public async Task Flow_WithTimeout_CancelsAfterDeadline()
    {
        var services = new ServiceCollection()
            .AddTransient<VerySlowHandler>()
            .AddTransient<TimeoutPolicy>()
            .AddSingleton<FlowWithTimeout>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<FlowWithTimeout>();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var context = new FlowContext
        {
            Services = services,
            CancellationToken = cts.Token
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, context));
    }

    [Fact]
    public async Task Flow_WithOnInterrupt_MapsAllFailures()
    {
        var services = ConfigureServices();
        var flow = services.GetRequiredService<CreateUserFlow>();
        var context = CreateContext(services);

        // Test email validation failure
        var result1 = await flow.ExecuteAsync(new CreateUserRequest { Email = "bad", Name = "Test" }, context);
        Assert.NotNull(result1.Message);
        Assert.Equal(400, result1.StatusCode);

        // Test unique check failure
        var repository = services.GetRequiredService<UserRepository>();
        repository.AddUser("exists@test.com", "Existing");
        var result2 = await flow.ExecuteAsync(new CreateUserRequest { Email = "exists@test.com", Name = "Test" }, context);
        Assert.NotNull(result2.Message);
        Assert.Equal(409, result2.StatusCode);
    }

    [Fact]
    public async Task ComplexScenario_MultipleFlows_ShareGlobalContext()
    {
        var services = ConfigureServices();
        var flow1 = services.GetRequiredService<CreateUserFlow>();
        var flow2 = services.GetRequiredService<ParentFlow>();
        var context = CreateContext(services);

        // Flow1 creates data
        await flow1.ExecuteAsync(new CreateUserRequest { Email = "shared@test.com", Name = "Shared User" }, context);

        // Flow2 accesses same repository
        var repository = services.GetRequiredService<UserRepository>();
        var user = repository.FindByEmail("shared@test.com");

        Assert.NotNull(user);
        Assert.Equal("Shared User", user.Name);
    }

    // Helper methods
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register infrastructure
        services.AddSingleton<UserRepository>();

        // Register all handlers, specs, policies
        services.AddTransient<CreateUserHandler>();
        services.AddTransient<EmailValidationSpec>();
        services.AddTransient<UniqueEmailSpec>();
        services.AddTransient<LoggingPolicy>();
        services.AddTransient<TransactionPolicy>();
        services.AddTransient<SubFlowHandler>();
        services.AddTransient<SubHandler>();

        // Register flows (this will also register the IFlow<,> interface)
        services.AddSingleton<CreateUserFlow>();
        services.AddSingleton<SubFlow>();
        services.AddSingleton<ParentFlow>();

        // Register flows with FlowFactory for IFlow<,> interface
        services.AddFlows(typeof(IntegrationTests).Assembly);

        return services.BuildServiceProvider();
    }

    // Domain types
    public record CreateUserRequest
    {
        public string Email { get; init; } = "";
        public string Name { get; init; } = "";
    }

    public record CreateUserResponse
    {
        public string Message { get; init; } = "";
        public Guid UserId { get; init; }
        public int StatusCode { get; init; } = 200;
    }

    public record UserCreatedEvent
    {
        public string Email { get; init; } = "";
        public string Name { get; init; } = "";
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
    }

    // Repository
    public class UserRepository
    {
        private readonly List<User> _users = new();

        public void AddUser(string email, string name)
        {
            _users.Add(new User { Email = email, Name = name });
        }

        public User? FindByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public bool EmailExists(string email)
        {
            return _users.Any(u => u.Email == email);
        }
    }

    // Main flow
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
    {
        protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
        {
            flow.Check<EmailValidationSpec>()
                .Check<UniqueEmailSpec>()
                .Use<LoggingPolicy>()
                .Use<TransactionPolicy>()
                .OnInterrupt(interrupt => new CreateUserResponse
                {
                    Message = interrupt.Message ?? "Unknown error",
                    StatusCode = interrupt.StatusCode
                })
                .Handle<CreateUserHandler>();
        }
    }

    // Specifications
    public class EmailValidationSpec : IFlowSpecification<CreateUserRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(CreateUserRequest request, FlowContext context)
        {
            if (!request.Email.Contains("@"))
            {
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Fail("Invalid email format", 400));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    public class UniqueEmailSpec : IFlowSpecification<CreateUserRequest>
    {
        private readonly UserRepository _repository;

        public UniqueEmailSpec(UserRepository repository)
        {
            _repository = repository;
        }

        public ValueTask<FlowInterrupt<object?>?> CheckAsync(CreateUserRequest request, FlowContext context)
        {
            if (_repository.EmailExists(request.Email))
            {
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Fail("Email already exists", 409));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // Policies
    public class LoggingPolicy : FlowPolicy<CreateUserRequest, CreateUserResponse>
    {
        public override async ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
        {
            var logs = context.GetOrAdd(() => new List<string>());
            logs.Add($"Executing flow for {request.Email}");

            var result = await Next.HandleAsync(request, context);

            logs.Add($"Completed flow with status {result.StatusCode}");
            return result;
        }
    }

    public record TransactionState
    {
        public bool InTransaction { get; init; }
        public bool Committed { get; init; }
        public bool RolledBack { get; init; }
    }

    public class TransactionPolicy : FlowPolicy<CreateUserRequest, CreateUserResponse>
    {
        public override async ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
        {
            // Simulate transaction
            context.Set(new TransactionState { InTransaction = true });
            try
            {
                var result = await Next.HandleAsync(request, context);
                context.Set(new TransactionState { InTransaction = false, Committed = true });
                return result;
            }
            catch
            {
                context.Set(new TransactionState { InTransaction = false, RolledBack = true });
                throw;
            }
        }
    }

    // Handler
    public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
    {
        private readonly UserRepository _repository;

        public CreateUserHandler(UserRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
        {
            var user = new User { Email = request.Email, Name = request.Name };
            _repository.AddUser(user.Email, user.Name);

            // Publish event
            await context.PublishAsync(new UserCreatedEvent { Email = user.Email, Name = user.Name }, context.CancellationToken);

            return new CreateUserResponse
            {
                Message = "User created successfully",
                UserId = user.Id,
                StatusCode = 201
            };
        }
    }

    // Event handler
    public class TestEventHandler : IEventHandler<UserCreatedEvent>
    {
        public bool WasCalled { get; private set; }
        public UserCreatedEvent? ReceivedEvent { get; private set; }

        public Task HandleAsync(UserCreatedEvent eventData, CancellationToken cancellationToken)
        {
            WasCalled = true;
            ReceivedEvent = eventData;
            return Task.CompletedTask;
        }
    }

    // Sub-flow tests
    public record ParentRequest { public string Value { get; init; } = ""; }
    public record ParentResponse { public string Message { get; init; } = ""; }
    public record SubRequest { public string Value { get; init; } = ""; }
    public record SubResponse { public string Result { get; init; } = ""; }

    public class ParentFlow : FlowDefinition<ParentRequest, ParentResponse>
    {
        protected override void Configure(IFlowBuilder<ParentRequest, ParentResponse> flow)
        {
            flow.Handle<SubFlowHandler>();
        }
    }

    public class SubFlow : FlowDefinition<SubRequest, SubResponse>
    {
        protected override void Configure(IFlowBuilder<SubRequest, SubResponse> flow)
        {
            flow.Handle<SubHandler>();
        }
    }

    public class SubFlowHandler : IFlowHandler<ParentRequest, ParentResponse>
    {
        private readonly SubFlow _subFlow;

        public SubFlowHandler(SubFlow subFlow)
        {
            _subFlow = subFlow;
        }

        public async ValueTask<ParentResponse> HandleAsync(ParentRequest request, FlowContext context)
        {
            var subResult = await _subFlow.ExecuteAsync(new SubRequest { Value = "sub" }, context);
            return new ParentResponse { Message = $"Parent processed, {subResult.Result}" };
        }
    }

    public class SubHandler : IFlowHandler<SubRequest, SubResponse>
    {
        public ValueTask<SubResponse> HandleAsync(SubRequest request, FlowContext context)
        {
            context.TryGet<string>(out var sharedValue);
            return ValueTask.FromResult(new SubResponse { Result = $"Sub received: {sharedValue}" });
        }
    }

    // Additional test flows
    public record SimpleRequest { public string Value { get; init; } = ""; }
    public record SimpleResponse { public string Message { get; init; } = ""; }

    public class FlowWithRetry : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Use<RetryPolicy>().Handle<UnstableHandler>();
        }
    }

    public class UnstableHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public int AttemptCount { get; private set; }

        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            AttemptCount++;
            if (AttemptCount < 3)
            {
                throw new InvalidOperationException("Simulated failure");
            }
            return ValueTask.FromResult(new SimpleResponse { Message = "Success after retries" });
        }
    }

    public class RetryPolicy : FlowPolicy<SimpleRequest, SimpleResponse>
    {
        public override async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            const int maxAttempts = 3;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    return await Next.HandleAsync(request, context);
                }
                catch when (i < maxAttempts - 1)
                {
                    await Task.Delay(10);
                }
            }
            throw new InvalidOperationException("Max retries exceeded");
        }
    }

    public class FlowWithCaching : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Use<CachingPolicy>().Handle<ExpensiveHandler>();
        }
    }

    public class ExpensiveHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public int ExecutionCount { get; private set; }

        public async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            ExecutionCount++;
            await Task.Delay(50);
            return new SimpleResponse { Message = $"Expensive result: {request.Value}" };
        }
    }

    public class CachingPolicy : FlowPolicy<SimpleRequest, SimpleResponse>
    {
        public override async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var cacheKey = $"cache:{request.Value}";
            if (context.TryGet<SimpleResponse>(out var cached))
            {
                return cached;
            }

            var result = await Next.HandleAsync(request, context);
            context.Set(result);
            return result;
        }
    }

    public class FlowWithTimeout : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Use<TimeoutPolicy>().Handle<VerySlowHandler>();
        }
    }

    public class VerySlowHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            await Task.Delay(5000, context.CancellationToken);
            return new SimpleResponse { Message = "Slow result" };
        }
    }

    public class TimeoutPolicy : FlowPolicy<SimpleRequest, SimpleResponse>
    {
        public override async ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            context.ThrowIfCancellationRequested();
            return await Next.HandleAsync(request, context);
        }
    }
}
