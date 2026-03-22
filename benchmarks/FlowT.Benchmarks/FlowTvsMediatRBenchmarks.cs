using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FlowT.Benchmarks;

/// <summary>
/// Comparison benchmarks between FlowT and MediatR.
/// Tests equivalent scenarios: simple handler, with behaviors/policies, with validation.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowTvsMediatRBenchmarks
{
    private IServiceProvider _services = null!;

    // FlowT
    private SimpleFlowT _simpleFlowT = null!;
    private FlowTWithPolicy _flowTWithPolicy = null!;
    private FlowTWithValidation _flowTWithValidation = null!;
    private FlowContext _flowContext = null!;

    // MediatR
    private IMediator _mediator = null!;

    private ComparisonRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register FlowT
        services.AddSingleton<SimpleFlowT>();
        services.AddSingleton<FlowTWithPolicy>();
        services.AddSingleton<FlowTWithValidation>();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<FlowTvsMediatRBenchmarks>());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediatRLoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediatRValidationBehavior<,>));

        _services = services.BuildServiceProvider();

        // FlowT setup
        _simpleFlowT = _services.GetRequiredService<SimpleFlowT>();
        _flowTWithPolicy = _services.GetRequiredService<FlowTWithPolicy>();
        _flowTWithValidation = _services.GetRequiredService<FlowTWithValidation>();
        _flowContext = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        // MediatR setup
        _mediator = _services.GetRequiredService<IMediator>();

        _request = new ComparisonRequest { Value = "Test", IsValid = true };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    // ============================================
    // Simple Handler (No middleware/policies)
    // ============================================

    [Benchmark(Baseline = true, Description = "FlowT: Simple handler")]
    public async Task<ComparisonResponse> FlowT_SimpleHandler()
    {
        return await _simpleFlowT.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "MediatR: Simple handler")]
    public async Task<ComparisonResponse> MediatR_SimpleHandler()
    {
        return await _mediator.Send(new SimpleMediatRRequest { Value = _request.Value });
    }

    // ============================================
    // With Policy/Behavior
    // ============================================

    [Benchmark(Description = "FlowT: Handler + 1 policy")]
    public async Task<ComparisonResponse> FlowT_WithPolicy()
    {
        return await _flowTWithPolicy.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "MediatR: Handler + 1 behavior")]
    public async Task<ComparisonResponse> MediatR_WithBehavior()
    {
        return await _mediator.Send(new MediatRRequestWithBehavior { Value = _request.Value });
    }

    // ============================================
    // With Validation
    // ============================================

    [Benchmark(Description = "FlowT: Handler + validation")]
    public async Task<ComparisonResponse> FlowT_WithValidation()
    {
        return await _flowTWithValidation.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "MediatR: Handler + validation")]
    public async Task<ComparisonResponse> MediatR_WithValidation()
    {
        return await _mediator.Send(new MediatRRequestWithValidation { Value = _request.Value, IsValid = true });
    }

    // ============================================
    // Test Data
    // ============================================

    public record ComparisonRequest
    {
        public string Value { get; init; } = "";
        public bool IsValid { get; init; }
    }

    public record ComparisonResponse
    {
        public string Result { get; init; } = "";
    }

    // ============================================
    // FlowT Implementations
    // ============================================

    public class SimpleFlowT : FlowDefinition<ComparisonRequest, ComparisonResponse>
    {
        protected override void Configure(IFlowBuilder<ComparisonRequest, ComparisonResponse> flow)
        {
            flow.Handle<SimpleFlowTHandler>();
        }
    }

    public class SimpleFlowTHandler : IFlowHandler<ComparisonRequest, ComparisonResponse>
    {
        public ValueTask<ComparisonResponse> HandleAsync(ComparisonRequest request, FlowContext context)
        {
            return ValueTask.FromResult(new ComparisonResponse { Result = $"FlowT: {request.Value}" });
        }
    }

    public class FlowTWithPolicy : FlowDefinition<ComparisonRequest, ComparisonResponse>
    {
        protected override void Configure(IFlowBuilder<ComparisonRequest, ComparisonResponse> flow)
        {
            flow.Use<FlowTLoggingPolicy>()
                .Handle<SimpleFlowTHandler>();
        }
    }

    public class FlowTLoggingPolicy : FlowPolicy<ComparisonRequest, ComparisonResponse>
    {
        public override async ValueTask<ComparisonResponse> HandleAsync(ComparisonRequest request, FlowContext context)
        {
            // Simulate logging
            var result = await Next.HandleAsync(request, context);
            return result with { Result = $"[Logged]{result.Result}" };
        }
    }

    public class FlowTWithValidation : FlowDefinition<ComparisonRequest, ComparisonResponse>
    {
        protected override void Configure(IFlowBuilder<ComparisonRequest, ComparisonResponse> flow)
        {
            flow.Check<FlowTValidation>()
                .OnInterrupt(interrupt => new ComparisonResponse { Result = "Invalid" })
                .Handle<SimpleFlowTHandler>();
        }
    }

    public class FlowTValidation : IFlowSpecification<ComparisonRequest>
    {
        public ValueTask<FlowInterrupt<object?>?> CheckAsync(ComparisonRequest request, FlowContext context)
        {
            if (!request.IsValid)
            {
                return ValueTask.FromResult<FlowInterrupt<object?>?>(
                    FlowInterrupt<object?>.Fail("Validation failed"));
            }
            return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
        }
    }

    // ============================================
    // MediatR Implementations
    // ============================================

    public record SimpleMediatRRequest : IRequest<ComparisonResponse>
    {
        public string Value { get; init; } = "";
    }

    public class SimpleMediatRHandler : IRequestHandler<SimpleMediatRRequest, ComparisonResponse>
    {
        public Task<ComparisonResponse> Handle(SimpleMediatRRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ComparisonResponse { Result = $"MediatR: {request.Value}" });
        }
    }

    public record MediatRRequestWithBehavior : IRequest<ComparisonResponse>
    {
        public string Value { get; init; } = "";
    }

    public class MediatRHandlerWithBehavior : IRequestHandler<MediatRRequestWithBehavior, ComparisonResponse>
    {
        public Task<ComparisonResponse> Handle(MediatRRequestWithBehavior request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ComparisonResponse { Result = $"MediatR: {request.Value}" });
        }
    }

    public class MediatRLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Only apply to specific request type
            if (request is not MediatRRequestWithBehavior)
            {
                return await next();
            }

            var response = await next();

            // Modify response if it's ComparisonResponse
            if (response is ComparisonResponse compResp)
            {
                var modified = new ComparisonResponse { Result = $"[Logged]{compResp.Result}" };
                return (TResponse)(object)modified;
            }

            return response;
        }
    }

    public record MediatRRequestWithValidation : IRequest<ComparisonResponse>
    {
        public string Value { get; init; } = "";
        public bool IsValid { get; init; }
    }

    public class MediatRHandlerWithValidation : IRequestHandler<MediatRRequestWithValidation, ComparisonResponse>
    {
        public Task<ComparisonResponse> Handle(MediatRRequestWithValidation request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ComparisonResponse { Result = $"MediatR: {request.Value}" });
        }
    }

    public class MediatRValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Only apply to validation request
            if (request is MediatRRequestWithValidation validationReq && !validationReq.IsValid)
            {
                return (TResponse)(object)new ComparisonResponse { Result = "Invalid" };
            }

            return await next();
        }
    }
}
