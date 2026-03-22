using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.SampleApp.Policies;
using FlowT.SampleApp;

namespace FlowT.SampleApp.Features.Users;

/// <summary>
/// Flow for creating a new user
/// Demonstrates full pipeline: Specifications → Policies → Handler
/// </summary>
[FlowDefinition]
public class CreateUserFlow : FlowDefinition<CreateUserRequest, CreateUserResponse>
{
    protected override void Configure(IFlowBuilder<CreateUserRequest, CreateUserResponse> flow)
    {
        flow
            // ✅ Specifications (validations) - run first, can interrupt with FlowInterrupt
            .Check<ValidateRequiredFieldsSpecification>()
            .Check<ValidateEmailSpecification>()

            // ✅ Policies (cross-cutting concerns) - run if all specs pass
            .Use<LoggingPolicy<CreateUserRequest, CreateUserResponse>>()
            .Use<ValidationPolicy<CreateUserRequest, CreateUserResponse>>()
            .Use<CachingPolicy<CreateUserRequest, CreateUserResponse>>()

            // ✅ OnInterrupt: map Fail() results to a typed response — required when specs can Fail()
            //    CreateUserResponse is a clean DTO with no error fields, so we throw and let
            //    FlowInterruptExceptionHandler convert it to the correct HTTP status code.
            .OnInterrupt(interrupt =>
                throw new FlowInterruptException(
                    interrupt.Message ?? "Request failed", interrupt.StatusCode))

            // ✅ Handler (business logic) - final step
            .Handle<CreateUserHandler>();
    }
}

/// <summary>
/// Flow for getting user by ID
/// </summary>
[FlowDefinition]
public class GetUserFlow : FlowDefinition<GetUserRequest, GetUserResponse>
{
    protected override void Configure(IFlowBuilder<GetUserRequest, GetUserResponse> flow)
    {
        flow
            .Check<ValidateUserExistsSpecification>()
            .Use<LoggingPolicy<GetUserRequest, GetUserResponse>>()
            .Use<CachingPolicy<GetUserRequest, GetUserResponse>>()
            .OnInterrupt(interrupt =>
                throw new FlowInterruptException(
                    interrupt.Message ?? "Request failed", interrupt.StatusCode))
            .Handle<GetUserHandler>();
    }
}

/// <summary>
/// Flow for listing all users
/// </summary>
[FlowDefinition]
public class ListUsersFlow : FlowDefinition<ListUsersRequest, ListUsersResponse>
{
    protected override void Configure(IFlowBuilder<ListUsersRequest, ListUsersResponse> flow)
    {
        flow
            .Use<LoggingPolicy<ListUsersRequest, ListUsersResponse>>()
            .Use<CachingPolicy<ListUsersRequest, ListUsersResponse>>()
            .Handle<ListUsersHandler>();
    }
}

/// <summary>
/// Flow for updating user
/// </summary>
[FlowDefinition]
public class UpdateUserFlow : FlowDefinition<UpdateUserRequest, UpdateUserResponse>
{
    protected override void Configure(IFlowBuilder<UpdateUserRequest, UpdateUserResponse> flow)
    {
        flow
            .Use<LoggingPolicy<UpdateUserRequest, UpdateUserResponse>>()
            .Handle<UpdateUserHandler>();
    }
}

/// <summary>
/// Flow for deleting user
/// </summary>
[FlowDefinition]
public class DeleteUserFlow : FlowDefinition<DeleteUserRequest, DeleteUserResponse>
{
    protected override void Configure(IFlowBuilder<DeleteUserRequest, DeleteUserResponse> flow)
    {
        flow
            .Use<LoggingPolicy<DeleteUserRequest, DeleteUserResponse>>()
            .Handle<DeleteUserHandler>();
    }
}

/// <summary>
/// Flow for exporting all users as a downloadable file.
/// Demonstrates <see cref="FileStreamResponse"/> — the handler returns a stream directly;
/// use <c>Results.File</c> in the endpoint to deliver it.
/// </summary>
[FlowDefinition]
public class ExportUsersFlow : FlowDefinition<ExportUsersRequest, FileStreamResponse>
{
    protected override void Configure(IFlowBuilder<ExportUsersRequest, FileStreamResponse> flow)
    {
        flow
            .Use<LoggingPolicy<ExportUsersRequest, FileStreamResponse>>()
            .Handle<ExportUsersHandler>();
    }
}
