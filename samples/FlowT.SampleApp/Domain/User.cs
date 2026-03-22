namespace FlowT.SampleApp.Domain;

/// <summary>
/// User entity
/// </summary>
public record User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public bool IsActive { get; init; } = true;
}
