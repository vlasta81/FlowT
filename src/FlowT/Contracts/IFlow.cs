
using System;
using System.Threading;
using System.Threading.Tasks;
using FlowT.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FlowT.Contracts
{
    /// <summary>
    /// Represents the main entry point for executing a flow pipeline.
    /// This interface is implemented by <see cref="FlowDefinition{TRequest, TResponse}"/> and represents a complete flow execution unit.
    /// </summary>
    /// <typeparam name="TRequest">The type of request processed by this flow.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by this flow.</typeparam>
    public interface IFlow<in TRequest, TResponse>
    {
        /// <summary>
        /// Executes the flow pipeline asynchronously with an existing flow context.
        /// Use this method when executing sub-flows to share the same context (especially FlowId).
        /// </summary>
        /// <param name="request">The request object containing input data for the flow.</param>
        /// <param name="context">The flow context providing shared state, services, and execution metadata.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation that produces the response.</returns>
        ValueTask<TResponse> ExecuteAsync(TRequest request, FlowContext context);

        /// <summary>
        /// Executes the flow pipeline asynchronously from an HTTP context.
        /// Automatically creates a <see cref="FlowContext"/> with services, cancellation token, and HTTP context from the provided <paramref name="httpContext"/>.
        /// This is a convenience method for ASP.NET Core scenarios where you have <see cref="HttpContext"/> available.
        /// </summary>
        /// <param name="request">The request object containing input data for the flow.</param>
        /// <param name="httpContext">The HTTP context providing services (<see cref="HttpContext.RequestServices"/>), cancellation token (<see cref="HttpContext.RequestAborted"/>), and HTTP metadata.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation that produces the response.</returns>
        /// <remarks>
        /// This method is equivalent to manually creating a <see cref="FlowContext"/> with:
        /// <code>
        /// var context = new FlowContext
        /// {
        ///     Services = httpContext.RequestServices,
        ///     CancellationToken = httpContext.RequestAborted,
        ///     HttpContext = httpContext
        /// };
        /// await flow.ExecuteAsync(request, context);
        /// </code>
        /// Each invocation creates a new <see cref="FlowContext"/> with a unique <see cref="FlowContext.FlowId"/> for correlation.
        /// </remarks>
        ValueTask<TResponse> ExecuteAsync(TRequest request, HttpContext httpContext);

        /// <summary>
        /// Executes the flow pipeline asynchronously from a service provider and cancellation token.
        /// Automatically creates a <see cref="FlowContext"/> without HTTP context.
        /// This is the method for non-HTTP scenarios (background jobs, message queue handlers, console apps, tests).
        /// </summary>
        /// <param name="request">The request object containing input data for the flow.</param>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="ct">The cancellation token to observe for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation that produces the response.</returns>
        /// <remarks>
        /// This method is equivalent to manually creating a <see cref="FlowContext"/> with:
        /// <code>
        /// var context = new FlowContext
        /// {
        ///     Services = serviceProvider,
        ///     CancellationToken = ct,
        ///     HttpContext = null
        /// };
        /// await flow.ExecuteAsync(request, context);
        /// </code>
        /// Use this method when executing flows outside of HTTP requests:
        /// <list type="bullet">
        /// <item>Background services (IHostedService, BackgroundService)</item>
        /// <item>Message queue consumers (RabbitMQ, Azure Service Bus, etc.)</item>
        /// <item>Console applications</item>
        /// <item>Unit/integration tests</item>
        /// <item>Blazor WebAssembly (client-side, no server HttpContext)</item>
        /// </list>
        /// Each invocation creates a new <see cref="FlowContext"/> with a unique <see cref="FlowContext.FlowId"/> for correlation.
        /// </remarks>
        ValueTask<TResponse> ExecuteAsync(TRequest request, IServiceProvider serviceProvider, CancellationToken ct);
    }
}
