using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;

// DispatchR using directives
using DispatchR;
using DispatchR.Abstractions.Send;
using DispatchR.Extensions;

namespace FlowT.Benchmarks;

/// <summary>
/// Comparison benchmarks between FlowT and DispatchR.
/// Tests equivalent scenarios: simple handler, with behaviors/policies, with validation.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class FlowTvsDispatchRBenchmarks
{
    private IServiceProvider _services = null!;

    // FlowT
    private SimpleFlowT _simpleFlowT = null!;
    private FlowTWithPolicy _flowTWithPolicy = null!;
    private FlowTWithValidation _flowTWithValidation = null!;
    private FlowContext _flowContext = null!;

    // DispatchR
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

        // Register DispatchR
        services.AddDispatchR(options =>
        {
            options.Assemblies.Add(typeof(FlowTvsDispatchRBenchmarks).Assembly);
            options.RegisterPipelines = true;
            options.RegisterNotifications = false; // Not testing notifications
            options.PipelineOrder =
            [
                typeof(DispatchRLoggingBehavior<,>),
                typeof(DispatchRValidationBehavior<,>)
            ];
        });

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

        // DispatchR setup
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

    [Benchmark(Description = "DispatchR: Simple handler")]
    public async ValueTask<ComparisonResponse> DispatchR_SimpleHandler()
    {
        return await _mediator.Send(new SimpleDispatchRRequest { Value = _request.Value }, CancellationToken.None);
    }

    // ============================================
    // With Policy/Behavior
    // ============================================

    [Benchmark(Description = "FlowT: Handler + 1 policy")]
    public async Task<ComparisonResponse> FlowT_WithPolicy()
    {
        return await _flowTWithPolicy.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "DispatchR: Handler + 1 behavior")]
    public async ValueTask<ComparisonResponse> DispatchR_WithBehavior()
    {
        return await _mediator.Send(new DispatchRRequestWithBehavior { Value = _request.Value }, CancellationToken.None);
    }

    // ============================================
    // With Validation
    // ============================================

    [Benchmark(Description = "FlowT: Handler + validation")]
    public async Task<ComparisonResponse> FlowT_WithValidation()
    {
        return await _flowTWithValidation.ExecuteAsync(_request, _flowContext);
    }

    [Benchmark(Description = "DispatchR: Handler + validation")]
    public async ValueTask<ComparisonResponse> DispatchR_WithValidation()
    {
        return await _mediator.Send(new DispatchRRequestWithValidation { Value = _request.Value, IsValid = true }, CancellationToken.None);
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
    // DispatchR Implementations
    // ============================================

    // Simple request without pipeline
    public record SimpleDispatchRRequest : IRequest<SimpleDispatchRRequest, ValueTask<ComparisonResponse>>
    {
        public string Value { get; init; } = "";
    }

    public class SimpleDispatchRHandler : IRequestHandler<SimpleDispatchRRequest, ValueTask<ComparisonResponse>>
    {
        public ValueTask<ComparisonResponse> Handle(SimpleDispatchRRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new ComparisonResponse { Result = $"DispatchR: {request.Value}" });
        }
    }

    // Request with logging behavior
    public record DispatchRRequestWithBehavior : IRequest<DispatchRRequestWithBehavior, ValueTask<ComparisonResponse>>
    {
        public string Value { get; init; } = "";
        public bool ApplyLogging { get; init; } = true; // Flag to enable logging
    }

    public class DispatchRHandlerWithBehavior : IRequestHandler<DispatchRRequestWithBehavior, ValueTask<ComparisonResponse>>
    {
        public ValueTask<ComparisonResponse> Handle(DispatchRRequestWithBehavior request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new ComparisonResponse { Result = $"DispatchR: {request.Value}" });
        }
    }

    // Generic logging pipeline behavior for DispatchR
    public class DispatchRLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, ValueTask<TResponse>>
        where TRequest : class, IRequest<TRequest, ValueTask<TResponse>>
    {
        public required IRequestHandler<TRequest, ValueTask<TResponse>> NextPipeline { get; set; }

        public async ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            // Only apply to specific request type with logging flag
            if (request is not DispatchRRequestWithBehavior behaviorRequest || !behaviorRequest.ApplyLogging)
            {
                return await NextPipeline.Handle(request, cancellationToken);
            }

            var response = await NextPipeline.Handle(request, cancellationToken);

            // Modify response if it's ComparisonResponse
            if (response is ComparisonResponse compResp)
            {
                return (TResponse)(object)new ComparisonResponse { Result = $"[Logged]{compResp.Result}" };
            }

            return response;
        }
    }

    // Request with validation
    public record DispatchRRequestWithValidation : IRequest<DispatchRRequestWithValidation, ValueTask<ComparisonResponse>>
    {
        public string Value { get; init; } = "";
        public bool IsValid { get; init; }
    }

    public class DispatchRHandlerWithValidation : IRequestHandler<DispatchRRequestWithValidation, ValueTask<ComparisonResponse>>
    {
        public ValueTask<ComparisonResponse> Handle(DispatchRRequestWithValidation request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new ComparisonResponse { Result = $"DispatchR: {request.Value}" });
        }
    }

    // Generic validation pipeline behavior for DispatchR
    public class DispatchRValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, ValueTask<TResponse>>
        where TRequest : class, IRequest<TRequest, ValueTask<TResponse>>
    {
        public required IRequestHandler<TRequest, ValueTask<TResponse>> NextPipeline { get; set; }

        public ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            // Only apply to validation request
            if (request is DispatchRRequestWithValidation validationReq && !validationReq.IsValid)
            {
                return ValueTask.FromResult((TResponse)(object)new ComparisonResponse { Result = "Invalid" });
            }

            return NextPipeline.Handle(request, cancellationToken);
        }
    }
}
