using FlowT.Contracts;
using FlowT.SampleApp.Domain;
using FlowT.SampleApp.Infrastructure;

namespace FlowT.SampleApp.Features.Users;

/// <summary>
/// Validates email format and uniqueness
/// Demonstrates FlowInterrupt for type-safe error handling
/// </summary>
public class ValidateEmailSpecification : IFlowSpecification<CreateUserRequest>
{
    public async ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateUserRequest request,
        FlowContext context)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return FlowInterrupt<object?>.Fail(
                "Invalid email format",
                StatusCodes.Status400BadRequest
            );
        }

        // Check email uniqueness using scoped service from context
        var userRepo = context.Service<IUserRepository>();
        var existingUser = await userRepo.GetByEmailAsync(request.Email, context.CancellationToken);

        if (existingUser != null)
        {
            return FlowInterrupt<object?>.Fail(
                $"User with email '{request.Email}' already exists",
                StatusCodes.Status409Conflict
            );
        }

        // Store validation result in context with named key
        context.Set(true, key: "email-validation:passed");

        // Validation passed
        return null;
    }
}

/// <summary>
/// Validates that required fields are present
/// </summary>
public class ValidateRequiredFieldsSpecification : IFlowSpecification<CreateUserRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateUserRequest request,
        FlowContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail(
                    "Name is required",
                    StatusCodes.Status400BadRequest
                )
            );
        }

        if (request.Name.Length < 2)
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail(
                    "Name must be at least 2 characters",
                    StatusCodes.Status400BadRequest
                )
            );
        }

        // Store validation metadata
        context.Set(request.Name.Length, key: "validation:name-length");

        return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
    }
}

/// <summary>
/// Validates that user exists for Get/Update/Delete operations
/// </summary>
public class ValidateUserExistsSpecification : IFlowSpecification<GetUserRequest>
{
    public async ValueTask<FlowInterrupt<object?>?> CheckAsync(
        GetUserRequest request,
        FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();
        var user = await userRepo.GetByIdAsync(request.Id, context.CancellationToken);

        if (user == null)
        {
            return FlowInterrupt<object?>.Fail(
                $"User with ID '{request.Id}' not found",
                StatusCodes.Status404NotFound
            );
        }

        // Store user in context for handler to use (avoids duplicate DB query)
        context.Set(user, key: "validated-user");

        return null;
    }
}
