using System.Threading.Tasks;
using FlowT.Contracts;

namespace FlowT.Abstractions
{
    /// <summary>
    /// Abstract base class for implementing flow policies (decorators) using the Chain of Responsibility pattern.
    /// Policies wrap handlers to provide cross-cutting concerns such as logging, transactions, caching, retry logic, etc.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this policy processes.</typeparam>
    /// <typeparam name="TResponse">The type of response this policy produces.</typeparam>
    /// <remarks>
    /// Policies are chained together at startup. Each policy receives a reference to the next handler in the chain via <see cref="Next"/>.
    /// Override <see cref="HandleAsync"/> to implement your policy logic, typically wrapping a call to <c>await Next.HandleAsync(request, context)</c>.
    /// </remarks>
    public abstract class FlowPolicy<TRequest, TResponse> : IFlowPolicy<TRequest, TResponse>
    {
        /// <summary>
        /// Gets the next handler in the pipeline chain.
        /// This is set automatically by the framework during pipeline construction.
        /// Call this from your <see cref="HandleAsync"/> implementation to continue the pipeline.
        /// </summary>
        protected IFlowHandler<TRequest, TResponse> Next = default!;

        /// <summary>
        /// Sets the next handler in the chain. This method is called by the framework during pipeline construction.
        /// </summary>
        /// <param name="next">The next handler to call in the pipeline.</param>
        /// <remarks>
        /// This method is <c>protected internal</c> to allow test scenarios where policies need to be manually chained.
        /// In production, this is called automatically by the framework.
        /// </remarks>
        protected internal void SetNext(IFlowHandler<TRequest, TResponse> next)
        {
            Next = next;
        }

        /// <summary>
        /// Handles the request, applying policy logic before and/or after calling the next handler.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The flow context providing shared state and services.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation.</returns>
        public abstract ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context);
    }

}
