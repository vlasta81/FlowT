using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.Extensions;
using FlowT.SampleApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

namespace FlowT.SampleApp.Features.Users;

/// <summary>
/// User module - demonstrates modular architecture with IFlowModule
/// Auto-discovered by AddFlowModules() due to [FlowModule] attribute
/// </summary>
[FlowModule]
public class UserModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        // ✅ Register flows explicitly (recommended for clarity)
        services.AddFlow<CreateUserFlow, CreateUserRequest, CreateUserResponse>();
        services.AddFlow<GetUserFlow, GetUserRequest, GetUserResponse>();
        services.AddFlow<ListUsersFlow, ListUsersRequest, ListUsersResponse>();
        services.AddFlow<UpdateUserFlow, UpdateUserRequest, UpdateUserResponse>();
        services.AddFlow<DeleteUserFlow, DeleteUserRequest, DeleteUserResponse>();
        services.AddFlow<ExportUsersFlow, ExportUsersRequest, FileStreamResponse>();

        // ✅ Register external dependencies (handlers/specs/policies auto-registered by FlowT)
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users")
            .WithTags("Users");

        // POST /api/users - Create user
        users.MapPost("/", async (
            [FromBody] CreateUserRequest request,
            CreateUserFlow flow,
            HttpContext httpContext) =>
        {
            // ✅ ExecuteAsync creates FlowContext automatically from HttpContext
            var result = await flow.ExecuteAsync(request, httpContext);
            return Results.Created($"/api/users/{result.Id}", result);
        })
        .WithName("CreateUser")
        .WithSummary("Create a new user")
        .Produces<CreateUserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        // GET /api/users - List all users
        users.MapGet("/", async (
            ListUsersFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(new ListUsersRequest(), httpContext);
            return Results.Ok(result);
        })
        .WithName("ListUsers")
        .WithSummary("Get all users")
        .Produces<ListUsersResponse>();

        // GET /api/users/{id} - Get user by ID
        users.MapGet("/{id:guid}", async (
            Guid id,
            GetUserFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(new GetUserRequest(id), httpContext);
            return Results.Ok(result);
        })
        .WithName("GetUser")
        .WithSummary("Get user by ID")
        .Produces<GetUserResponse>()
        .Produces(StatusCodes.Status404NotFound);

        // PUT /api/users/{id} - Update user
        users.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUserRequest request,
            UpdateUserFlow flow,
            HttpContext httpContext) =>
        {
            // Ensure ID in route matches ID in body
            if (id != request.Id)
            {
                return Results.BadRequest(new { error = "ID mismatch" });
            }

            var result = await flow.ExecuteAsync(request, httpContext);
            return Results.Ok(result);
        })
        .WithName("UpdateUser")
        .WithSummary("Update user")
        .Produces<UpdateUserResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/users/{id} - Delete user
        users.MapDelete("/{id:guid}", async (
            Guid id,
            DeleteUserFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(new DeleteUserRequest(id), httpContext);
            return result.Success ? Results.NoContent() : Results.NotFound(result);
        })
        .WithName("DeleteUser")
        .WithSummary("Delete user")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/users/export
        // ✅ MapFlow auto-detects FileStreamResponse and calls Results.File() internally
        users.MapFlow<ExportUsersFlow, ExportUsersRequest, FileStreamResponse>("/export", "GET")
            .WithName("ExportUsers")
            .WithSummary("Export all users as a downloadable JSON file (FileStreamResponse demo)");
    }
}
