using FlowT;
using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FlowT.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFlows_RegistersFlowsFromAssembly()
    {
        var services = new ServiceCollection();
#pragma warning disable CS0618 // Testing obsolete method
        services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();
        var flow = provider.GetService<IFlow<TestFlowRequest, TestFlowResponse>>();

        Assert.NotNull(flow);
    }

    [Fact]
    public void AddFlows_RegistersFlowsAsSingleton_ObsoleteMethod()
    {
        var services = new ServiceCollection();
#pragma warning disable CS0618 // Testing obsolete method
        services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();
        var flow1 = provider.GetService<IFlow<TestFlowRequest, TestFlowResponse>>();
        var flow2 = provider.GetService<IFlow<TestFlowRequest, TestFlowResponse>>();

        Assert.Same(flow1, flow2); // Same instance = singleton
    }

    [Fact]
    public async Task RegisteredFlow_CanBeExecuted()
    {
        var services = new ServiceCollection();
        // ✅ Use new AddFlow API instead of obsolete AddFlows
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();

        var provider = services.BuildServiceProvider();
        var flow = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();

        var context = new FlowContext
        {
            Services = provider,
            CancellationToken = CancellationToken.None
        };

        var result = await flow.ExecuteAsync(new TestFlowRequest { Value = "Test" }, context);

        Assert.Equal("Handled: Test", result.Message);
    }

    [Fact]
    public void AddFlow_ThrowsException_WhenFlowNotMarkedWithAttribute()
    {
        var services = new ServiceCollection();

        // FlowWithoutAttribute is not marked with [FlowDefinition]
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddFlow<FlowWithoutAttribute, FlowWithoutAttributeRequest, FlowWithoutAttributeResponse>());

        Assert.Contains("must be marked with [FlowDefinition] attribute", exception.Message);
    }

    [Fact]
    public void AddFlows_UsesCallingAssembly_WhenNoAssemblyProvided()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddFlows(); // Testing obsolete method for backward compatibility
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();
        var flow = provider.GetService<IFlow<TestFlowRequest, TestFlowResponse>>();

        Assert.NotNull(flow);
    }

    [Fact]
    public void AddFlowModules_RegistersModulesFromAssembly()
    {
        var services = new ServiceCollection();

        services.AddFlowModules(Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var modules = provider.GetServices<IFlowModule>();

        Assert.NotEmpty(modules);
    }

    [Fact]
    public void AddFlowModules_CallsModuleRegister()
    {
        var services = new ServiceCollection();

        services.AddFlowModules(Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();

        // TestModule registers a service called TestModuleService
        var service = provider.GetService<TestModuleService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void AddFlowModules_UsesCallingAssembly_WhenNoAssemblyProvided()
    {
        var services = new ServiceCollection();

        services.AddFlowModules(); // Should use calling assembly

        var provider = services.BuildServiceProvider();
        var modules = provider.GetServices<IFlowModule>();

        Assert.NotEmpty(modules);
    }

    [Fact]
    public void RegisteredFlow_LazyInitializesPipeline()
    {
        var services = new ServiceCollection();
        // ✅ Use new AddFlow API
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();

        var provider = services.BuildServiceProvider();
        var flow = provider.GetRequiredService<TestFlow>();

        // Pipeline should not be initialized yet
        // (We can't easily assert this, but execution should trigger initialization)

        var context = new FlowContext
        {
            Services = provider,
            CancellationToken = CancellationToken.None
        };

        var exception = Record.ExceptionAsync(async () =>
            await flow.ExecuteAsync(new TestFlowRequest(), context));

        Assert.Null(exception.Result);
    }

    [Fact]
    public void AddFlows_HandlesMultipleFlowsInAssembly()
    {
        var services = new ServiceCollection();
#pragma warning disable CS0618 // Testing obsolete method
        services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();

        var flow1 = provider.GetService<IFlow<TestFlowRequest, TestFlowResponse>>();
        var flow2 = provider.GetService<IFlow<AnotherRequest, AnotherResponse>>();

        Assert.NotNull(flow1);
        Assert.NotNull(flow2);
    }

    [Fact]
    public void AddFlows_RegistersFlowAsSingleton()
    {
        var services = new ServiceCollection();
#pragma warning disable CS0618 // Testing obsolete method
        services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();

        var flow1 = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();
        var flow2 = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();

        // Should be the same instance (Singleton)
        Assert.Same(flow1, flow2);
    }

    [Fact]
    public void AddFlow_RegistersFlowAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();

        var provider = services.BuildServiceProvider();

        var flow1 = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();
        var flow2 = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();

        // Should be the same instance (Singleton)
        Assert.Same(flow1, flow2);
    }

    [Fact]
    public void AddFlow_PreventsDuplicateRegistrations()
    {
        var services = new ServiceCollection();

        // Register the same flow multiple times
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();
        services.AddFlow<TestFlow, TestFlowRequest, TestFlowResponse>();

        var provider = services.BuildServiceProvider();

        // Should only have ONE registration (TryAddSingleton prevents duplicates)
        var flows = provider.GetServices<IFlow<TestFlowRequest, TestFlowResponse>>().ToList();

        Assert.Single(flows); // Only one registration

        // Verify it still works
        var flow = provider.GetRequiredService<IFlow<TestFlowRequest, TestFlowResponse>>();
        Assert.NotNull(flow);
    }

    [Fact]
    public void AddFlows_IgnoresFlowsWithoutAttribute()
    {
        var services = new ServiceCollection();
#pragma warning disable CS0618
        services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618

        var provider = services.BuildServiceProvider();
        var flow = provider.GetService<IFlow<FlowWithoutAttributeRequest, FlowWithoutAttributeResponse>>();

        // Should be null because FlowWithoutAttribute doesn't have [FlowDefinition]
        Assert.Null(flow);
    }

    [Fact]
    public void AddFlowModules_IgnoresModulesWithoutAttribute()
    {
        var services = new ServiceCollection();
        services.AddFlowModules(Assembly.GetExecutingAssembly());

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ModuleWithoutAttributeService>();

        // Should be null because ModuleWithoutAttribute doesn't register anything
        Assert.Null(service);
    }

    // Helper types
    public record TestFlowRequest
    {
        public string Value { get; init; } = "";
    }

    public record TestFlowResponse
    {
        public string Message { get; init; } = "";
    }

    [FlowDefinition]
    public class TestFlow : FlowDefinition<TestFlowRequest, TestFlowResponse>
    {
        protected override void Configure(IFlowBuilder<TestFlowRequest, TestFlowResponse> flow)
        {
            flow.Handle<TestFlowHandler>();
        }
    }

    public class TestFlowHandler : IFlowHandler<TestFlowRequest, TestFlowResponse>
    {
        public ValueTask<TestFlowResponse> HandleAsync(TestFlowRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new TestFlowResponse { Message = $"Handled: {request.Value}" });
        }
    }

    public record AnotherRequest;
    public record AnotherResponse;

    [FlowDefinition]
    public class AnotherFlow : FlowDefinition<AnotherRequest, AnotherResponse>
    {
        protected override void Configure(IFlowBuilder<AnotherRequest, AnotherResponse> flow)
        {
            flow.Handle<AnotherFlowHandler>();
        }
    }

    public class AnotherFlowHandler : IFlowHandler<AnotherRequest, AnotherResponse>
    {
        public ValueTask<AnotherResponse> HandleAsync(AnotherRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new AnotherResponse());
        }
    }

    [FlowModule]
    public class TestModule : IFlowModule
    {
        public void Register(IServiceCollection services)
        {
            services.AddSingleton<TestModuleService>();
#pragma warning disable CS0618 // Testing obsolete in module context
            services.AddFlows(Assembly.GetExecutingAssembly());
#pragma warning restore CS0618
        }

        public void MapEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
        {
            // Not testing endpoint mapping here
        }
    }

    public class TestModuleService
    {
        public string Name => "TestService";
    }

    // Test types without attributes (should not be registered)
    public record FlowWithoutAttributeRequest;
    public record FlowWithoutAttributeResponse;

    // Missing [FlowDefinition] attribute - should NOT be discovered by AddFlows
    // But should throw exception when used with AddFlow directly
    public class FlowWithoutAttribute : FlowDefinition<FlowWithoutAttributeRequest, FlowWithoutAttributeResponse>
    {
        protected override void Configure(IFlowBuilder<FlowWithoutAttributeRequest, FlowWithoutAttributeResponse> flow)
        {
            flow.Handle<FlowWithoutAttributeHandler>();
        }
    }

    public class FlowWithoutAttributeHandler : IFlowHandler<FlowWithoutAttributeRequest, FlowWithoutAttributeResponse>
    {
        public ValueTask<FlowWithoutAttributeResponse> HandleAsync(FlowWithoutAttributeRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new FlowWithoutAttributeResponse());
        }
    }

    // Missing [FlowModule] attribute - should NOT be discovered
    public class ModuleWithoutAttribute : IFlowModule
    {
        public void Register(IServiceCollection services)
        {
            services.AddSingleton<ModuleWithoutAttributeService>();
        }

        public void MapEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
        {
        }
    }

    public class ModuleWithoutAttributeService
    {
        public string Name => "ShouldNotBeRegistered";
    }
}
