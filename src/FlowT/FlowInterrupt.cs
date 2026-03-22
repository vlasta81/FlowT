namespace FlowT
{
    /// <summary>
    /// Represents an interruption of flow execution, primarily used for capturing validation failures and business rule violations
    /// from specifications without throwing exceptions. Also supports early returns with successful results.
    /// </summary>
    /// <typeparam name="TResponse">The type of response that can be returned as an early result.</typeparam>
    /// <remarks>
    /// <para>
    /// <see cref="FlowInterrupt{TResponse}"/> provides a type-safe, exception-free mechanism for handling errors in flow pipelines.
    /// It is returned by <see cref="Contracts.IFlowSpecification{TRequest}"/> implementations to signal:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Validation failures</strong> - Invalid input data (e.g., bad email format, missing required fields)</description></item>
    /// <item><description><strong>Business rule violations</strong> - Domain constraints (e.g., duplicate email, insufficient permissions)</description></item>
    /// <item><description><strong>Early successful returns</strong> - Short-circuit pipeline with a valid result (e.g., cached data)</description></item>
    /// </list>
    /// <para>
    /// <strong>Benefits over exceptions:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>No performance overhead of try-catch blocks</description></item>
    /// <item><description>Explicit error flow in method signatures</description></item>
    /// <item><description>Type-safe with compiler guarantees</description></item>
    /// <item><description>Supports HTTP status codes (400, 401, 409, etc.)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Validation failure example
    /// public class ValidateEmailSpec : IFlowSpecification<CreateUserRequest>
    /// {
    ///     public ValueTask<FlowInterrupt<UserResponse>?> CheckAsync(
    ///         CreateUserRequest request, FlowContext context)
    ///     {
    ///         if (!IsValidEmail(request.Email))
    ///         {
    ///             return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(
    ///                 FlowInterrupt<UserResponse>.Fail(
    ///                     "Email format is invalid",
    ///                     StatusCodes.Status400BadRequest
    ///                 )
    ///             );
    ///         }
    ///         
    ///         return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(null);
    ///     }
    /// }
    /// 
    /// // Early return example
    /// public class CheckCacheSpec : IFlowSpecification<GetUserRequest>
    /// {
    ///     public ValueTask<FlowInterrupt<UserResponse>?> CheckAsync(
    ///         GetUserRequest request, FlowContext context)
    ///     {
    ///         var cached = context.TryGet<UserResponse>(out var cachedData);
    ///         if (cached)
    ///         {
    ///             return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(
    ///                 FlowInterrupt<UserResponse>.Stop(cachedData!, StatusCodes.Status200OK)
    ///             );
    ///         }
    ///         
    ///         return ValueTask.FromResult<FlowInterrupt<UserResponse>?>(null);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public readonly struct FlowInterrupt<TResponse>
    {
        /// <summary>
        /// Gets the response value for an early return scenario.
        /// This is <c>null</c> if the interrupt represents a failure.
        /// </summary>
        public TResponse? Response { get; }

        /// <summary>
        /// Gets the error message for a failure scenario.
        /// This is <c>null</c> if the interrupt represents an early return.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets the HTTP-like status code for this interrupt.
        /// Default is 400 for failures and 200 for early returns.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets a value indicating whether this interrupt represents a failure (has an error message).
        /// </summary>
        public bool IsFailure => Message is not null;

        /// <summary>
        /// Gets a value indicating whether this interrupt represents an early return (has a response but no error).
        /// </summary>
        public bool IsEarlyReturn => Response is not null && Message is null;

        private FlowInterrupt(TResponse? response, string? message, int statusCode)
        {
            Response = response;
            Message = message;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a failure interrupt representing a validation error or business rule violation.
        /// This is the primary method for capturing errors from specifications without throwing exceptions.
        /// </summary>
        /// <param name="message">The error message describing why the flow was interrupted (e.g., "Email format is invalid", "User already exists").</param>
        /// <param name="statusCode">The HTTP status code for this failure. Common values:
        /// <list type="bullet">
        /// <item><description>400 - Bad Request (validation failure)</description></item>
        /// <item><description>401 - Unauthorized (authentication required)</description></item>
        /// <item><description>403 - Forbidden (insufficient permissions)</description></item>
        /// <item><description>404 - Not Found (resource doesn't exist)</description></item>
        /// <item><description>409 - Conflict (duplicate data, business rule violation)</description></item>
        /// <item><description>422 - Unprocessable Entity (semantic validation error)</description></item>
        /// </list>
        /// Default is 400 (Bad Request).
        /// </param>
        /// <returns>A <see cref="FlowInterrupt{TResponse}"/> representing a failure that will stop pipeline execution.</returns>
        /// <remarks>
        /// Use <see cref="Fail"/> in specifications to return validation errors without exceptions:
        /// <code><![CDATA[
        /// if (!IsValidEmail(request.Email))
        /// {
        ///     return FlowInterrupt<UserResponse>.Fail(
        ///         "Email format is invalid",
        ///         StatusCodes.Status400BadRequest
        ///     );
        /// }
        /// ]]></code>
        /// </remarks>
        public static FlowInterrupt<TResponse> Fail(string message, int statusCode = 400) => new(default, message, statusCode);

        /// <summary>
        /// Creates an early return interrupt with a successful response value.
        /// This allows specifications or policies to short-circuit the pipeline while returning a valid result,
        /// useful for caching, pre-computed results, or conditional logic.
        /// </summary>
        /// <param name="earlyReturn">The response value to return immediately, bypassing remaining pipeline steps.</param>
        /// <param name="statusCode">The HTTP status code for this early return. Common values:
        /// <list type="bullet">
        /// <item><description>200 - OK (successful response)</description></item>
        /// <item><description>201 - Created (resource created)</description></item>
        /// <item><description>204 - No Content (successful with no response body)</description></item>
        /// <item><description>304 - Not Modified (cached response)</description></item>
        /// </list>
        /// Default is 200 (OK).
        /// </param>
        /// <returns>A <see cref="FlowInterrupt{TResponse}"/> representing an early successful return.</returns>
        /// <remarks>
        /// Use <see cref="Stop"/> for optimization scenarios like returning cached data:
        /// <code><![CDATA[
        /// if (context.TryGet<UserResponse>(out var cached))
        /// {
        ///     return FlowInterrupt<UserResponse>.Stop(
        ///         cached,
        ///         StatusCodes.Status200OK
        ///     );
        /// }
        /// ]]></code>
        /// </remarks>
        public static FlowInterrupt<TResponse> Stop(TResponse earlyReturn, int statusCode = 200) => new(earlyReturn, null, statusCode);
    }
}
