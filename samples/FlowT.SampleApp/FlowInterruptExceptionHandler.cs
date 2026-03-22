using Microsoft.AspNetCore.Diagnostics;

namespace FlowT.SampleApp;

/// <summary>
/// Converts <see cref="FlowInterruptException"/> thrown from an <c>.OnInterrupt()</c> mapper
/// into an HTTP JSON error response with the correct status code.
/// </summary>
internal sealed class FlowInterruptExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FlowInterruptException ex)
            return false;

        httpContext.Response.StatusCode = ex.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new { error = ex.Message, statusCode = ex.StatusCode },
            cancellationToken);
        return true;
    }
}
