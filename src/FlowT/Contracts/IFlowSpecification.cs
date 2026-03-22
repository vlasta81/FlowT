
using System.Threading.Tasks;

namespace FlowT.Contracts
{
    /// <summary>
    /// Represents a guard specification that validates a request before it reaches the handler.
    /// Specifications implement business rules and validation logic.
    /// If validation fails, the specification returns a <see cref="FlowInterrupt{TResponse}"/> which stops pipeline execution.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to validate.</typeparam>
    public interface IFlowSpecification<in TRequest>
    {
        /// <summary>
        /// Checks whether the request satisfies this specification.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="context">The flow context providing access to shared state and services.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> containing either:
        /// <list type="bullet">
        /// <item><c>null</c> if validation passes (pipeline continues),</item>
        /// <item>or a <see cref="FlowInterrupt{T}"/> if validation fails (pipeline stops).</item>
        /// </list>
        /// </returns>
        ValueTask<FlowInterrupt<object?>?> CheckAsync(TRequest request, FlowContext context);
    }

}
