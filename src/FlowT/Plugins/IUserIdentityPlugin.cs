using FlowT.Abstractions;
using System;
using System.Security.Claims;

namespace FlowT.Plugins
{
    /// <summary>
    /// Built-in plugin that exposes authenticated user identity for the current flow execution.
    /// Claims are resolved once from <see cref="FlowContext.HttpContext"/> on first access and
    /// cached for the lifetime of the flow. Returns <c>null</c> / <c>false</c> in non-HTTP scenarios.
    /// </summary>
    /// <remarks>
    /// Register via <c>services.AddFlowPlugin&lt;IUserIdentityPlugin, UserIdentityPlugin&gt;()</c>.
    /// <para>
    /// Usage:
    /// <code>
    /// var identity = context.Plugin&lt;IUserIdentityPlugin&gt;();
    /// if (!identity.IsAuthenticated)
    ///     return FlowInterrupt&lt;object?&gt;.Fail("Unauthorized", 401);
    ///
    /// var userId = identity.UserId;         // Guid? from NameIdentifier claim
    /// var email  = identity.Email;          // string? from Email claim
    /// var isAdmin = identity.IsInRole("Admin");
    /// </code>
    /// </para>
    /// </remarks>
    public interface IUserIdentityPlugin
    {
        /// <summary>
        /// Gets the authenticated user's ID parsed from <see cref="ClaimTypes.NameIdentifier"/>,
        /// or <c>null</c> when unauthenticated or in a non-HTTP scenario.
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the authenticated user's e-mail address parsed from <see cref="ClaimTypes.Email"/>,
        /// or <c>null</c> when the claim is absent or in a non-HTTP scenario.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// Returns <c>false</c> in non-HTTP scenarios.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the raw <see cref="ClaimsPrincipal"/> for the current request,
        /// or <c>null</c> in non-HTTP scenarios.
        /// </summary>
        ClaimsPrincipal? Principal { get; }

        /// <summary>
        /// Determines whether the current user belongs to the specified role.
        /// Returns <c>false</c> in non-HTTP scenarios or when unauthenticated.
        /// </summary>
        /// <param name="role">The role name to check.</param>
        bool IsInRole(string role);
    }

    /// <summary>
    /// Default implementation of <see cref="IUserIdentityPlugin"/>.
    /// Inherits from <see cref="FlowPlugin"/> so that <see cref="FlowContext"/> is injected automatically.
    /// </summary>
    public class UserIdentityPlugin : FlowPlugin, IUserIdentityPlugin
    {
        private ClaimsPrincipal? _principal;
        private bool _resolved;

        private ClaimsPrincipal? ResolvePrincipal()
        {
            if (!_resolved)
            {
                _principal = Context.HttpContext?.User;
                _resolved = true;
            }
            return _principal;
        }

        /// <inheritdoc />
        public ClaimsPrincipal? Principal => ResolvePrincipal();

        /// <inheritdoc />
        public Guid? UserId
        {
            get
            {
                string? raw = ResolvePrincipal()?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(raw, out var id) ? id : null;
            }
        }

        /// <inheritdoc />
        public string? Email => ResolvePrincipal()?.FindFirstValue(ClaimTypes.Email);

        /// <inheritdoc />
        public bool IsAuthenticated => ResolvePrincipal()?.Identity?.IsAuthenticated ?? false;

        /// <inheritdoc />
        public bool IsInRole(string role) => ResolvePrincipal()?.IsInRole(role) ?? false;
    }
}
