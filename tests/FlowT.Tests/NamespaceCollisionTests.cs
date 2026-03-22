using FlowT;
using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Shared types FIRST (so they can be used by both namespaces)
public record SharedRequest(string Data);
public record SharedResponse(string Source);

namespace FlowT.Tests.NamespaceCollisionTest
{
    /// <summary>
    /// Tests verifying behavior when flows with same names exist in different namespaces
    /// </summary>
    public class NamespaceCollisionTests
    {
        [Fact]
        public void AddFlow_RegistersBothFlows_WhenSameNameDifferentNamespaces()
        {
            var services = new ServiceCollection();

            // Register flows with SAME NAME but DIFFERENT NAMESPACES
            services.AddFlow<ModuleA.CreateUserFlow, ModuleA.CreateUserRequest, ModuleA.CreateUserResponse>();
            services.AddFlow<ModuleB.CreateUserFlow, ModuleB.CreateUserRequest, ModuleB.CreateUserResponse>();

            var provider = services.BuildServiceProvider();

            // Both concrete types should be registered (different Type objects)
            var flowA = provider.GetService<ModuleA.CreateUserFlow>();
            var flowB = provider.GetService<ModuleB.CreateUserFlow>();

            Assert.NotNull(flowA);
            Assert.NotNull(flowB);
            Assert.NotSame(flowA, flowB); // Different instances
        }

        [Fact]
        public void AddFlow_RegistersBothInterfaces_WhenDifferentRequestResponseTypes()
        {
            var services = new ServiceCollection();

            // Different request/response types = different IFlow<,> registrations
            services.AddFlow<ModuleA.CreateUserFlow, ModuleA.CreateUserRequest, ModuleA.CreateUserResponse>();
            services.AddFlow<ModuleB.CreateUserFlow, ModuleB.CreateUserRequest, ModuleB.CreateUserResponse>();

            var provider = services.BuildServiceProvider();

            // Both interfaces should be registered (different generic types)
            var flowInterfaceA = provider.GetService<IFlow<ModuleA.CreateUserRequest, ModuleA.CreateUserResponse>>();
            var flowInterfaceB = provider.GetService<IFlow<ModuleB.CreateUserRequest, ModuleB.CreateUserResponse>>();

            Assert.NotNull(flowInterfaceA);
            Assert.NotNull(flowInterfaceB);
        }

        [Fact]
        public void AddFlow_RegistersOnlyFirstFlow_WhenSameRequestResponseTypes()
        {
            var services = new ServiceCollection();

            // ⚠️ PROBLEM: Same request/response types = same IFlow<,> interface
            // Only the FIRST registration will be kept for IFlow<SharedRequest, SharedResponse>
            services.AddFlow<ModuleA.SharedFlow, SharedRequest, SharedResponse>();
            services.AddFlow<ModuleB.SharedFlow, SharedRequest, SharedResponse>();

            var provider = services.BuildServiceProvider();

            // Concrete types: BOTH registered (different Types)
            var concreteA = provider.GetService<ModuleA.SharedFlow>();
            var concreteB = provider.GetService<ModuleB.SharedFlow>();
            Assert.NotNull(concreteA);
            Assert.NotNull(concreteB);

            // Interface: ONLY ONE registered (TryAddSingleton prevents duplicate)
            var interfaces = provider.GetServices<IFlow<SharedRequest, SharedResponse>>().ToList();
            Assert.Single(interfaces); // Only ONE registration for IFlow<SharedRequest, SharedResponse>

            // Which one? The FIRST one (ModuleA.SharedFlow)
            var flow = provider.GetRequiredService<IFlow<SharedRequest, SharedResponse>>();
            Assert.IsType<ModuleA.SharedFlow>(flow);
        }
    }
}

// Test fixtures for ModuleA
namespace ModuleA
{
    public record CreateUserRequest(string Email);
    public record CreateUserResponse(Guid Id);

    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
    {
        protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
        {
            flow.Handle<CreateUserHandler>();
        }
    }

    public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
    {
        public ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new CreateUserResponse(Guid.NewGuid()));
        }
    }

    // Shared request/response types (for collision test)
    [FlowDefinition]
    public class SharedFlow : FlowDefinition<SharedRequest, SharedResponse>
    {
        protected override void Configure(IFlowBuilder<SharedRequest, SharedResponse> flow)
        {
            flow.Handle<SharedFlowHandler>();
        }
    }

    public class SharedFlowHandler : IFlowHandler<SharedRequest, SharedResponse>
    {
        public ValueTask<SharedResponse> HandleAsync(SharedRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new SharedResponse("ModuleA"));
        }
    }
}

// Test fixtures for ModuleB
namespace ModuleB
{
    public record CreateUserRequest(string Username); // Different structure!
    public record CreateUserResponse(int UserId);

    [FlowDefinition]
    public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
    {
        protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
        {
            flow.Handle<CreateUserHandler>();
        }
    }

    public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
    {
        public ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new CreateUserResponse(123));
        }
    }

    // Shared request/response types (for collision test)
    [FlowDefinition]
    public class SharedFlow : FlowDefinition<SharedRequest, SharedResponse>
    {
        protected override void Configure(IFlowBuilder<SharedRequest, SharedResponse> flow)
        {
            flow.Handle<SharedFlowHandler>();
        }
    }

    public class SharedFlowHandler : IFlowHandler<SharedRequest, SharedResponse>
    {
        public ValueTask<SharedResponse> HandleAsync(SharedRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new SharedResponse("ModuleB"));
        }
    }
}
