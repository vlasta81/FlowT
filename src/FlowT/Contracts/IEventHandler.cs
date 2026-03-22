using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Contracts
{
    /// <summary>
    /// Represents a handler for domain events published via <see cref="FlowContext.PublishAsync{TEvent}"/>.
    /// Multiple handlers can be registered for the same event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event this handler processes.</typeparam>
    public interface IEventHandler<in TEvent>
    {
        /// <summary>
        /// Handles the event asynchronously.
        /// </summary>
        /// <param name="eventData">The event data to process.</param>
        /// <param name="cancellationToken">A cancellation token to observe.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken);
    }

}
