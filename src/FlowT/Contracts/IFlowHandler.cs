
using System.Threading.Tasks;

namespace FlowT.Contracts
{
    /// <summary>
    /// Represents a handler that processes a request and produces a response.
    /// Handlers contain the main business logic of a flow. They can also be wrapped by policies.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this handler processes.</typeparam>
    /// <typeparam name="TResponse">The type of response this handler produces.</typeparam>
    public interface IFlowHandler<in TRequest, TResponse>
    {
        /// <summary>
        /// Handles the request asynchronously and produces a response.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The flow context providing shared state and services.</param>
        /// <returns>A <see cref="ValueTask{TResponse}"/> representing the asynchronous operation that produces the response.</returns>
        ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context);
    }

}
