using System.Diagnostics;
using FlowT.Abstractions;

namespace FlowT.SampleApp.Policies;

/// <summary>
/// Logging policy - logs request/response for all flows
/// </summary>
public class LoggingPolicy<TRequest, TResponse> : FlowPolicy<TRequest, TResponse>
{
    private readonly ILogger<LoggingPolicy<TRequest, TResponse>> _logger;

    public LoggingPolicy(ILogger<LoggingPolicy<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        var flowId = context.GetFlowIdString();
        var requestType = typeof(TRequest).Name;

        _logger.LogInformation("[{FlowId}] Starting {RequestType}", flowId, requestType);

        try
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var response = await Next!.HandleAsync(request, context);
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

            _logger.LogInformation(
                "[{FlowId}] Completed {RequestType} in {ElapsedMs}ms",
                flowId,
                requestType,
                elapsed.TotalMilliseconds
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{FlowId}] Failed {RequestType}: {Error}", flowId, requestType, ex.Message);
            throw;
        }
    }
}
