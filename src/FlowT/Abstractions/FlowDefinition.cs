using FlowT.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Abstractions
{
    /// <summary>
    /// Abstract base class for defining flows. 
    /// Each flow represents a complete use-case with its own pipeline of specifications, policies, and handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this flow processes.</typeparam>
    /// <typeparam name="TResponse">The type of response this flow produces.</typeparam>
    /// <remarks>
    /// Flows use a fluent API in the <see cref="Configure"/> method to define their pipeline.
    /// The pipeline is built once at startup and cached for optimal runtime performance.
    /// Execution follows this order: Specifications (guards) → Policies (decorators) → Handler (business logic).
    /// </remarks>
    public abstract class FlowDefinition<TRequest, TResponse> : IFlow<TRequest, TResponse>
    {
        private IFlowHandler<TRequest, TResponse>? _pipeline;
        private IFlowSpecification<TRequest>[] _specifications = [];
        private Func<FlowInterrupt<object?>, TResponse>? _interruptMapper;
        private bool _isInitialized;
        private readonly object _initLock = new();

        /// <summary>
        /// Configures the flow pipeline using a fluent builder API.
        /// This method is called once during initialization to define the execution chain.
        /// </summary>
        /// <param name="flow">The builder used to configure specifications, policies, and the handler.</param>
        /// <remarks>
        /// This method must call <see cref="IFlowBuilder{TRequest, TResponse}.Handle{THandler}"/> exactly once.
        /// The order of calls determines the execution order:
        /// <list type="number">
        /// <item><see cref="IFlowBuilder{TRequest, TResponse}.Check{TSpec}"/> - validation/guard logic (executed sequentially)</item>
        /// <item><see cref="IFlowBuilder{TRequest, TResponse}.Use{TPolicy}"/> - cross-cutting concerns (wrapped outer to inner)</item>
        /// <item><see cref="IFlowBuilder{TRequest, TResponse}.Handle{THandler}"/> - main business logic</item>
        /// </list>
        /// </remarks>
        protected abstract void Configure(IFlowBuilder<TRequest, TResponse> flow);

        /// <summary>
        /// Executes the flow pipeline asynchronously.
        /// Ensures the pipeline is initialized, runs all specifications, then invokes the handler chain.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The flow context providing shared state and services.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method uses an optimized hot-path for synchronous specifications (checks <see cref="ValueTask{T}.IsCompletedSuccessfully"/>).
        /// If any specification returns a <see cref="FlowInterrupt{T}"/>, the pipeline stops immediately and returns the mapped response.
        /// Use this overload when executing sub-flows where you want to share the same <see cref="FlowContext"/> (especially FlowId).
        /// For main flow execution, use <see cref="ExecuteAsync(TRequest, IServiceProvider, CancellationToken)"/> instead.
        /// </remarks>
        public ValueTask<TResponse> ExecuteAsync(TRequest request, FlowContext context)
        {
            EnsureInitialized(context.Services);
            IFlowSpecification<TRequest>[] specs = _specifications;
            for (int i = 0; i < specs.Length; i++)
            {
                IFlowSpecification<TRequest> spec = specs[i];
                ValueTask<FlowInterrupt<object?>?> resultTask = spec.CheckAsync(request, context);
                if (!resultTask.IsCompletedSuccessfully)
                {
                    return AwaitSpecification(resultTask, request, context, i);
                }
                FlowInterrupt<object?>? interrupt = resultTask.Result;
                if (interrupt.HasValue)
                {
                    return new ValueTask<TResponse>(MapInterrupt(interrupt.Value));
                }
            }
            return _pipeline!.HandleAsync(request, context);
        }

        /// <summary>
        /// Executes the flow pipeline asynchronously from an HTTP context.
        /// Automatically creates a <see cref="FlowContext"/> with services, cancellation token, and HTTP context from the provided <paramref name="httpContext"/>.
        /// This is a convenience method for ASP.NET Core scenarios where you have <see cref="HttpContext"/> available.
        /// </summary>
        /// <param name="request">The request object containing input data for the flow.</param>
        /// <param name="httpContext">The HTTP context providing services (<see cref="HttpContext.RequestServices"/>), cancellation token (<see cref="HttpContext.RequestAborted"/>), and HTTP metadata.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation that produces the response.</returns>
        /// <remarks>
        /// This method creates a new <see cref="FlowContext"/> with a unique <see cref="FlowContext.FlowId"/> for correlation.
        /// The HTTP context is passed to handlers, allowing access to:
        /// <list type="bullet">
        /// <item>User authentication and claims (<see cref="HttpContext.User"/>)</item>
        /// <item>Request headers, query parameters, cookies (<see cref="HttpContext.Request"/>)</item>
        /// <item>Response control: status codes, headers, cookies (<see cref="HttpContext.Response"/>)</item>
        /// <item>Connection information: IP addresses (<see cref="HttpContext.Connection"/>)</item>
        /// <item>Per-request storage (<see cref="HttpContext.Items"/>)</item>
        /// </list>
        /// Use <see cref="ExecuteAsync(TRequest, FlowContext)"/> when executing sub-flows to share the same <see cref="FlowContext"/> (especially <see cref="FlowContext.FlowId"/>).
        /// </remarks>
        public ValueTask<TResponse> ExecuteAsync(TRequest request, HttpContext httpContext)
        {
            FlowContext context = new FlowContext
            {
                Services = httpContext.RequestServices,
                CancellationToken = httpContext.RequestAborted,
                HttpContext = httpContext
            };
            return ExecuteAsync(request, context);
        }

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
        /// This method creates a new <see cref="FlowContext"/> with a unique <see cref="FlowContext.FlowId"/> for correlation.
        /// The <see cref="FlowContext.HttpContext"/> property will be <c>null</c>.
        /// Use this method when executing flows outside of HTTP requests:
        /// <list type="bullet">
        /// <item>Background services (<see cref="Microsoft.Extensions.Hosting.IHostedService"/>, <see cref="Microsoft.Extensions.Hosting.BackgroundService"/>)</item>
        /// <item>Message queue consumers (RabbitMQ, Azure Service Bus, etc.)</item>
        /// <item>Console applications</item>
        /// <item>Unit/integration tests</item>
        /// <item>Blazor WebAssembly (client-side, no server HttpContext)</item>
        /// </list>
        /// Use <see cref="ExecuteAsync(TRequest, FlowContext)"/> when executing sub-flows to share the same <see cref="FlowContext"/> (especially <see cref="FlowContext.FlowId"/>).
        /// </remarks>
        public ValueTask<TResponse> ExecuteAsync(TRequest request, IServiceProvider serviceProvider, CancellationToken ct)
        {
            FlowContext context = new FlowContext
            {
                Services = serviceProvider,
                CancellationToken = ct,
                HttpContext = null
            };
            return ExecuteAsync(request, context);
        }

        private async ValueTask<TResponse> AwaitSpecification(ValueTask<FlowInterrupt<object?>?> resultTask, TRequest request, FlowContext context, int index)
        {
            FlowInterrupt<object?>? interrupt = await resultTask.ConfigureAwait(false);
            if (interrupt.HasValue)
            {
                return MapInterrupt(interrupt.Value);
            }
            for (int i = index + 1; i < _specifications.Length; i++)
            {
                interrupt = await _specifications[i].CheckAsync(request, context).ConfigureAwait(false);
                if (interrupt.HasValue)
                {
                    return MapInterrupt(interrupt.Value);
                }
            }
            return await _pipeline!.HandleAsync(request, context).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TResponse MapInterrupt(FlowInterrupt<object?> interrupt)
        {
            if (_interruptMapper is not null)
            {
                return _interruptMapper(interrupt);
            }
            if (interrupt.Response is TResponse typed)
            {
                return typed;
            }
            return default!;
        }

        private void EnsureInitialized(IServiceProvider sp)
        {

            if (Volatile.Read(ref _isInitialized))
            {
                return;
            }
            lock (_initLock)
            {
                if (_isInitialized)
                {
                    return;
                }
                Initialize(sp);
                Volatile.Write(ref _isInitialized, true);
            }
        }

        private void Initialize(IServiceProvider serviceProvider)
        {
            FlowBuilder<TRequest, TResponse> builder = new();
            Configure(builder);
            if (builder.Handler is null)
            {
                throw new InvalidOperationException($"Flow {GetType().Name} does not define a Handler.");
            }
            _interruptMapper = builder.InterruptMapper;
            _specifications = builder.Specifications
                .Select(type => 
                    serviceProvider.GetService(type) as IFlowSpecification<TRequest>
                    ?? (IFlowSpecification<TRequest>)ActivatorUtilities.CreateInstance(serviceProvider, type))
                .ToArray();
            _pipeline = BuildPipeline(serviceProvider, builder);
        }

        private static IFlowHandler<TRequest, TResponse> BuildPipeline(IServiceProvider sp, FlowBuilder<TRequest, TResponse> builder)
        {
            IFlowHandler<TRequest, TResponse> handler = sp.GetService(builder.Handler!) as IFlowHandler<TRequest, TResponse> ?? (IFlowHandler<TRequest, TResponse>)ActivatorUtilities.CreateInstance(sp, builder.Handler!);
            IFlowHandler<TRequest, TResponse> current = handler;
            for (int i = builder.Policies.Count - 1; i >= 0; i--)
            {
                FlowPolicy<TRequest, TResponse> policy = sp.GetService(builder.Policies[i]) as FlowPolicy<TRequest, TResponse> ?? (FlowPolicy<TRequest, TResponse>)ActivatorUtilities.CreateInstance(sp, builder.Policies[i]);
                policy.SetNext(current);
                current = policy;
            }
            return current;
        }

        internal void InitializePipeline(IServiceProvider sp) => EnsureInitialized(sp);
    }

}
