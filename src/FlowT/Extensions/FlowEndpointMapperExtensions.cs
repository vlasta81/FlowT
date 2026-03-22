using FlowT.Contracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Extensions
{
    /// <summary>
    /// Extension methods for automatically mapping flow module endpoints to the application's route builder.
    /// </summary>
    public static class FlowEndpointMapperExtensions
    {
        /// <summary>
        /// Maps all registered flow modules' endpoints to the application.
        /// This method enumerates all <see cref="IFlowModule"/> instances registered in the dependency injection container
        /// and calls their <see cref="IFlowModule.MapEndpoints"/> method to register HTTP endpoints.
        /// </summary>
        /// <param name="app">The endpoint route builder where endpoints will be mapped.</param>
        /// <remarks>
        /// This method should be called in the application's startup/configuration after services are registered.
        /// It provides a centralized way to discover and map all flow module endpoints without explicitly
        /// calling each module's mapping method individually.
        /// <para>
        /// Example usage:
        /// <code>
        /// var app = builder.Build();
        /// app.MapFlowModules(); // Automatically maps all module endpoints
        /// app.Run();
        /// </code>
        /// </para>
        /// </remarks>
        public static void MapFlowModules(this IEndpointRouteBuilder app)
        {
            foreach (IFlowModule module in app.ServiceProvider.GetServices<IFlowModule>())
            {
                module.MapEndpoints(app);
            }
        }
    }

}
