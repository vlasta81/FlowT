namespace FlowT.SampleApp;

/// <summary>
/// Thrown from <c>.OnInterrupt()</c> when the flow response type is a clean DTO with no error fields.
/// Handled by <see cref="FlowInterruptExceptionHandler"/>, which maps it to an appropriate HTTP response.
/// </summary>
public sealed class FlowInterruptException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
