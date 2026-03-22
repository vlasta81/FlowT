namespace FlowT.SampleApp.Features.Users;

// ===== REQUESTS & RESPONSES =====

public record CreateUserRequest(string Email, string Name, string? PhoneNumber = null);
public record CreateUserResponse(Guid Id, string Email, string Name, DateTimeOffset CreatedAt);

public record GetUserRequest(Guid Id);
public record GetUserResponse(Guid Id, string Email, string Name, string? PhoneNumber, bool IsActive, DateTimeOffset CreatedAt);

public record ListUsersRequest();
public record ListUsersResponse(List<UserDto> Users);

public record UpdateUserRequest(Guid Id, string Name, string? PhoneNumber);
public record UpdateUserResponse(Guid Id, string Name, string? PhoneNumber, DateTimeOffset UpdatedAt);

public record DeleteUserRequest(Guid Id);
public record DeleteUserResponse(bool Success, string Message);

public record ExportUsersRequest();

// ===== DTOs =====

public record UserDto(Guid Id, string Email, string Name, string? PhoneNumber, bool IsActive);
