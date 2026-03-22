
using System;
using FlowT.Contracts;
using FlowT.Extensions;

namespace FlowT.Attributes
{
    /// <summary>
    /// Provides OpenAPI/Swagger metadata for flow endpoints.
    /// Apply this attribute to flow classes to customize their documentation in API explorers.
    /// </summary>
    /// <remarks>
    /// This attribute is read by <see cref="FlowEndpointExtensions.MapFlow{TFlow, TRequest, TResponse}"/> to apply metadata to generated endpoints.
    /// The actual HTTP route and method are defined in the module's <see cref="IFlowModule.MapEndpoints"/> method.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FlowEndpointInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the summary description for the endpoint (short one-line description).
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the detailed description for the endpoint (can be multi-line).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the tags used to group endpoints in API documentation.
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowEndpointInfoAttribute"/> class with the specified tags.
        /// </summary>
        /// <param name="tags">The tags to apply to the endpoint for grouping in API documentation.</param>
        public FlowEndpointInfoAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }

}
