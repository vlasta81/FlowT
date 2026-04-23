using FlowT;
using FlowT.Extensions;
using FlowT.Plugins;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Tests;

/// <summary>
/// Tests for IFeatureFlagPlugin / FeatureFlagPlugin.
/// Uses a hand-written IVariantFeatureManager fake to avoid adding a mocking library.
/// </summary>
public class FeatureFlagPluginTests : FlowTestBase
{
    // ── IsEnabledAsync — basic on/off ─────────────────────────────────────────

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_ReturnsTrueForEnabledFeature()
    {
        var provider = BuildProvider(new FakeFeatureManager(("Beta", true)));
        var context = CreateContext(provider);

        var result = await context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("Beta");

        Assert.True(result);
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_ReturnsFalseForDisabledFeature()
    {
        var provider = BuildProvider(new FakeFeatureManager(("Beta", false)));
        var context = CreateContext(provider);

        var result = await context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("Beta");

        Assert.False(result);
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_ReturnsFalseForUnknownFeature()
    {
        var provider = BuildProvider(new FakeFeatureManager());
        var context = CreateContext(provider);

        var result = await context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("Unknown");

        Assert.False(result);
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_ThrowsOnNullFeatureName()
    {
        var provider = BuildProvider(new FakeFeatureManager());
        var context = CreateContext(provider);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync(null!).AsTask());
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_ThrowsOnWhitespaceFeatureName()
    {
        var provider = BuildProvider(new FakeFeatureManager());
        var context = CreateContext(provider);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("   ").AsTask());
    }

    // ── Per-flow result cache ─────────────────────────────────────────────────

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_CachesResultOnSecondCall()
    {
        var fake = new FakeFeatureManager(("X", true));
        var provider = BuildProvider(fake);
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFeatureFlagPlugin>();

        await plugin.IsEnabledAsync("X");
        await plugin.IsEnabledAsync("X");

        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task FeatureFlagPlugin_Cache_ContainsEvaluatedFeatures()
    {
        var provider = BuildProvider(new FakeFeatureManager(("A", true), ("B", false)));
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFeatureFlagPlugin>();

        await plugin.IsEnabledAsync("A");
        await plugin.IsEnabledAsync("B");

        Assert.True(plugin.Cache.ContainsKey("A"));
        Assert.True(plugin.Cache.ContainsKey("B"));
        Assert.True(plugin.Cache["A"]);
        Assert.False(plugin.Cache["B"]);
    }

    [Fact]
    public async Task FeatureFlagPlugin_Cache_IsEmptyBeforeAnyCall()
    {
        var provider = BuildProvider(new FakeFeatureManager());
        var context = CreateContext(provider);

        Assert.Empty(context.Plugin<IFeatureFlagPlugin>().Cache);

        await Task.CompletedTask; // keeps method async
    }

    // ── TryGetCached ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FeatureFlagPlugin_TryGetCached_ReturnsFalseBeforeEvaluation()
    {
        var provider = BuildProvider(new FakeFeatureManager(("Beta", true)));
        var context = CreateContext(provider);

        bool found = context.Plugin<IFeatureFlagPlugin>().TryGetCached("Beta", out _);

        Assert.False(found);

        await Task.CompletedTask;
    }

    [Fact]
    public async Task FeatureFlagPlugin_TryGetCached_ReturnsTrueAfterEvaluation()
    {
        var provider = BuildProvider(new FakeFeatureManager(("Beta", true)));
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFeatureFlagPlugin>();

        await plugin.IsEnabledAsync("Beta");
        bool found = plugin.TryGetCached("Beta", out bool value);

        Assert.True(found);
        Assert.True(value);
    }

    [Fact]
    public async Task FeatureFlagPlugin_TryGetCached_ReturnsCorrectValueForDisabledFeature()
    {
        var provider = BuildProvider(new FakeFeatureManager(("Off", false)));
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFeatureFlagPlugin>();

        await plugin.IsEnabledAsync("Off");
        bool found = plugin.TryGetCached("Off", out bool value);

        Assert.True(found);
        Assert.False(value);
    }

    // ── Contextual overload ───────────────────────────────────────────────────

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_WithContext_ReturnsTrueForEnabledFeature()
    {
        var provider = BuildProvider(new FakeFeatureManager(("BetaTargeted", true)));
        var context = CreateContext(provider);

        var result = await context.Plugin<IFeatureFlagPlugin>()
            .IsEnabledAsync("BetaTargeted", new TargetingContext { UserId = "user1" });

        Assert.True(result);
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_WithContext_ThrowsOnNullFeatureName()
    {
        var provider = BuildProvider(new FakeFeatureManager());
        var context = CreateContext(provider);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.Plugin<IFeatureFlagPlugin>()
                   .IsEnabledAsync<TargetingContext>(null!, new TargetingContext(), CancellationToken.None)
                   .AsTask());
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsEnabledAsync_WithContext_CachesResult()
    {
        var fake = new FakeFeatureManager(("T", true));
        var provider = BuildProvider(fake);
        var context = CreateContext(provider);
        var plugin = context.Plugin<IFeatureFlagPlugin>();

        await plugin.IsEnabledAsync("T", new TargetingContext { UserId = "u" });
        await plugin.IsEnabledAsync("T", new TargetingContext { UserId = "u" });

        Assert.Equal(1, fake.CallCount);
    }

    // ── PerFlow isolation ─────────────────────────────────────────────────────

    [Fact]
    public async Task FeatureFlagPlugin_IsShared_AcrossPipelineStagesOnSameContext()
    {
        var provider = BuildProvider(new FakeFeatureManager(("X", true)));
        var context = CreateContext(provider);

        await context.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("X");

        Assert.True(context.Plugin<IFeatureFlagPlugin>().TryGetCached("X", out _));
        Assert.Same(context.Plugin<IFeatureFlagPlugin>(), context.Plugin<IFeatureFlagPlugin>());
    }

    [Fact]
    public async Task FeatureFlagPlugin_IsIsolated_BetweenContexts()
    {
        var provider = BuildProvider(new FakeFeatureManager(("X", true)));
        var ctx1 = CreateContext(provider);
        var ctx2 = CreateContext(provider);

        await ctx1.Plugin<IFeatureFlagPlugin>().IsEnabledAsync("X");

        Assert.False(ctx2.Plugin<IFeatureFlagPlugin>().TryGetCached("X", out _));
    }

    // ── Construction guard ────────────────────────────────────────────────────

    [Fact]
    public void FeatureFlagPlugin_ThrowsArgumentNullException_WhenFeatureManagerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FeatureFlagPlugin(null!));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IServiceProvider BuildProvider(FakeFeatureManager fake)
    {
        return BuildServiceProvider(s =>
        {
            s.AddSingleton<IVariantFeatureManager>(fake);
            s.AddFlowPlugin<IFeatureFlagPlugin, FeatureFlagPlugin>();
        });
    }

    /// <summary>
    /// Minimal IVariantFeatureManager fake that serves a predefined feature flag dictionary.
    /// Tracks how many times the underlying evaluation was called (to verify caching).
    /// </summary>
    private sealed class FakeFeatureManager : IVariantFeatureManager
    {
        private readonly Dictionary<string, bool> _flags;
        private int _callCount;

        public int CallCount => _callCount;

        public FakeFeatureManager(params (string Name, bool Enabled)[] flags)
        {
            _flags = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var (name, enabled) in flags)
                _flags[name] = enabled;
        }

        public ValueTask<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default)
        {
            _callCount++;
            return new ValueTask<bool>(_flags.TryGetValue(feature, out bool v) && v);
        }

        public ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext appContext, CancellationToken cancellationToken = default)
        {
            _callCount++;
            return new ValueTask<bool>(_flags.TryGetValue(feature, out bool v) && v);
        }

        public ValueTask<Variant?> GetVariantAsync(string feature, CancellationToken cancellationToken = default) =>
            new ValueTask<Variant?>((Variant?)null);

        public ValueTask<Variant?> GetVariantAsync(string feature, ITargetingContext targetingContext, CancellationToken cancellationToken = default) =>
            new ValueTask<Variant?>((Variant?)null);

#pragma warning disable CS8424 // EnumeratorCancellationAttribute
        public async IAsyncEnumerable<string> GetFeatureNamesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore CS8424
        {
            foreach (var key in _flags.Keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return key;
                await Task.CompletedTask;
            }
        }
    }
}
