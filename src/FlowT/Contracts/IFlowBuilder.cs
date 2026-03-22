using System;
using FlowT.Abstractions;

namespace FlowT.Contracts
{
    /// <summary>
    /// Fluent API builder for configuring a flow pipeline.
    /// Use this interface in <see cref="FlowDefinition{TRequest, TResponse}.Configure"/> to define the execution chain.
    /// </summary>
    /// <typeparam name="TRequest">The type of request processed by this flow.</typeparam>
    /// <typeparam name="TResponse">The type of response produced by this flow.</typeparam>
    public interface IFlowBuilder<TRequest, TResponse>
    {
        /// <summary>
        /// Adds a specification (guard) to the pipeline.
        /// Specifications are executed sequentially before the handler. If any specification returns an interrupt, the pipeline stops.
        /// </summary>
        /// <typeparam name="TSpec">The type of specification to add.</typeparam>
        /// <returns>The builder instance for method chaining.</returns>
        IFlowBuilder<TRequest, TResponse> Check<TSpec>() where TSpec : IFlowSpecification<TRequest>;

        /// <summary>
        /// Adds a policy (decorator) to the pipeline.
        /// Policies wrap the handler (and other policies) to provide cross-cutting concerns like logging, transactions, retry, etc.
        /// Policies are applied in the order they are added (outer to inner).
        /// </summary>
        /// <typeparam name="TPolicy">The type of policy to add.</typeparam>
        /// <returns>The builder instance for method chaining.</returns>
        IFlowBuilder<TRequest, TResponse> Use<TPolicy>() where TPolicy : IFlowPolicy<TRequest, TResponse>;

        /// <summary>
        /// Sets the main handler for this flow.
        /// The handler contains the core business logic. This method must be called exactly once per flow.
        /// </summary>
        /// <typeparam name="THandler">The type of handler to use.</typeparam>
        /// <returns>The builder instance for method chaining.</returns>
        IFlowBuilder<TRequest, TResponse> Handle<THandler>() where THandler : IFlowHandler<TRequest, TResponse>;

        /// <summary>
        /// Registers a mapper function to convert <see cref="FlowInterrupt{T}"/> results from specifications to typed responses.
        /// Invoked only when a <see cref="IFlowSpecification{TRequest}"/> added via <see cref="Check{TSpec}"/> returns a non-null interrupt.
        /// Exceptions thrown by policies or the handler propagate normally and are <b>not</b> caught by this mapper.
        /// Can only be called once per flow.
        /// </summary>
        /// <param name="mapper">
        /// A function that receives the interrupt (with <see cref="FlowInterrupt{T}.Message"/>, <see cref="FlowInterrupt{T}.StatusCode"/>,
        /// <see cref="FlowInterrupt{T}.IsFailure"/> and <see cref="FlowInterrupt{T}.IsEarlyReturn"/>) and must return a <typeparamref name="TResponse"/>.
        /// May throw instead of returning when the response type cannot carry error information.
        /// </param>
        /// <returns>The builder instance for method chaining.</returns>
        IFlowBuilder<TRequest, TResponse> OnInterrupt(Func<FlowInterrupt<object?>, TResponse> mapper);
    }
}
