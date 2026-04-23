using System.Threading.Tasks;
using FlowT.Contracts;

namespace FlowT.Abstractions
{
    /// <summary>
    /// Optional abstract base class for implementing flow specifications.
    /// Provides the <see cref="Continue"/>, <see cref="Fail"/>, and <see cref="Stop"/> helpers
    /// to avoid verbose <c>ValueTask.FromResult&lt;FlowInterrupt&lt;object?&gt;?&gt;(...)</c> boilerplate.
    /// </summary>
    /// <typeparam name="TRequest">The type of request this specification validates.</typeparam>
    /// <remarks>
    /// <para>
    /// Implementing <see cref="IFlowSpecification{TRequest}"/> directly is always valid.
    /// Inherit from this class only when the helper methods simplify your implementation.
    /// </para>
    /// <para>
    /// <see cref="Continue"/> is backed by a <c>static readonly</c> cached field — calling it never allocates.
    /// </para>
    /// <example>
    /// <code><![CDATA[
    /// public class ValidateEmailSpec : FlowSpecification<CreateUserRequest>
    /// {
    ///     public override ValueTask<FlowInterrupt<object?>?> CheckAsync(
    ///         CreateUserRequest request, FlowContext context)
    ///     {
    ///         if (!IsValidEmail(request.Email))
    ///             return Fail("Email format is invalid", 400);
    ///
    ///         return Continue();
    ///     }
    /// }
    ///
    /// public class CheckCacheSpec : FlowSpecification<GetUserRequest>
    /// {
    ///     public override ValueTask<FlowInterrupt<object?>?> CheckAsync(
    ///         GetUserRequest request, FlowContext context)
    ///     {
    ///         if (context.TryGet<UserResponse>(out var cached))
    ///             return Stop(cached, 200);
    ///
    ///         return Continue();
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    /// </remarks>
    public abstract class FlowSpecification<TRequest> : IFlowSpecification<TRequest>
    {
        /// <summary>
        /// Cached completed task representing a passing result — <c>null</c> interrupt, zero allocation.
        /// </summary>
        private static readonly ValueTask<FlowInterrupt<object?>?> _continue = ValueTask.FromResult<FlowInterrupt<object?>?>(null);

        /// <inheritdoc />
        public abstract ValueTask<FlowInterrupt<object?>?> CheckAsync(TRequest request, FlowContext context);

        /// <summary>
        /// Returns a completed <see cref="ValueTask"/> signalling that validation passed
        /// and the pipeline should continue to the next step.
        /// This value is cached — calling this method never allocates.
        /// </summary>
        protected static ValueTask<FlowInterrupt<object?>?> Continue() => _continue;

        /// <summary>
        /// Returns a completed <see cref="ValueTask"/> wrapping a <see cref="FlowInterrupt{T}.Fail"/> interrupt,
        /// stopping the pipeline with the given error message and HTTP status code.
        /// </summary>
        /// <param name="message">The error message describing the validation failure (e.g. <c>"Email format is invalid"</c>).</param>
        /// <param name="statusCode">
        /// The HTTP status code for this failure. Common values:
        /// <list type="bullet">
        /// <item><description>400 — Bad Request (validation failure)</description></item>
        /// <item><description>401 — Unauthorized</description></item>
        /// <item><description>403 — Forbidden</description></item>
        /// <item><description>404 — Not Found</description></item>
        /// <item><description>409 — Conflict (business rule violation)</description></item>
        /// </list>
        /// Default is 400.
        /// </param>
        protected static ValueTask<FlowInterrupt<object?>?> Fail(string message, int statusCode = 400) =>
            ValueTask.FromResult<FlowInterrupt<object?>?>(FlowInterrupt<object?>.Fail(message, statusCode));

        /// <summary>
        /// Returns a completed <see cref="ValueTask"/> wrapping a <see cref="FlowInterrupt{T}.Stop"/> interrupt,
        /// short-circuiting the pipeline with a successful early response.
        /// </summary>
        /// <param name="earlyReturn">The response value to return immediately, bypassing remaining pipeline steps.</param>
        /// <param name="statusCode">
        /// The HTTP status code for this early return. Common values:
        /// <list type="bullet">
        /// <item><description>200 — OK</description></item>
        /// <item><description>201 — Created</description></item>
        /// <item><description>204 — No Content</description></item>
        /// <item><description>304 — Not Modified (cached response)</description></item>
        /// </list>
        /// Default is 200.
        /// </param>
        protected static ValueTask<FlowInterrupt<object?>?> Stop(object? earlyReturn, int statusCode = 200) =>
            ValueTask.FromResult<FlowInterrupt<object?>?>(FlowInterrupt<object?>.Stop(earlyReturn, statusCode));
    }
}
