using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace FlowT.Tests;

/// <summary>
/// Tests for HttpContext integration and automatic FlowContext creation.
/// Verifies that HttpContext is correctly passed through flow execution
/// and that helper methods work as expected.
/// </summary>
public class HttpContextIntegrationTests : FlowTestBase
{
    #region Automatic FlowContext Creation Tests

    [Fact]
    public async Task ExecuteAsync_WithHttpContext_AutomaticallyCreatesFlowContext()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<SimpleHandler>()
            .AddSingleton<SimpleFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<SimpleFlow>();
        var httpContext = CreateHttpContext(services);

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: Test", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithHttpContext_PassesHttpContextToHandler()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<HttpContextAwareHandler>()
            .AddSingleton<HttpContextFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<HttpContextFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.Request.Headers["X-Test-Header"] = "TestValue";

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("Header: TestValue", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceProvider_CreatesFlowContextWithoutHttpContext()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<SimpleHandler>()
            .AddSingleton<SimpleFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<SimpleFlow>();

        // Act
        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "Test" },
            services,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: Test", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceProvider_HttpContextIsNull()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<NullHttpContextHandler>()
            .AddSingleton<NullHttpContextFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<NullHttpContextFlow>();

        // Act
        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "Test" },
            services,
            CancellationToken.None);

        // Assert
        Assert.Equal("HttpContext is null", result.Message);
    }

    #endregion

    #region HttpContext Helper Methods Tests

    [Fact]
    public async Task GetUser_ReturnsAuthenticatedUser()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<UserAwareHandler>()
            .AddSingleton<UserAwareFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<UserAwareFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.User = CreateUser("user123", "john@example.com", "Admin");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("User: john@example.com, Role: Admin", result.Message);
    }

    [Fact]
    public async Task IsAuthenticated_ReturnsTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<AuthCheckHandler>()
            .AddSingleton<AuthCheckFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<AuthCheckFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.User = CreateUser("user123", "john@example.com");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("Authenticated: True", result.Message);
    }

    [Fact]
    public async Task IsAuthenticated_ReturnsFalse_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<AuthCheckHandler>()
            .AddSingleton<AuthCheckFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<AuthCheckFlow>();
        var httpContext = CreateHttpContext(services);
        // No user set - anonymous

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("Authenticated: False", result.Message);
    }

    [Fact]
    public async Task IsInRole_ReturnsTrue_WhenUserHasRole()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<RoleCheckHandler>()
            .AddSingleton<RoleCheckFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<RoleCheckFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.User = CreateUser("user123", "john@example.com", "Admin", "User");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Admin" }, httpContext);

        // Assert
        Assert.Equal("HasRole: True", result.Message);
    }

    [Fact]
    public async Task IsInRole_ReturnsFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<RoleCheckHandler>()
            .AddSingleton<RoleCheckFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<RoleCheckFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.User = CreateUser("user123", "john@example.com", "User");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Admin" }, httpContext);

        // Assert
        Assert.Equal("HasRole: False", result.Message);
    }

    [Fact]
    public async Task GetUserId_ReturnsUserIdentifier()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<UserIdHandler>()
            .AddSingleton<UserIdFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<UserIdFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.User = CreateUser("user123", "john@example.com");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("UserId: user123", result.Message);
    }

    [Fact]
    public async Task GetHeader_ReturnsRequestHeader()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<HeaderHandler>()
            .AddSingleton<HeaderFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<HeaderFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.Request.Headers["X-Custom-Header"] = "CustomValue";

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "X-Custom-Header" }, httpContext);

        // Assert
        Assert.Equal("Header: CustomValue", result.Message);
    }

    [Fact]
    public async Task GetQueryParam_ReturnsQueryStringParameter()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<QueryParamHandler>()
            .AddSingleton<QueryParamFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<QueryParamFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.Request.QueryString = new QueryString("?page=5&size=20");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "page" }, httpContext);

        // Assert
        Assert.Equal("QueryParam: 5", result.Message);
    }

    [Fact]
    public async Task GetClientIpAddress_ReturnsClientIp()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IpAddressHandler>()
            .AddSingleton<IpAddressFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<IpAddressFlow>();
        var httpContext = CreateHttpContext(services);
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Test" }, httpContext);

        // Assert
        Assert.Equal("IP: 192.168.1.100", result.Message);
    }

    [Fact]
    public async Task SetStatusCode_ModifiesResponseStatusCode()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<StatusCodeHandler>()
            .AddSingleton<StatusCodeFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<StatusCodeFlow>();
        var httpContext = CreateHttpContext(services);

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "201" }, httpContext);

        // Assert
        Assert.Equal(201, httpContext.Response.StatusCode);
        Assert.Equal("Status set to 201", result.Message);
    }

    [Fact]
    public async Task SetResponseHeader_AddsResponseHeader()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<ResponseHeaderHandler>()
            .AddSingleton<ResponseHeaderFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<ResponseHeaderFlow>();
        var httpContext = CreateHttpContext(services);

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "Location:/api/users/123" }, httpContext);

        // Assert
        Assert.True(httpContext.Response.Headers.ContainsKey("Location"));
        Assert.Equal("/api/users/123", httpContext.Response.Headers["Location"].ToString());
        Assert.Equal("Header set", result.Message);
    }

    [Fact]
    public async Task SetCookie_AddsCookieToResponse()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<CookieHandler>()
            .AddSingleton<CookieFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<CookieFlow>();
        var httpContext = CreateHttpContext(services);

        // Act
        var result = await flow.ExecuteAsync(new SimpleRequest { Value = "session:abc123" }, httpContext);

        // Assert
        Assert.Equal("Cookie set", result.Message);
        // Note: Verifying cookies in unit tests requires inspecting Response.Headers["Set-Cookie"]
        // which is set by ASP.NET Core infrastructure
    }

    #endregion

    #region Helper Methods for Non-HTTP Scenarios

    [Fact]
    public async Task GetUser_ReturnsNull_WhenHttpContextIsNull()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<UserAwareHandler>()
            .AddSingleton<UserAwareFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<UserAwareFlow>();

        // Act - Execute without HttpContext
        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "Test" },
            services,
            CancellationToken.None);

        // Assert
        Assert.Equal("User: (null), Role: (null)", result.Message);
    }

    [Fact]
    public async Task IsAuthenticated_ReturnsFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<AuthCheckHandler>()
            .AddSingleton<AuthCheckFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<AuthCheckFlow>();

        // Act
        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "Test" },
            services,
            CancellationToken.None);

        // Assert
        Assert.Equal("Authenticated: False", result.Message);
    }

    [Fact]
    public async Task GetHeader_ReturnsNull_WhenHttpContextIsNull()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<HeaderHandler>()
            .AddSingleton<HeaderFlow>()
            .BuildServiceProvider();

        var flow = services.GetRequiredService<HeaderFlow>();

        // Act
        var result = await flow.ExecuteAsync(
            new SimpleRequest { Value = "X-Custom-Header" },
            services,
            CancellationToken.None);

        // Assert
        Assert.Equal("Header: (null)", result.Message);
    }

    #endregion

    #region Test Helpers

    private static DefaultHttpContext CreateHttpContext(IServiceProvider services)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        // Initialize Request and Response
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

    public class SimpleFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<SimpleHandler>();
        }
    }

    public class SimpleHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Processed: {request.Value}"
            });
        }
    }

    public class HttpContextFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<HttpContextAwareHandler>();
        }
    }

    public class HttpContextAwareHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var header = context.GetHeader("X-Test-Header");
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Header: {header}"
            });
        }
    }

    public class NullHttpContextFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<NullHttpContextHandler>();
        }
    }

    public class NullHttpContextHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var isNull = context.HttpContext is null;
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = isNull ? "HttpContext is null" : "HttpContext is not null"
            });
        }
    }

    public class UserAwareFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<UserAwareHandler>();
        }
    }

    public class UserAwareHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var user = context.GetUser();
            var email = user?.FindFirst(ClaimTypes.Email)?.Value ?? "(null)";
            var role = context.IsInRole("Admin") ? "Admin" : "(null)";

            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"User: {email}, Role: {role}"
            });
        }
    }

    public class AuthCheckFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<AuthCheckHandler>();
        }
    }

    public class AuthCheckHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var isAuth = context.IsAuthenticated();
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Authenticated: {isAuth}"
            });
        }
    }

    public class RoleCheckFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<RoleCheckHandler>();
        }
    }

    public class RoleCheckHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var hasRole = context.IsInRole(request.Value);
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"HasRole: {hasRole}"
            });
        }
    }

    public class UserIdFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<UserIdHandler>();
        }
    }

    public class UserIdHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var userId = context.GetUserId();
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"UserId: {userId}"
            });
        }
    }

    public class HeaderFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<HeaderHandler>();
        }
    }

    public class HeaderHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var header = context.GetHeader(request.Value) ?? "(null)";
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Header: {header}"
            });
        }
    }

    public class QueryParamFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<QueryParamHandler>();
        }
    }

    public class QueryParamHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var param = context.GetQueryParam(request.Value) ?? "(null)";
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"QueryParam: {param}"
            });
        }
    }

    public class IpAddressFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<IpAddressHandler>();
        }
    }

    public class IpAddressHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var ip = context.GetClientIpAddress() ?? "(null)";
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"IP: {ip}"
            });
        }
    }

    public class StatusCodeFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<StatusCodeHandler>();
        }
    }

    public class StatusCodeHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var statusCode = int.Parse(request.Value);
            context.SetStatusCode(statusCode);
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = $"Status set to {statusCode}"
            });
        }
    }

    public class ResponseHeaderFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<ResponseHeaderHandler>();
        }
    }

    public class ResponseHeaderHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var parts = request.Value.Split(':');
            context.SetResponseHeader(parts[0], parts[1]);
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = "Header set"
            });
        }
    }

    public class CookieFlow : FlowDefinition<SimpleRequest, SimpleResponse>
    {
        protected override void Configure(IFlowBuilder<SimpleRequest, SimpleResponse> flow)
        {
            flow.Handle<CookieHandler>();
        }
    }

    public class CookieHandler : IFlowHandler<SimpleRequest, SimpleResponse>
    {
        public ValueTask<SimpleResponse> HandleAsync(SimpleRequest request, FlowContext context)
        {
            var parts = request.Value.Split(':');
            context.SetCookie(parts[0], parts[1], new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });
            return new ValueTask<SimpleResponse>(new SimpleResponse
            {
                Message = "Cookie set"
            });
        }
    }

    #endregion
}
