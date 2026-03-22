using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Extensions;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FlowT.Tests;

/// <summary>
/// Tests for the plugin system: AddFlowPlugin registration, Plugin&lt;T&gt;() resolution and
/// PerFlow caching, and FlowPlugin abstract base class context binding.
/// </summary>
public class PluginTests : FlowTestBase
{
    // ── AddFlowPlugin Registration ──────────────────────────────────────────────

    [Fact]
    public void AddFlowPlugin_RegistersPlugin_InServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddFlowPlugin<ISimplePlugin, SimplePlugin>();
        var provider = services.BuildServiceProvider();

        var plugin = provider.GetService<ISimplePlugin>();

        Assert.NotNull(plugin);
    }

    [Fact]
    public void AddFlowPlugin_RegistersPlugin_AsTransient()
    {
        var services = new ServiceCollection();
        services.AddFlowPlugin<ISimplePlugin, SimplePlugin>();
        var provider = services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<ISimplePlugin>();
        var instance2 = provider.GetRequiredService<ISimplePlugin>();

        Assert.NotSame(instance1, instance2);
    }

    [Fact]
    public void AddFlowPlugin_DoesNotOverwrite_WhenCalledTwice()
    {
        var services = new ServiceCollection();
        services.AddFlowPlugin<ISimplePlugin, SimplePlugin>();
        services.AddFlowPlugin<ISimplePlugin, AlternativePlugin>(); // must be ignored

        var provider = services.BuildServiceProvider();
        var plugin = provider.GetRequiredService<ISimplePlugin>();

        Assert.IsType<SimplePlugin>(plugin);
    }

    [Fact]
    public void AddFlowPlugin_ReturnsServiceCollection_ForMethodChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddFlowPlugin<ISimplePlugin, SimplePlugin>();

        Assert.Same(services, result);
    }

    // ── Plugin<T>() Resolution and PerFlow Caching ─────────────────────────────

    [Fact]
    public void Plugin_ReturnsRegisteredPlugin()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ISimplePlugin, SimplePlugin>());
        var context = CreateContext(provider);

        var plugin = context.Plugin<ISimplePlugin>();

        Assert.NotNull(plugin);
        Assert.IsType<SimplePlugin>(plugin);
    }

    [Fact]
    public void Plugin_ReturnsSameInstance_ForSameContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ISimplePlugin, SimplePlugin>());
        var context = CreateContext(provider);

        var first = context.Plugin<ISimplePlugin>();
        var second = context.Plugin<ISimplePlugin>();

        Assert.Same(first, second);
    }

    [Fact]
    public void Plugin_ReturnsDifferentInstance_ForDifferentContexts()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ISimplePlugin, SimplePlugin>());
        var context1 = CreateContext(provider);
        var context2 = CreateContext(provider);

        var plugin1 = context1.Plugin<ISimplePlugin>();
        var plugin2 = context2.Plugin<ISimplePlugin>();

        Assert.NotSame(plugin1, plugin2);
    }

    [Fact]
    public void Plugin_ThrowsInvalidOperationException_WhenNotRegistered()
    {
        var context = CreateContext();

        Assert.Throws<InvalidOperationException>(() => context.Plugin<ISimplePlugin>());
    }

    [Fact]
    public void Plugin_WithoutFlowPluginBase_WorksCorrectly()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ISimplePlugin, SimplePlugin>());
        var context = CreateContext(provider);

        var plugin = context.Plugin<ISimplePlugin>();

        Assert.Equal("simple", plugin.GetValue());
    }

    // ── FlowPlugin Context Binding ──────────────────────────────────────────────

    [Fact]
    public void FlowPlugin_ContextIsSet_AfterPluginResolution()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IContextAwarePlugin, ContextAwarePlugin>());
        var context = CreateContext(provider);

        var plugin = context.Plugin<IContextAwarePlugin>();

        Assert.True(plugin.IsContextSet);
    }

    [Fact]
    public void FlowPlugin_ContextIsCorrectInstance()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IContextAwarePlugin, ContextAwarePlugin>());
        var context = CreateContext(provider);

        var plugin = context.Plugin<IContextAwarePlugin>();

        Assert.Same(context, plugin.GetContext());
    }

    [Fact]
    public void FlowPlugin_ContextBoundOnce_NotResetOnSubsequentCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IContextAwarePlugin, ContextAwarePlugin>());
        var context = CreateContext(provider);

        var first = context.Plugin<IContextAwarePlugin>();
        var second = context.Plugin<IContextAwarePlugin>();

        Assert.Same(first.GetContext(), second.GetContext());
    }

    [Fact]
    public void FlowPlugin_CanWrite_FlowState_ViaContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICounterPlugin, CounterPlugin>());
        var context = CreateContext(provider);

        var plugin = context.Plugin<ICounterPlugin>();
        plugin.Increment();
        plugin.Increment();

        context.TryGet<int>(out var count);
        Assert.Equal(2, count);
    }

    [Fact]
    public void FlowPlugin_CanRead_FlowState_ViaContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICounterPlugin, CounterPlugin>());
        var context = CreateContext(provider);
        context.Set(99);

        context.Plugin<ICounterPlugin>().Increment();

        context.TryGet<int>(out var count);
        Assert.Equal(100, count);
    }

    [Fact]
    public void FlowPlugin_CanAccessServices_ViaContext()
    {
        var provider = BuildServiceProvider(s =>
        {
            s.AddSingleton<IPluginMarkerService, PluginMarkerService>();
            s.AddFlowPlugin<IServiceAccessPlugin, ServiceAccessPlugin>();
        });
        var context = CreateContext(provider);

        var plugin = context.Plugin<IServiceAccessPlugin>();
        var resolved = plugin.ResolveMarkerService();

        Assert.NotNull(resolved);
        Assert.IsType<PluginMarkerService>(resolved);
    }

    // ── FlowPlugin Accessibility ────────────────────────────────────────────────

    [Fact]
    public void FlowPlugin_Initialize_IsNotPublic()
    {
        var publicMethods = typeof(FlowPlugin).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(publicMethods, m => m.Name == "Initialize");
    }

    [Fact]
    public void FlowPlugin_Initialize_IsInternal()
    {
        var method = typeof(FlowPlugin).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);
        Assert.True(method!.IsAssembly);
    }

    [Fact]
    public void FlowPlugin_Context_IsNotPublic()
    {
        var publicProperties = typeof(FlowPlugin).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.DoesNotContain(publicProperties, p => p.Name == "Context");
    }

    [Fact]
    public void FlowPlugin_Context_IsProtected()
    {
        var property = typeof(FlowPlugin).GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(property!.GetMethod!.IsFamily);
    }

    // ── Pipeline Integration ────────────────────────────────────────────────────

    [Fact]
    public async Task Plugin_IsSharedAcrossPipelineStages_PolicyAndHandler()
    {
        var services = new ServiceCollection();
        services.AddFlowPlugin<ICounterPlugin, CounterPlugin>();
        services.AddTransient<PluginAwarePolicy>();
        services.AddTransient<PluginAwareHandler>();
        services.AddSingleton<PluginAwareFlow>();
        var provider = services.BuildServiceProvider();

        var flow = provider.GetRequiredService<PluginAwareFlow>();
        var context = new FlowContext
        {
            Services = provider,
            CancellationToken = CancellationToken.None
        };

        await flow.ExecuteAsync(new PluginTestRequest(), context);

        // Policy incremented once, handler incremented once → shared instance = 2
        context.TryGet<int>(out var totalCount);
        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task Plugin_IsolatedBetweenFlowExecutions()
    {
        var services = new ServiceCollection();
        services.AddFlowPlugin<ICounterPlugin, CounterPlugin>();
        services.AddTransient<PluginAwareHandler>();
        services.AddSingleton<PluginAwareHandlerOnlyFlow>();
        var provider = services.BuildServiceProvider();

        var flow = provider.GetRequiredService<PluginAwareHandlerOnlyFlow>();

        var context1 = new FlowContext { Services = provider, CancellationToken = CancellationToken.None };
        var context2 = new FlowContext { Services = provider, CancellationToken = CancellationToken.None };

        await flow.ExecuteAsync(new PluginTestRequest(), context1);
        await flow.ExecuteAsync(new PluginTestRequest(), context2);

        context1.TryGet<int>(out var count1);
        context2.TryGet<int>(out var count2);

        Assert.Equal(1, count1);
        Assert.Equal(1, count2);
        Assert.NotSame(context1.Plugin<ICounterPlugin>(), context2.Plugin<ICounterPlugin>());
    }

    // ── Test Types ──────────────────────────────────────────────────────────────

    public interface ISimplePlugin
    {
        string GetValue();
    }

    public class SimplePlugin : ISimplePlugin
    {
        public string GetValue() => "simple";
    }

    public class AlternativePlugin : ISimplePlugin
    {
        public string GetValue() => "alternative";
    }

    public interface IContextAwarePlugin
    {
        bool IsContextSet { get; }
        FlowContext GetContext();
    }

    public class ContextAwarePlugin : FlowPlugin, IContextAwarePlugin
    {
        public bool IsContextSet => Context is not null;
        public FlowContext GetContext() => Context;
    }

    public interface ICounterPlugin
    {
        void Increment();
    }

    public class CounterPlugin : FlowPlugin, ICounterPlugin
    {
        public void Increment()
        {
            Context.TryGet<int>(out var current);
            Context.Set(current + 1);
        }
    }

    public interface IPluginMarkerService { }

    public class PluginMarkerService : IPluginMarkerService { }

    public interface IServiceAccessPlugin
    {
        IPluginMarkerService ResolveMarkerService();
    }

    public class ServiceAccessPlugin : FlowPlugin, IServiceAccessPlugin
    {
        public IPluginMarkerService ResolveMarkerService() => Context.Service<IPluginMarkerService>();
    }

    public record PluginTestRequest { }

    public record PluginTestResponse { }

    public class PluginAwareFlow : FlowDefinition<PluginTestRequest, PluginTestResponse>
    {
        protected override void Configure(IFlowBuilder<PluginTestRequest, PluginTestResponse> flow)
        {
            flow.Use<PluginAwarePolicy>().Handle<PluginAwareHandler>();
        }
    }

    public class PluginAwareHandlerOnlyFlow : FlowDefinition<PluginTestRequest, PluginTestResponse>
    {
        protected override void Configure(IFlowBuilder<PluginTestRequest, PluginTestResponse> flow)
        {
            flow.Handle<PluginAwareHandler>();
        }
    }

    public class PluginAwarePolicy : FlowPolicy<PluginTestRequest, PluginTestResponse>
    {
        public override async ValueTask<PluginTestResponse> HandleAsync(PluginTestRequest request, FlowContext context)
        {
            context.Plugin<ICounterPlugin>().Increment();
            return await Next.HandleAsync(request, context);
        }
    }

    public class PluginAwareHandler : IFlowHandler<PluginTestRequest, PluginTestResponse>
    {
        public ValueTask<PluginTestResponse> HandleAsync(PluginTestRequest request, FlowContext context)
        {
            context.Plugin<ICounterPlugin>().Increment();
            return ValueTask.FromResult(new PluginTestResponse());
        }
    }
}
