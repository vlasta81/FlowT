using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace FlowT.Contracts
{
    /// <summary>
    /// Represents a modular unit that groups related flows, services, and endpoint mappings.
    /// Modules provide a way to organize features using vertical slice architecture.
    /// </summary>
    public interface IFlowModule
    {
        /// <summary>
        /// Registers flows and services required by this module into the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to register services into.</param>
        void Register(IServiceCollection services);

        /// <summary>
        /// Maps HTTP endpoints for flows defined in this module.
        /// </summary>
        /// <param name="app">The endpoint route builder to map endpoints to.</param>
        void MapEndpoints(IEndpointRouteBuilder app);
    }

}
