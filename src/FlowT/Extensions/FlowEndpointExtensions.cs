using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Reflection;
using System.Text.Json;

namespace FlowT.Extensions
{
    /// <summary>
    /// Extension methods for mapping flows to HTTP endpoints.
    /// </summary>
    public static class FlowEndpointExtensions
    {
        /// <summary>
        /// Maps a flow to an HTTP endpoint with automatic request binding and response serialization.
        /// The flow is resolved from dependency injection and executed with a new <see cref="FlowContext"/>.
        /// </summary>
        /// <typeparam name="TFlow">The type of flow to execute (must implement <see cref="IFlow{TRequest, TResponse}"/>).</typeparam>
        /// <typeparam name="TRequest">The type of request the flow processes.</typeparam>
        /// <typeparam name="TResponse">The type of response the flow produces.</typeparam>
        /// <param name="app">The endpoint route builder.</param>
        /// <param name="pattern">The route pattern (e.g., "/users", "/orders/{id}").</param>
        /// <param name="httpMethod">The HTTP method (e.g., "GET", "POST", "PUT", "DELETE").</param>
        /// <returns>A <see cref="RouteHandlerBuilder"/> for further endpoint configuration (authorization, tags, etc.).</returns>
        /// <remarks>
        /// <para>
        /// The method automatically applies metadata from <see cref="Attributes.FlowEndpointInfoAttribute"/> if present on the flow class.
        /// Request parameters are bound from route values, query string, or request body based on ASP.NET Core conventions.
        /// </para>
        /// <para><strong>Automatic Response Dispatch:</strong></para>
        /// <para>
        /// <c>MapFlow</c> inspects <typeparamref name="TResponse"/> at registration time and selects the correct
        /// serialization path — no manual <c>Results.Stream()</c> or <c>Results.File()</c> is needed:
        /// <list type="bullet">
        /// <item><description><strong>Standard response</strong> — serialized as JSON via <c>Results.Json()</c></description></item>
        /// <item><description><strong><see cref="IStreamableResponse"/></strong> (e.g. <see cref="PagedStreamResponse{T}"/>) — <c>Results.Stream()</c> with chunked transfer encoding and progressive delivery</description></item>
        /// <item><description><strong><see cref="FileStreamResponse"/></strong> — <c>Results.File()</c> with Content-Disposition, ETag, Last-Modified, and range request support</description></item>
        /// </list>
        /// </para>
        /// <para><strong>Example:</strong></para>
        /// <code>
        /// // ✅ Standard — JSON serialized automatically
        /// app.MapFlow&lt;GetOrderFlow, GetOrderRequest, GetOrderResponse&gt;("/orders/{id}", "GET");
        /// 
        /// // ✅ Streaming — Results.Stream() invoked automatically (no WriteToStreamAsync boilerplate)
        /// app.MapFlow&lt;StreamProductsFlow, StreamProductsRequest, PagedStreamResponse&lt;ProductDto&gt;&gt;("/products/stream", "GET");
        /// 
        /// // ✅ File download — Results.File() invoked automatically (no manual stream wiring)
        /// app.MapFlow&lt;ExportUsersFlow, ExportUsersRequest, FileStreamResponse&gt;("/users/export", "GET");
        /// </code>
        /// <para>All three forms are compatible with route groups and support further chaining:</para>
        /// <code>
        /// var api = app.MapGroup("/api").RequireAuthorization();
        /// api.MapFlow&lt;StreamProductsFlow, StreamProductsRequest, PagedStreamResponse&lt;ProductDto&gt;&gt;("/products/stream", "GET")
        ///    .WithSummary("Stream paginated products")
        ///    .WithTags("Products");
        /// </code>
        /// </remarks>
        public static RouteHandlerBuilder MapFlow<TFlow, TRequest, TResponse>(this IEndpointRouteBuilder app, string pattern, string httpMethod) where TFlow : IFlow<TRequest, TResponse> where TRequest : notnull where TResponse : notnull
        {
            bool isStreamable = typeof(IStreamableResponse).IsAssignableFrom(typeof(TResponse));
            bool isFileStream = typeof(TResponse) == typeof(FileStreamResponse);
            RouteHandlerBuilder builder = isFileStream
                ? MapFileStreamFlow<TFlow, TRequest, TResponse>(app, pattern, httpMethod)
                : isStreamable
                    ? MapStreamableFlow<TFlow, TRequest, TResponse>(app, pattern, httpMethod)
                    : MapStandardFlow<TFlow, TRequest, TResponse>(app, pattern, httpMethod);
            ApplyMetadata(builder, typeof(TFlow));
            return builder;
        }

        private static RouteHandlerBuilder MapFileStreamFlow<TFlow, TRequest, TResponse>(IEndpointRouteBuilder app, string pattern, string httpMethod) where TFlow : IFlow<TRequest, TResponse> where TRequest : notnull where TResponse : notnull
        {
            return app.MapMethods(pattern, new[] { httpMethod }, async (TRequest request, TFlow flow, HttpContext http) =>
            {
                TResponse response = await flow.ExecuteAsync(request, http);
                FileStreamResponse fileResponse = (FileStreamResponse)(object)response!;
                return Results.File(
                    fileResponse.Stream,
                    contentType: fileResponse.ContentType,
                    fileDownloadName: fileResponse.FileDownloadName,
                    lastModified: fileResponse.LastModified,
                    entityTag: fileResponse.EntityTag != null ? new Microsoft.Net.Http.Headers.EntityTagHeaderValue(fileResponse.EntityTag) : null,
                    enableRangeProcessing: fileResponse.EnableRangeProcessing
                );
            });
        }

        private static RouteHandlerBuilder MapStreamableFlow<TFlow, TRequest, TResponse>(IEndpointRouteBuilder app, string pattern, string httpMethod) where TFlow : IFlow<TRequest, TResponse> where TRequest : notnull where TResponse : notnull
        {
            return app.MapMethods(pattern, new[] { httpMethod }, async (TRequest request, TFlow flow, HttpContext http) =>
            {
                TResponse response = await flow.ExecuteAsync(request, http);
                return Results.Stream(async (outputStream) =>
                {
                    await using Utf8JsonWriter writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions
                    {
                        Indented = false
                    });
                    IStreamableResponse streamable = (IStreamableResponse)(object)response!;
                    await streamable.WriteToStreamAsync(writer, http.RequestAborted);
                    await writer.FlushAsync(http.RequestAborted);
                }, contentType: "application/json");
            });
        }

        private static RouteHandlerBuilder MapStandardFlow<TFlow, TRequest, TResponse>(IEndpointRouteBuilder app, string pattern, string httpMethod) where TFlow : IFlow<TRequest, TResponse> where TRequest : notnull where TResponse : notnull
        {
            return app.MapMethods(pattern, new[] { httpMethod }, (TRequest request, TFlow flow, HttpContext http) => flow.ExecuteAsync(request, http));
        }

        private static void ApplyMetadata(RouteHandlerBuilder builder, Type flowType)
        {
            Attributes.FlowEndpointInfoAttribute? info = flowType.GetCustomAttribute<Attributes.FlowEndpointInfoAttribute>();
            if (info is null)
            {
                return;
            }
            if (info.Tags.Length > 0)
            {
                builder.WithTags(info.Tags);
            }
            if (!string.IsNullOrEmpty(info.Summary))
            {
                builder.WithSummary(info.Summary);
            }
            if (!string.IsNullOrEmpty(info.Description))
            {
                builder.WithDescription(info.Description);
            }
        }
    }

}
