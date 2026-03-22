
using FlowT.Abstractions;

namespace FlowT.Contracts
{
    /// <summary>
    /// Marker interface for flow policies.
    /// Policies are decorators that wrap handlers to provide cross-cutting concerns such as logging, transactions, retry, caching, etc.
    /// Policies form a chain of responsibility where each policy can execute logic before and after calling the next handler in the chain.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this policy processes.</typeparam>
    /// <typeparam name="TResponse">The type of response this policy produces.</typeparam>
    /// <remarks>
    /// To implement a policy, inherit from <see cref="FlowPolicy{TRequest, TResponse}"/> instead of implementing this interface directly.
    /// </remarks>
    public interface IFlowPolicy<in TRequest, TResponse> : IFlowHandler<TRequest, TResponse> { }

}
