using System;
using FlowT.Abstractions;
using FlowT.Contracts;

namespace FlowT
{
    /// <summary>
    /// Factory class responsible for creating and initializing flow instances from flow definitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This factory is used internally by the dependency injection system to convert <see cref="FlowDefinition{TRequest, TResponse}"/>
    /// instances into <see cref="IFlow{TRequest, TResponse}"/> implementations. The factory ensures that the flow pipeline
    /// is properly initialized with all specifications, policies, and handlers before the flow is used.
    /// </para>
    /// <para>
    /// The initialization is performed lazily and uses double-check locking to ensure thread-safety while maintaining
    /// high performance. Once initialized, the pipeline is cached and reused across all executions.
    /// </para>
    /// </remarks>
    public static class FlowFactory
    {
        /// <summary>
        /// Creates and initializes a flow instance from a flow definition.
        /// </summary>
        /// <typeparam name="TRequest">The type of request handled by the flow. Must be a non-null reference type.</typeparam>
        /// <typeparam name="TResponse">The type of response returned by the flow. Must be a non-null reference type.</typeparam>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies during pipeline initialization.
        /// This is typically the scoped or root service provider from the DI container.
        /// </param>
        /// <param name="definition">
        /// The flow definition to initialize. This contains the pipeline configuration (specifications, policies, handler)
        /// defined in the <see cref="FlowDefinition{TRequest, TResponse}.Configure"/> method.
        /// </param>
        /// <returns>
        /// An initialized <see cref="IFlow{TRequest, TResponse}"/> instance ready for execution.
        /// The returned instance is the same as the input <paramref name="definition"/>, but with the pipeline initialized.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method calls <see cref="FlowDefinition{TRequest, TResponse}.InitializePipeline"/> which builds the execution pipeline
        /// by chaining together specifications, policies, and the handler. The initialization happens only once per flow instance
        /// using lazy initialization with thread-safe double-check locking.
        /// </para>
        /// <para>
        /// <strong>Performance:</strong> Pipeline initialization is expensive (reflection, expression compilation), but it only
        /// happens once per flow type. Subsequent executions use the cached pipeline, making FlowT 9-10x faster than alternatives
        /// that rebuild pipelines on every request.
        /// </para>
        /// <example>
        /// This method is typically called automatically by the DI system:
        /// <code>
        /// services.AddSingleton&lt;IFlow&lt;CreateUserRequest, CreateUserResponse&gt;&gt;(sp =&gt;
        /// {
        ///     var definition = sp.GetRequiredService&lt;CreateUserFlow&gt;();
        ///     return FlowFactory.Create(sp, definition);
        /// });
        /// </code>
        /// </example>
        /// </remarks>
        public static IFlow<TRequest, TResponse> Create<TRequest, TResponse>(IServiceProvider serviceProvider, FlowDefinition<TRequest, TResponse> definition) where TRequest : notnull where TResponse : notnull
        {
            definition.InitializePipeline(serviceProvider);
            return definition;
        }
    }

}
