using FlowT.Abstractions;

namespace FlowT.SampleApp.Policies;

/// <summary>
/// Caching policy - demonstrates GetOrAdd with named keys for request-scoped caching
/// </summary>
public class CachingPolicy<TRequest, TResponse> : FlowPolicy<TRequest, TResponse>
{
    private readonly ILogger<CachingPolicy<TRequest, TResponse>> _logger;

    public CachingPolicy(ILogger<CachingPolicy<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public override async ValueTask<TResponse> HandleAsync(TRequest request, FlowContext context)
    {
        // Create request-scoped cache using GetOrAdd with named key
        // This cache persists for the entire request pipeline
        var requestCache = context.GetOrAdd(
            () => new Dictionary<string, object>(),
            key: "cache:request-scope"
        );

        _logger.LogDebug("Cache initialized for {RequestType}, entries: {Count}",
            typeof(TRequest).Name,
            requestCache.Count
        );

        // Example: Store request metadata in cache
        requestCache[$"request:{typeof(TRequest).Name}:timestamp"] = DateTimeOffset.UtcNow;

        var response = await Next!.HandleAsync(request, context);

        // Cache stats
        _logger.LogDebug("Cache usage: {Count} entries", requestCache.Count);

        return response;
    }
}
