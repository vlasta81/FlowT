using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Plugins;
using FlowT.SampleApp.Domain;
using FlowT.SampleApp.Infrastructure;
using System.Text.Json;

namespace FlowT.SampleApp.Features.Users;

/// <summary>
/// Creates a new user
/// Demonstrates safe usage of scoped services in singleton handler via FlowContext
/// </summary>
public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly ILogger<CreateUserHandler> _logger;

    // ✅ Only inject singleton dependencies (ILogger, IConfiguration, etc.)
    // ❌ Never inject scoped dependencies (DbContext, HttpContext, etc.) - use context.Service<T>()
    public CreateUserHandler(ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CreateUserResponse> HandleAsync(
        CreateUserRequest request,
        FlowContext context)
    {
        // ✅ Resolve scoped service per-request from FlowContext
        // This is thread-safe even though handler is singleton
        var userRepo = context.Service<IUserRepository>();

        // PerformancePlugin: track how long the database write takes
        var perf = context.Plugin<IPerformancePlugin>();

        // AuditPlugin: build a structured audit trail for this flow execution
        var audit = context.Plugin<IAuditPlugin>();

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };

        // Measure the repository call — result available in perf.Elapsed after the using block
        using (perf.Measure("db-create-user"))
        {
            await userRepo.CreateAsync(user, context.CancellationToken);
        }

        // Record audit entry — can be flushed to a persistent store after the flow
        audit.Record("UserCreated", new { user.Id, user.Email });

        _logger.LogInformation(
            "User created: {UserId} ({Email}) by flow {FlowId} — db write: {Elapsed}ms",
            user.Id,
            user.Email,
            context.FlowIdString,
            perf.Elapsed["db-create-user"].TotalMilliseconds
        );

        foreach (var entry in audit.Entries)
            _logger.LogDebug("[Audit] {Action} at {Timestamp}", entry.Action, entry.Timestamp);

        // Store created user in context for potential use by subsequent policies
        context.Set(user, key: "created-user");

        // Return response
        return new CreateUserResponse(user.Id, user.Email, user.Name, user.CreatedAt);
    }
}

/// <summary>
/// Gets user by ID
/// Demonstrates retrieving data stored by specification
/// </summary>
public class GetUserHandler : IFlowHandler<GetUserRequest, GetUserResponse>
{
    public ValueTask<GetUserResponse> HandleAsync(GetUserRequest request, FlowContext context)
    {
        // Specification already loaded the user into context - reuse it!
        // This avoids duplicate database query
        if (context.TryGet<User>(out var user, key: "validated-user"))
        {
            return ValueTask.FromResult(new GetUserResponse(
                user.Id,
                user.Email,
                user.Name,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt
            ));
        }

        // Fallback (should not happen if specification ran)
        throw new InvalidOperationException("User not found in context");
    }
}

/// <summary>
/// Lists all users
/// </summary>
public class ListUsersHandler : IFlowHandler<ListUsersRequest, ListUsersResponse>
{
    public async ValueTask<ListUsersResponse> HandleAsync(
        ListUsersRequest request,
        FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();
        var users = await userRepo.GetAllAsync(context.CancellationToken);

        var userDtos = users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.Name,
            u.PhoneNumber,
            u.IsActive
        )).ToList();

        return new ListUsersResponse(userDtos);
    }
}

/// <summary>
/// Updates user
/// </summary>
public class UpdateUserHandler : IFlowHandler<UpdateUserRequest, UpdateUserResponse>
{
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(ILogger<UpdateUserHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<UpdateUserResponse> HandleAsync(
        UpdateUserRequest request,
        FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();

        // Get existing user
        var existingUser = await userRepo.GetByIdAsync(request.Id, context.CancellationToken);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User {request.Id} not found");
        }

        // Update user
        var updatedUser = existingUser with
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepo.UpdateAsync(updatedUser, context.CancellationToken);

        _logger.LogInformation("User updated: {UserId}", request.Id);

        return new UpdateUserResponse(
            updatedUser.Id,
            updatedUser.Name,
            updatedUser.PhoneNumber,
            updatedUser.UpdatedAt ?? DateTimeOffset.UtcNow
        );
    }
}

/// <summary>
/// Deletes user
/// </summary>
public class DeleteUserHandler : IFlowHandler<DeleteUserRequest, DeleteUserResponse>
{
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(ILogger<DeleteUserHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<DeleteUserResponse> HandleAsync(
        DeleteUserRequest request,
        FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();
        var deleted = await userRepo.DeleteAsync(request.Id, context.CancellationToken);

        if (deleted)
        {
            _logger.LogInformation("User deleted: {UserId}", request.Id);
            return new DeleteUserResponse(true, $"User {request.Id} deleted successfully");
        }

        return new DeleteUserResponse(false, $"User {request.Id} not found");
    }
}

/// <summary>
/// Exports all users as a downloadable JSON file.
/// Demonstrates <see cref="FileStreamResponse"/> for binary/file downloads.
/// </summary>
public class ExportUsersHandler : IFlowHandler<ExportUsersRequest, FileStreamResponse>
{
    public async ValueTask<FileStreamResponse> HandleAsync(ExportUsersRequest request, FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();
        var users = await userRepo.GetAllAsync(context.CancellationToken);

        var dtos = users.Select(u => new UserDto(u.Id, u.Email, u.Name, u.PhoneNumber, u.IsActive)).ToList();
        var json = JsonSerializer.SerializeToUtf8Bytes(dtos, new JsonSerializerOptions { WriteIndented = true });
        var stream = new MemoryStream(json);

        return new FileStreamResponse
        {
            Stream = stream,
            ContentType = "application/json",
            FileDownloadName = $"users-{DateTimeOffset.UtcNow:yyyy-MM-dd}.json"
        };
    }
}
