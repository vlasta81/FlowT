
using System;
using FlowT.Contracts;
using FlowT.Extensions;

namespace FlowT.Attributes
{
    /// <summary>
    /// Marks a class as a flow module for automatic discovery and registration.
    /// Apply this attribute to classes that implement <see cref="IFlowModule"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is <strong>required</strong> for automatic registration by <see cref="FlowServiceCollectionExtensions.AddFlowModules"/>.
    /// Without this attribute, the module will not be discovered during assembly scanning.
    /// </para>
    /// <para>
    /// <strong>Why required?</strong> Explicit opt-in provides clear intent and prevents accidental registration
    /// of helper classes or test modules. It enables vertical slice architecture by clearly identifying feature boundaries.
    /// </para>
    /// <para>
    /// Modules provide a way to organize flows, services, and endpoints using vertical slice architecture.
    /// Each module encapsulates all code for a specific feature or bounded context.
    /// </para>
    /// <example>
    /// <code>
    /// [FlowModule]
    /// public class UserModule : IFlowModule
    /// {
    ///     public void Register(IServiceCollection services)
    ///     {
    ///         services.AddFlows(typeof(UserModule).Assembly);
    ///         services.AddScoped&lt;IUserRepository, UserRepository&gt;();
    ///     }
    ///     
    ///     public void MapEndpoints(IEndpointRouteBuilder app)
    ///     {
    ///         app.MapPost("/api/users", ...);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FlowModuleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowModuleAttribute"/> class.
        /// </summary>
        public FlowModuleAttribute() { }
    }

}
