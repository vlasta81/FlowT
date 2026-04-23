using FlowT;
using FlowT.Extensions;
using FlowT.Plugins;
using FlowT.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace FlowT.Tests;

/// <summary>
/// Tests for the four built-in FlowT plugins:
/// IUserIdentityPlugin, ICorrelationPlugin, IRetryStatePlugin, ITransactionPlugin.
/// </summary>
public class BuiltInPluginTests : FlowTestBase
{
    // ── IUserIdentityPlugin — non-web (HttpContext = null) ──────────────────────

    [Fact]
    public void UserIdentityPlugin_WhenHttpContextIsNull_IsAuthenticated_ReturnsFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContext(provider);

        Assert.False(context.Plugin<IUserIdentityPlugin>().IsAuthenticated);
    }

    [Fact]
    public void UserIdentityPlugin_WhenHttpContextIsNull_UserId_ReturnsNull()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContext(provider);

        Assert.Null(context.Plugin<IUserIdentityPlugin>().UserId);
    }

    [Fact]
    public void UserIdentityPlugin_WhenHttpContextIsNull_Email_ReturnsNull()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContext(provider);

        Assert.Null(context.Plugin<IUserIdentityPlugin>().Email);
    }

    [Fact]
    public void UserIdentityPlugin_WhenHttpContextIsNull_Principal_ReturnsNull()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContext(provider);

        Assert.Null(context.Plugin<IUserIdentityPlugin>().Principal);
    }

    [Fact]
    public void UserIdentityPlugin_WhenHttpContextIsNull_IsInRole_ReturnsFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContext(provider);

        Assert.False(context.Plugin<IUserIdentityPlugin>().IsInRole("Admin"));
    }

    // ── IUserIdentityPlugin — web (HttpContext with claims) ─────────────────────

    [Fact]
    public void UserIdentityPlugin_WithAuthenticatedUser_IsAuthenticated_ReturnsTrue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(Guid.NewGuid(), "user@example.com"));

        Assert.True(context.Plugin<IUserIdentityPlugin>().IsAuthenticated);
    }

    [Fact]
    public void UserIdentityPlugin_WithAuthenticatedUser_UserId_ReturnsCorrectGuid()
    {
        var expectedId = Guid.NewGuid();
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(expectedId, "user@example.com"));

        Assert.Equal(expectedId, context.Plugin<IUserIdentityPlugin>().UserId);
    }

    [Fact]
    public void UserIdentityPlugin_WithAuthenticatedUser_Email_ReturnsCorrectValue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(Guid.NewGuid(), "user@example.com"));

        Assert.Equal("user@example.com", context.Plugin<IUserIdentityPlugin>().Email);
    }

    [Fact]
    public void UserIdentityPlugin_WithAuthenticatedUser_IsInRole_ReturnsTrueForMatchingRole()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(Guid.NewGuid(), "user@example.com", "Admin"));

        Assert.True(context.Plugin<IUserIdentityPlugin>().IsInRole("Admin"));
    }

    [Fact]
    public void UserIdentityPlugin_WithAuthenticatedUser_IsInRole_ReturnsFalseForNonMatchingRole()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(Guid.NewGuid(), "user@example.com", "Admin"));

        Assert.False(context.Plugin<IUserIdentityPlugin>().IsInRole("SuperAdmin"));
    }

    [Fact]
    public void UserIdentityPlugin_Principal_ReturnsSameInstance_OnMultipleCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IUserIdentityPlugin, UserIdentityPlugin>());
        var context = CreateContextWithHttp(provider, CreateHttpContextWithUser(Guid.NewGuid(), "user@example.com"));
        var plugin = context.Plugin<IUserIdentityPlugin>();

        Assert.Same(plugin.Principal, plugin.Principal);
    }

    // ── ICorrelationPlugin ──────────────────────────────────────────────────────

    [Fact]
    public void CorrelationPlugin_WhenHttpContextIsNull_FallsBackToFlowId()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>());
        var context = CreateContext(provider);

        Assert.Equal(context.FlowIdString, context.Plugin<ICorrelationPlugin>().CorrelationId);
    }

    [Fact]
    public void CorrelationPlugin_WhenHeaderIsPresent_ReturnsHeaderValue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Correlation-Id"] = "my-correlation-id";
        var context = CreateContextWithHttp(provider, httpContext);

        Assert.Equal("my-correlation-id", context.Plugin<ICorrelationPlugin>().CorrelationId);
    }

    [Fact]
    public void CorrelationPlugin_WhenHeaderIsAbsent_FallsBackToFlowId()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>());
        var context = CreateContextWithHttp(provider, new DefaultHttpContext());

        Assert.Equal(context.FlowIdString, context.Plugin<ICorrelationPlugin>().CorrelationId);
    }

    [Fact]
    public void CorrelationPlugin_ReturnsSameValue_OnMultipleCalls()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<ICorrelationPlugin>();

        Assert.Equal(plugin.CorrelationId, plugin.CorrelationId);
    }

    [Fact]
    public void CorrelationPlugin_ReturnsSameInstance_ViaPerFlowCache()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>());
        var context = CreateContext(provider);

        Assert.Same(context.Plugin<ICorrelationPlugin>(), context.Plugin<ICorrelationPlugin>());
    }

    // ── IRetryStatePlugin ───────────────────────────────────────────────────────

    [Fact]
    public void RetryStatePlugin_InitialAttemptNumber_IsZero()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);

        Assert.Equal(0, context.Plugin<IRetryStatePlugin>().AttemptNumber);
    }

    [Fact]
    public void RetryStatePlugin_RegisterAttempt_IncrementsAttemptNumber()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IRetryStatePlugin>();

        plugin.RegisterAttempt();

        Assert.Equal(1, plugin.AttemptNumber);
    }

    [Fact]
    public void RetryStatePlugin_MultipleRegisterAttempt_AccumulatesCorrectly()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IRetryStatePlugin>();

        plugin.RegisterAttempt();
        plugin.RegisterAttempt();
        plugin.RegisterAttempt();

        Assert.Equal(3, plugin.AttemptNumber);
    }

    [Fact]
    public void RetryStatePlugin_ShouldRetry_ReturnsTrueWhenBelowMax()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IRetryStatePlugin>();

        plugin.RegisterAttempt();

        Assert.True(plugin.ShouldRetry(3));
    }

    [Fact]
    public void RetryStatePlugin_ShouldRetry_ReturnsFalseWhenAtMax()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<IRetryStatePlugin>();

        plugin.RegisterAttempt();
        plugin.RegisterAttempt();
        plugin.RegisterAttempt();

        Assert.False(plugin.ShouldRetry(3));
    }

    [Fact]
    public void RetryStatePlugin_IsShared_AcrossPluginCallsOnSameContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context = CreateContext(provider);

        context.Plugin<IRetryStatePlugin>().RegisterAttempt();

        Assert.Equal(1, context.Plugin<IRetryStatePlugin>().AttemptNumber);
    }

    [Fact]
    public void RetryStatePlugin_IsIsolated_BetweenContexts()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<IRetryStatePlugin, RetryStatePlugin>());
        var context1 = CreateContext(provider);
        var context2 = CreateContext(provider);

        context1.Plugin<IRetryStatePlugin>().RegisterAttempt();
        context1.Plugin<IRetryStatePlugin>().RegisterAttempt();

        Assert.Equal(0, context2.Plugin<IRetryStatePlugin>().AttemptNumber);
    }

    // ── ITransactionPlugin ──────────────────────────────────────────────────────

    [Fact]
    public void FlowTransactionPlugin_IsAbstract()
    {
        Assert.True(typeof(TransactionPlugin).IsAbstract);
    }

    [Fact]
    public void FlowTransactionPlugin_ImplementsITransactionPlugin()
    {
        Assert.True(typeof(ITransactionPlugin).IsAssignableFrom(typeof(TransactionPlugin)));
    }

    [Fact]
    public void FlowTransactionPlugin_IsActive_IsFalse_Initially()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITransactionPlugin, TestTransactionPlugin>());
        var context = CreateContext(provider);

        Assert.False(context.Plugin<ITransactionPlugin>().IsActive);
    }

    [Fact]
    public async Task FlowTransactionPlugin_BeginAsync_SetsIsActive_ToTrue()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITransactionPlugin, TestTransactionPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<ITransactionPlugin>();

        await plugin.BeginAsync();

        Assert.True(plugin.IsActive);
    }

    [Fact]
    public async Task FlowTransactionPlugin_CommitAsync_SetsIsActive_ToFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITransactionPlugin, TestTransactionPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<ITransactionPlugin>();

        await plugin.BeginAsync();
        await plugin.CommitAsync();

        Assert.False(plugin.IsActive);
    }

    [Fact]
    public async Task FlowTransactionPlugin_RollbackAsync_SetsIsActive_ToFalse()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITransactionPlugin, TestTransactionPlugin>());
        var context = CreateContext(provider);
        var plugin = context.Plugin<ITransactionPlugin>();

        await plugin.BeginAsync();
        await plugin.RollbackAsync();

        Assert.False(plugin.IsActive);
    }

    [Fact]
    public async Task FlowTransactionPlugin_IsShared_AcrossPluginCallsOnSameContext()
    {
        var provider = BuildServiceProvider(s =>
            s.AddFlowPlugin<ITransactionPlugin, TestTransactionPlugin>());
        var context = CreateContext(provider);

        await context.Plugin<ITransactionPlugin>().BeginAsync();

        Assert.True(context.Plugin<ITransactionPlugin>().IsActive);
        Assert.Same(context.Plugin<ITransactionPlugin>(), context.Plugin<ITransactionPlugin>());
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static FlowContext CreateContextWithHttp(IServiceProvider provider, HttpContext httpContext) =>
        new FlowContext
        {
            Services = provider,
            CancellationToken = CancellationToken.None,
            HttpContext = httpContext
        };

    private static DefaultHttpContext CreateHttpContextWithUser(Guid userId, string email, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return httpContext;
    }

    // ── Test transaction plugin ─────────────────────────────────────────────────

    private sealed class TestTransactionPlugin : TransactionPlugin
    {
        public override ValueTask BeginAsync(CancellationToken cancellationToken = default)
        {
            IsActive = true;
            return ValueTask.CompletedTask;
        }

        public override ValueTask CommitAsync(CancellationToken cancellationToken = default)
        {
            IsActive = false;
            return ValueTask.CompletedTask;
        }

        public override ValueTask RollbackAsync(CancellationToken cancellationToken = default)
        {
            IsActive = false;
            return ValueTask.CompletedTask;
        }
    }
}
