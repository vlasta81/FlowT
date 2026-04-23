using FlowT;
using FlowT.Extensions;
using FlowT.Plugins;
using FlowT.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;

namespace FlowT.Tests;

/// <summary>
/// Tests for new built-in plugins: IAuditPlugin, ITenantPlugin, IIdempotencyPlugin,
/// IPerformancePlugin, IFlowScopePlugin.
/// </summary>
public class NewPluginTests : FlowTestBase
{
    // ── IAuditPlugin ─────────────────────────────────────────────────────────────

    [Fact]
    public void AuditPlugin_Entries_IsEmptyInitially()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);

        Assert.Empty(context.Plugin<IAuditPlugin>().Entries);
    }

    [Fact]
    public void AuditPlugin_Record_AddsEntryWithCorrectAction()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IAuditPlugin>();

        plugin.Record("OrderCreated");

        Assert.Single(plugin.Entries);
        Assert.Equal("OrderCreated", plugin.Entries[0].Action);
    }

    [Fact]
    public void AuditPlugin_Record_StoresData()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IAuditPlugin>();
        var data = new { orderId = 42 };

        plugin.Record("OrderCreated", data);

        Assert.Same(data, plugin.Entries[0].Data);
    }

    [Fact]
    public void AuditPlugin_Record_SetsTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IAuditPlugin>();

        plugin.Record("OrderCreated");

        Assert.True(plugin.Entries[0].Timestamp >= before);
    }

    [Fact]
    public void AuditPlugin_Record_AccumulatesMultipleEntries()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IAuditPlugin>();

        plugin.Record("Step1");
        plugin.Record("Step2");
        plugin.Record("Step3");

        Assert.Equal(3, plugin.Entries.Count);
        Assert.Equal("Step1", plugin.Entries[0].Action);
        Assert.Equal("Step2", plugin.Entries[1].Action);
        Assert.Equal("Step3", plugin.Entries[2].Action);
    }

    [Fact]
    public void AuditPlugin_Record_ThrowsOnNullAction()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);

        Assert.Throws<ArgumentException>(() => context.Plugin<IAuditPlugin>().Record(null!));
    }

    [Fact]
    public void AuditPlugin_Record_ThrowsOnWhitespaceAction()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);

        Assert.Throws<ArgumentException>(() => context.Plugin<IAuditPlugin>().Record("   "));
    }

    [Fact]
    public void AuditPlugin_IsShared_AcrossPipelineStagesOnSameContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IAuditPlugin, AuditPlugin>());
        var context = CreateContext(provider);

        context.Plugin<IAuditPlugin>().Record("Step1");

        Assert.Single(context.Plugin<IAuditPlugin>().Entries);
    }

    // ── ITenantPlugin ─────────────────────────────────────────────────────────────

    [Fact]
    public void TenantPlugin_WhenHttpContextIsNull_ReturnsDefault()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var context = CreateContext(provider);

        Assert.Equal("default", context.Plugin<ITenantPlugin>().TenantId);
    }

    [Fact]
    public void TenantPlugin_WhenClaimTidPresent_ReturnsClaim()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("tid", "tenant-from-claim") }, "TestAuth"));
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.Equal("tenant-from-claim", context.Plugin<ITenantPlugin>().TenantId);
    }

    [Fact]
    public void TenantPlugin_WhenHeaderPresent_ReturnsHeader()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "tenant-from-header";
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.Equal("tenant-from-header", context.Plugin<ITenantPlugin>().TenantId);
    }

    [Fact]
    public void TenantPlugin_ClaimTakesPrecedenceOverHeader()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("tid", "claim-tenant") }, "TestAuth"));
        httpContext.Request.Headers["X-Tenant-Id"] = "header-tenant";
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.Equal("claim-tenant", context.Plugin<ITenantPlugin>().TenantId);
    }

    [Fact]
    public void TenantPlugin_WhenNoSourcePresent_FallsBackToDefault()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var context = CreateContextWithHttp(provider, new DefaultHttpContext());

        Assert.Equal("default", context.Plugin<ITenantPlugin>().TenantId);
    }

    [Fact]
    public void TenantPlugin_ReturnsSameValue_OnMultipleCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITenantPlugin, TenantPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "my-tenant";
        var context = CreateContextWithHttp(provider, httpContext);
        var plugin = context.Plugin<ITenantPlugin>();

        Assert.Equal(plugin.TenantId, plugin.TenantId);
    }

    // ── IIdempotencyPlugin ────────────────────────────────────────────────────────

    [Fact]
    public void IdempotencyPlugin_WhenHttpContextIsNull_HasKey_ReturnsFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var context = CreateContext(provider);

        Assert.False(context.Plugin<IIdempotencyPlugin>().HasKey);
    }

    [Fact]
    public void IdempotencyPlugin_WhenHttpContextIsNull_Key_ReturnsNull()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var context = CreateContext(provider);

        Assert.Null(context.Plugin<IIdempotencyPlugin>().Key);
    }

    [Fact]
    public void IdempotencyPlugin_WhenHeaderPresent_HasKey_ReturnsTrue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Idempotency-Key"] = "idem-123";
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.True(context.Plugin<IIdempotencyPlugin>().HasKey);
    }

    [Fact]
    public void IdempotencyPlugin_WhenHeaderPresent_Key_ReturnsHeaderValue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Idempotency-Key"] = "idem-123";
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.Equal("idem-123", context.Plugin<IIdempotencyPlugin>().Key);
    }

    [Fact]
    public void IdempotencyPlugin_WhenHeaderAbsent_HasKey_ReturnsFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var context = CreateContextWithHttp(provider, new DefaultHttpContext());

        Assert.False(context.Plugin<IIdempotencyPlugin>().HasKey);
    }

    [Fact]
    public void IdempotencyPlugin_ReturnsSameKey_OnMultipleCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Idempotency-Key"] = "idem-456";
        var context = CreateContextWithHttp(provider, httpContext);
        var plugin = context.Plugin<IIdempotencyPlugin>();

        Assert.Equal(plugin.Key, plugin.Key);
    }

    // ── IPerformancePlugin ────────────────────────────────────────────────────────

    [Fact]
    public void PerformancePlugin_Elapsed_IsEmptyInitially()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);

        Assert.Empty(context.Plugin<IPerformancePlugin>().Elapsed);
    }

    [Fact]
    public void PerformancePlugin_Measure_RecordsElapsedAfterDispose()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IPerformancePlugin>();

        using (plugin.Measure("operation"))
        {
        }

        Assert.True(plugin.Elapsed.ContainsKey("operation"));
        Assert.True(plugin.Elapsed["operation"] >= TimeSpan.Zero);
    }

    [Fact]
    public void PerformancePlugin_Measure_DoesNotRecordBeforeDispose()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IPerformancePlugin>();

        var scope = plugin.Measure("operation");

        Assert.False(plugin.Elapsed.ContainsKey("operation"));

        scope.Dispose();
    }

    [Fact]
    public void PerformancePlugin_Measure_AccumulatesMultipleMeasurements()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IPerformancePlugin>();

        using (plugin.Measure("step1")) { }
        using (plugin.Measure("step2")) { }
        using (plugin.Measure("step3")) { }

        Assert.Equal(3, plugin.Elapsed.Count);
        Assert.True(plugin.Elapsed.ContainsKey("step1"));
        Assert.True(plugin.Elapsed.ContainsKey("step2"));
        Assert.True(plugin.Elapsed.ContainsKey("step3"));
    }

    [Fact]
    public void PerformancePlugin_Measure_ThrowsOnNullName()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);

        Assert.Throws<ArgumentException>(() => context.Plugin<IPerformancePlugin>().Measure(null!));
    }

    [Fact]
    public void PerformancePlugin_Measure_ThrowsOnWhitespaceName()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);

        Assert.Throws<ArgumentException>(() => context.Plugin<IPerformancePlugin>().Measure("  "));
    }

    [Fact]
    public void PerformancePlugin_IsShared_AcrossPipelineStagesOnSameContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>());
        var context = CreateContext(provider);

        using (context.Plugin<IPerformancePlugin>().Measure("op")) { }

        Assert.True(context.Plugin<IPerformancePlugin>().Elapsed.ContainsKey("op"));
    }

    // ── IFlowScopePlugin ──────────────────────────────────────────────────────────

    [Fact]
    public void FlowScopePlugin_ScopedServices_ReturnsNonNull()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>());
        var context = CreateContext(provider);

        using var plugin = context.Plugin<IFlowScopePlugin>();

        Assert.NotNull(plugin.ScopedServices);
    }

    [Fact]
    public void FlowScopePlugin_ScopedServices_ReturnsSameInstance_OnMultipleCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>());
        var context = CreateContext(provider);

        using var plugin = context.Plugin<IFlowScopePlugin>();

        Assert.Same(plugin.ScopedServices, plugin.ScopedServices);
    }

    [Fact]
    public void FlowScopePlugin_AfterDispose_ScopedServices_ThrowsObjectDisposedException()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFlowScopePlugin>();

        plugin.Dispose();

        Assert.Throws<ObjectDisposedException>(() => plugin.ScopedServices);
    }

    [Fact]
    public void FlowScopePlugin_Dispose_IsIdempotent()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFlowScopePlugin>();

        plugin.Dispose();
        plugin.Dispose(); // must not throw
    }

    [Fact]
    public void FlowScopePlugin_ScopedServices_CanResolveRegisteredService()
    {
        var provider = BuildServiceProvider(s =>
        {
            s.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>();
            s.AddScoped<FlowScopeTestService>();
        });
        var context = CreateContext(provider);

        using var plugin = context.Plugin<IFlowScopePlugin>();
        var service = plugin.ScopedServices.GetRequiredService<FlowScopeTestService>();

        Assert.NotNull(service);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static FlowContext CreateContextWithHttp(IServiceProvider provider, HttpContext httpContext) =>
        new FlowContext
        {
            Services = provider,
            CancellationToken = CancellationToken.None,
            HttpContext = httpContext
        };

    private sealed class FlowScopeTestService { }
}
