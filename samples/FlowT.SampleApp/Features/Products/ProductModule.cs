using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.Extensions;
using FlowT.SampleApp.Domain;
using FlowT.SampleApp.Infrastructure;
using FlowT.SampleApp.Policies;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace FlowT.SampleApp.Features.Products;

// ===== CONTRACTS =====

public record ListProductsRequest();
public record ListProductsResponse(List<ProductDto> Products);

public record GetProductRequest(Guid Id);
public record GetProductResponse(Guid Id, string Name, string Description, decimal Price, int StockQuantity);

public record CreateProductRequest(string Name, string Description, decimal Price, int StockQuantity);
public record CreateProductResponse(Guid Id, string Name, decimal Price);

public record ProductDto(Guid Id, string Name, string Description, decimal Price, int StockQuantity);

// ===== SPECIFICATIONS =====

public class ValidateProductSpecification : IFlowSpecification<CreateProductRequest>
{
    public ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateProductRequest request,
        FlowContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail("Product name is required", StatusCodes.Status400BadRequest)
            );
        }

        if (request.Price <= 0)
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail("Price must be greater than 0", StatusCodes.Status400BadRequest)
            );
        }

        if (request.StockQuantity < 0)
        {
            return ValueTask.FromResult<FlowInterrupt<object?>?>(
                FlowInterrupt<object?>.Fail("Stock quantity cannot be negative", StatusCodes.Status400BadRequest)
            );
        }

        return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
    }

}

// ===== HANDLERS =====

public class ListProductsHandler : IFlowHandler<ListProductsRequest, ListProductsResponse>
{
    public async ValueTask<ListProductsResponse> HandleAsync(ListProductsRequest request, FlowContext context)
    {
        var productRepo = context.Service<IProductRepository>();
        var products = await productRepo.GetAllAsync(context.CancellationToken);

        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity
        )).ToList();

        return new ListProductsResponse(productDtos);
    }
}

public class GetProductHandler : IFlowHandler<GetProductRequest, GetProductResponse>
{
    public async ValueTask<GetProductResponse> HandleAsync(GetProductRequest request, FlowContext context)
    {
        var productRepo = context.Service<IProductRepository>();
        var product = await productRepo.GetByIdAsync(request.Id, context.CancellationToken);

        if (product == null)
        {
            throw new InvalidOperationException($"Product {request.Id} not found");
        }

        return new GetProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity
        );
    }
}

public class CreateProductHandler : IFlowHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(ILogger<CreateProductHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CreateProductResponse> HandleAsync(CreateProductRequest request, FlowContext context)
    {
        var productRepo = context.Service<IProductRepository>();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await productRepo.CreateAsync(product, context.CancellationToken);

        _logger.LogInformation("Product created: {ProductId} ({Name})", product.Id, product.Name);

        return new CreateProductResponse(product.Id, product.Name, product.Price);
    }
}

// ===== FLOWS =====

[FlowDefinition]
public class ListProductsFlow : FlowDefinition<ListProductsRequest, ListProductsResponse>
{
    protected override void Configure(IFlowBuilder<ListProductsRequest, ListProductsResponse> flow)
    {
        flow
            .Use<LoggingPolicy<ListProductsRequest, ListProductsResponse>>()
            .Handle<ListProductsHandler>();
    }
}

[FlowDefinition]
public class GetProductFlow : FlowDefinition<GetProductRequest, GetProductResponse>
{
    protected override void Configure(IFlowBuilder<GetProductRequest, GetProductResponse> flow)
    {
        flow
            .Use<LoggingPolicy<GetProductRequest, GetProductResponse>>()
            .Handle<GetProductHandler>();
    }
}

[FlowDefinition]
public class CreateProductFlow : FlowDefinition<CreateProductRequest, CreateProductResponse>
{
    protected override void Configure(IFlowBuilder<CreateProductRequest, CreateProductResponse> flow)
    {
        flow
            .Check<ValidateProductSpecification>()
            .Use<LoggingPolicy<CreateProductRequest, CreateProductResponse>>()
            .Handle<CreateProductHandler>();
    }
}

// ===== STREAMING =====

public record StreamProductsRequest(int Page = 0, int PageSize = 10);

/// <summary>
/// Returns products as a paginated streaming response.
/// Demonstrates <see cref="PagedStreamResponse{T}"/> — metadata is sent first,
/// then items are streamed progressively via IAsyncEnumerable.
/// </summary>
public class StreamProductsHandler : IFlowHandler<StreamProductsRequest, PagedStreamResponse<ProductDto>>
{
    public async ValueTask<PagedStreamResponse<ProductDto>> HandleAsync(
        StreamProductsRequest request, FlowContext context)
    {
        var productRepo = context.Service<IProductRepository>();
        var all = await productRepo.GetAllAsync(context.CancellationToken);

        var page = all
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity))
            .ToList();

        return new PagedStreamResponse<ProductDto>
        {
            TotalCount = all.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            Items = AsAsyncEnumerable(page, context.CancellationToken)
        };
    }

    // Converts a buffered list to IAsyncEnumerable to simulate progressive streaming
    private static async IAsyncEnumerable<ProductDto> AsAsyncEnumerable(
        IEnumerable<ProductDto> items,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }
}

[FlowDefinition]
public class StreamProductsFlow : FlowDefinition<StreamProductsRequest, PagedStreamResponse<ProductDto>>
{
    protected override void Configure(IFlowBuilder<StreamProductsRequest, PagedStreamResponse<ProductDto>> flow)
    {
        flow
            .Use<LoggingPolicy<StreamProductsRequest, PagedStreamResponse<ProductDto>>>()
            .Handle<StreamProductsHandler>();
    }
}

// ===== MODULE =====

[FlowModule]
public class ProductModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlow<ListProductsFlow, ListProductsRequest, ListProductsResponse>();
        services.AddFlow<GetProductFlow, GetProductRequest, GetProductResponse>();
        services.AddFlow<CreateProductFlow, CreateProductRequest, CreateProductResponse>();
        services.AddFlow<StreamProductsFlow, StreamProductsRequest, PagedStreamResponse<ProductDto>>();

        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/products")
            .WithTags("Products");

        products.MapGet("/", async (
            ListProductsFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(new ListProductsRequest(), httpContext);
            return Results.Ok(result);
        })
        .WithName("ListProducts")
        .WithSummary("Get all products")
        .Produces<ListProductsResponse>();

        products.MapGet("/{id:guid}", async (
            Guid id,
            GetProductFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(new GetProductRequest(id), httpContext);
            return Results.Ok(result);
        })
        .WithName("GetProduct")
        .WithSummary("Get product by ID")
        .Produces<GetProductResponse>()
        .Produces(StatusCodes.Status404NotFound);

        products.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            CreateProductFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(request, httpContext);
            return Results.Created($"/api/products/{result.Id}", result);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/products/stream?page=0&pageSize=10
        // ✅ MapFlow auto-detects PagedStreamResponse<T> : IStreamableResponse and calls Results.Stream() internally
        products.MapFlow<StreamProductsFlow, StreamProductsRequest, PagedStreamResponse<ProductDto>>("/stream", "GET")
            .WithName("StreamProducts")
            .WithSummary("Stream paginated products with metadata (PagedStreamResponse<T> demo)");
    }
}
