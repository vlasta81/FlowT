namespace FlowT.Tests.Helpers;

/// <summary>
/// Shared test models for flow tests
/// </summary>

public record SimpleRequest
{
    public string Value { get; init; } = string.Empty;
}

public record SimpleResponse
{
    public string Message { get; init; } = string.Empty;
}
