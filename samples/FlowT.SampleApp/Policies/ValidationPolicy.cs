using FlowT.Abstractions;

namespace FlowT.SampleApp.Policies;

/// <summary>
/// Validation policy - validates requests using FlowContext named keys
/// Demonstrates storing validation results with named keys
/// </summary>
public class ValidationPolicy<TRequest, TResponse> : FlowPolicy<TRequest, TResponse>
{
    private readonly ILogger<ValidationPolicy<TRequest, TResponse>> _logger;

    public ValidationPolicy(ILogger<ValidationPolicy<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        // Store validation timestamp using named key
        context.Set(DateTimeOffset.UtcNow, key: "validation:timestamp");

        // Store validation status
        context.Set(true, key: "validation:passed");

        _logger.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);

        var response = await Next!.HandleAsync(request, context);

        // Validation policy could also validate response
        if (context.TryGet<bool>(out var validationPassed, key: "validation:passed"))
        {
            _logger.LogDebug("Validation status: {Status}", validationPassed ? "PASSED" : "FAILED");
        }

        return response;
    }
}
