using System;
using FlowT.Abstractions;
using FlowT.Extensions;

namespace FlowT.Attributes
{
    /// <summary>
    /// Marks a class as a flow definition for automatic discovery and registration.
    /// Apply this attribute to classes that inherit from <see cref="FlowDefinition{TRequest, TResponse}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is <strong>required</strong> for automatic registration by <see cref="FlowServiceCollectionExtensions.AddFlows"/>.
    /// Without this attribute, the flow will not be discovered during assembly scanning.
    /// </para>
    /// <para>
    /// <strong>Why required?</strong> Explicit opt-in prevents accidental registration of base classes, 
    /// test fixtures, or internal flows not meant for production use. It also improves code readability
    /// by clearly marking which flows are part of the application's public API.
    /// </para>
    /// <example>
    /// <code>
    /// [FlowDefinition]
    /// public class CreateUserFlow : FlowDefinition&lt;CreateUserRequest, CreateUserResponse&gt;
    /// {
    ///     protected override void Configure(IFlowBuilder&lt;CreateUserRequest, CreateUserResponse&gt; flow)
    ///     {
    ///         flow.Handle&lt;CreateUserHandler&gt;();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FlowDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowDefinitionAttribute"/> class.
        /// </summary>
        public FlowDefinitionAttribute() { }
    }

}
