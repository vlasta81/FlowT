using System.Threading;
using FlowT.Abstractions;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that tracks retry attempt count across all pipeline stages within a single flow execution.
    /// Because the plugin is PerFlow, the counter is shared between the retry policy and any other stage that inspects it.
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IRetryStatePlugin, RetryStatePlugin&gt;()</c>.
    /// <para>
    /// Typical usage in a retry policy:
    /// <code>
    /// public class RetryPolicy : FlowPolicy&lt;TRequest, TResponse&gt;
    /// {
    ///     public override async ValueTask&lt;TResponse&gt; HandleAsync(TRequest request, FlowContext context)
    ///     {
    ///         var retry = context.Plugin&lt;IRetryStatePlugin&gt;();
    ///         while (retry.ShouldRetry(maxAttempts: 3))
    ///         {
    ///             retry.RegisterAttempt();
    ///             try   { return await Next!.HandleAsync(request, context); }
    ///             catch { if (!retry.ShouldRetry(3)) throw; }
    ///         }
    ///         return await Next!.HandleAsync(request, context);
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public interface IRetryStatePlugin
    {
        /// <summary>
        /// Gets the number of attempts registered so far.
        /// Starts at zero before any call to <see cref="RegisterAttempt"/>.
        /// </summary>
        int AttemptNumber { get; }

        /// <summary>
        /// Determines whether another retry is allowed given the maximum number of attempts.
        /// Returns <c>true</c> when <see cref="AttemptNumber"/> is less than <paramref name="maxAttempts"/>.
        /// </summary>
        /// <param name="maxAttempts">Maximum total attempts permitted.</param>
        bool ShouldRetry(int maxAttempts);

        /// <summary>
        /// Atomically registers a new attempt, incrementing <see cref="AttemptNumber"/> by one.
        /// Thread-safe via <see cref="Interlocked.Increment(ref int)"/>.
        /// </summary>
        void RegisterAttempt();
    }

    /// <summary>
    /// Default implementation of <see cref="IRetryStatePlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class RetryStatePlugin : FlowPlugin, IRetryStatePlugin
    {
        private int _attempt;

        /// <inheritdoc />
        public int AttemptNumber => _attempt;

        /// <inheritdoc />
        public bool ShouldRetry(int maxAttempts) => _attempt < maxAttempts;

        /// <inheritdoc />
        public void RegisterAttempt() => Interlocked.Increment(ref _attempt);
    }
}
