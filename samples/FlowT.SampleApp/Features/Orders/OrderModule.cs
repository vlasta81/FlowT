using FlowT.Abstractions;
using FlowT.Attributes;
using FlowT.Contracts;
using FlowT.Extensions;
using FlowT.Plugins;
using FlowT.SampleApp;
using FlowT.SampleApp.Domain;
using FlowT.SampleApp.Infrastructure;
using FlowT.SampleApp.Policies;
using Microsoft.AspNetCore.Mvc;

namespace FlowT.SampleApp.Features.Orders;

// ===== CONTRACTS =====

public record CreateOrderRequest(Guid UserId, List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Quantity);
public record CreateOrderResponse(Guid OrderId, decimal TotalAmount, OrderStatus Status, string Message);

// ===== SPECIFICATIONS =====

/// <summary>
/// Validates that user exists before creating order
/// Demonstrates early exit with FlowInterrupt
/// </summary>
public class ValidateUserForOrderSpecification : IFlowSpecification<CreateOrderRequest>
{
    public async ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateOrderRequest request,
        FlowContext context)
    {
        var userRepo = context.Service<IUserRepository>();
        var user = await userRepo.GetByIdAsync(request.UserId, context.CancellationToken);

        if (user == null)
        {
            return FlowInterrupt<object?>.Fail(
                $"User {request.UserId} not found",
                StatusCodes.Status404NotFound
            );
        }

        if (!user.IsActive)
        {
            return FlowInterrupt<object?>.Fail(
                "Cannot create order for inactive user",
                StatusCodes.Status403Forbidden
            );
        }

        // Store user for handler
        context.Set(user, key: "order:user");

        return null;
    }
}

/// <summary>
/// Validates order items and checks product availability
/// Demonstrates complex validation with multiple checks
/// </summary>
public class ValidateOrderItemsSpecification : IFlowSpecification<CreateOrderRequest>
{
    private readonly ILogger<ValidateOrderItemsSpecification> _logger;

    public ValidateOrderItemsSpecification(ILogger<ValidateOrderItemsSpecification> logger)
    {
        _logger = logger;
    }

    public async ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateOrderRequest request,
        FlowContext context)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return FlowInterrupt<object?>.Fail(
                "Order must contain at least one item",
                StatusCodes.Status400BadRequest
            );
        }

        var productRepo = context.Service<IProductRepository>();
        var validatedItems = new List<(Product Product, int Quantity)>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return FlowInterrupt<object?>.Fail(
                    $"Invalid quantity for product {item.ProductId}",
                    StatusCodes.Status400BadRequest
                );
            }

            var product = await productRepo.GetByIdAsync(item.ProductId, context.CancellationToken);
            if (product == null)
            {
                return FlowInterrupt<object?>.Fail(
                    $"Product {item.ProductId} not found",
                    StatusCodes.Status404NotFound
                );
            }

            if (product.StockQuantity < item.Quantity)
            {
                return FlowInterrupt<object?>.Fail(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}",
                    StatusCodes.Status409Conflict
                );
            }

            validatedItems.Add((product, item.Quantity));
        }

        // Store validated items in context with named key
        context.Set(validatedItems, key: "order:validated-items");

        _logger.LogDebug("Order validation passed: {ItemCount} items", validatedItems.Count);

        return null;
    }
}

/// <summary>
/// Business rule: Orders over $1000 require approval
/// Demonstrates business logic in specifications
/// </summary>
public class CheckOrderValueSpecification : IFlowSpecification<CreateOrderRequest>
{
    private const decimal ApprovalThreshold = 1000m;

    public ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateOrderRequest request,
        FlowContext context)
    {
        // Get validated items from context
        if (!context.TryGet<List<(Product Product, int Quantity)>>(out var validatedItems, key: "order:validated-items"))
        {
            throw new InvalidOperationException("Validated items not found in context");
        }

        // Calculate total
        var totalAmount = validatedItems.Sum(item => item.Product.Price * item.Quantity);

        // Store total in context
        context.Set(totalAmount, key: "order:total-amount");

        if (totalAmount > ApprovalThreshold)
        {
            // Store flag for handler
            context.Set(true, key: "order:requires-approval");

            // Could return FlowInterrupt to stop order, but we'll let handler decide
            // return FlowInterrupt<object?>.Fail(
            //     $"Orders over ${ApprovalThreshold} require manual approval",
            //     StatusCodes.Status402PaymentRequired
            // );
        }

        return ValueTask.FromResult<FlowInterrupt<object?>?>(null);
    }
}

// ===== HANDLER =====

/// <summary>
/// Guards access to the new order workflow behind a feature flag.
/// When <c>NewOrderWorkflow</c> is disabled in <c>appsettings.json</c>, the order is rejected
/// with a 503 so the caller knows to fall back to the legacy path.
/// Demonstrates <see cref="IFeatureFlagPlugin"/>: async evaluation with per-flow caching.
/// </summary>
public class FeatureFlagOrderSpecification : IFlowSpecification<CreateOrderRequest>
{
    public async ValueTask<FlowInterrupt<object?>?> CheckAsync(
        CreateOrderRequest request,
        FlowContext context)
    {
        var ff = context.Plugin<IFeatureFlagPlugin>();

        // Result is cached for this flow — safe to call multiple times
        var enabled = await ff.IsEnabledAsync("NewOrderWorkflow", context.CancellationToken);

        if (!enabled)
        {
            return FlowInterrupt<object?>.Fail(
                "Order creation via the new workflow is currently disabled.",
                StatusCodes.Status503ServiceUnavailable
            );
        }

        return null;
    }
}

// ===== HANDLER =====

public class CreateOrderHandler : IFlowHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(ILogger<CreateOrderHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CreateOrderResponse> HandleAsync(
        CreateOrderRequest request,
        FlowContext context)
    {
        // Get validated data from context
        var user = context.TryGet<User>(out var u, key: "order:user") ? u : throw new InvalidOperationException("User not found");
        var validatedItems = context.TryGet<List<(Product Product, int Quantity)>>(out var items, key: "order:validated-items")
            ? items
            : throw new InvalidOperationException("Validated items not found");
        var totalAmount = context.TryGet<decimal>(out var total, key: "order:total-amount") ? total : 0m;
        var requiresApproval = context.TryGet<bool>(out var approval, key: "order:requires-approval") && approval;

        // IdempotencyPlugin: log when the caller supplies a deduplication key.
        // In production you would check a cache/store here to return the previous response.
        var idempotency = context.Plugin<IIdempotencyPlugin>();
        if (idempotency.HasKey)
            _logger.LogInformation("Idempotency key present: {Key} — check cache before persisting", idempotency.Key);

        // TenantPlugin: resolve tenant from claim ‘tid’ → X-Tenant-Id header → route → “default”
        var tenant = context.Plugin<ITenantPlugin>();
        _logger.LogInformation("Processing order for tenant: {TenantId}", tenant.TenantId);

        // AuditPlugin: accumulate structured entries for the lifetime of this flow
        var audit = context.Plugin<IAuditPlugin>();

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Items = validatedItems.Select(item => new OrderItem
            {
                ProductId = item.Product.Id,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            }).ToList(),
            TotalAmount = totalAmount,
            Status = requiresApproval ? OrderStatus.Pending : OrderStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Update product stock
        var productRepo = context.Service<IProductRepository>();
        foreach (var (product, quantity) in validatedItems)
        {
            await productRepo.UpdateStockAsync(
                product.Id,
                product.StockQuantity - quantity,
                context.CancellationToken
            );
        }

        var message = requiresApproval
            ? $"Order created with status Pending (requires approval for ${totalAmount:F2})"
            : $"Order created successfully";

        // Record audit entries
        audit.Record(requiresApproval ? "OrderRequiresApproval" : "OrderCreated",
            new { OrderId = order.Id, UserId = user.Id, TotalAmount = totalAmount, order.Status });

        _logger.LogInformation(
            "Order {OrderId} created for user {UserId}: {ItemCount} items, total ${TotalAmount:F2}, status: {Status}",
            order.Id,
            user.Id,
            order.Items.Count,
            totalAmount,
            order.Status
        );

        foreach (var entry in audit.Entries)
            _logger.LogDebug("[Audit] {Action} at {Timestamp}", entry.Action, entry.Timestamp);

        // FlowScopePlugin: demonstrates creating an explicit DI scope within a flow.
        // In HTTP flows ASP.NET Core already provides a request scope, so the value here
        // is educational — showing the pattern used in background jobs / hosted services
        // where no ambient scope exists. The scope is disposed after use.
        var scopePlugin = context.Plugin<IFlowScopePlugin>();
        try
        {
            var scopedLogger = scopePlugin.ScopedServices
                .GetRequiredService<ILogger<CreateOrderHandler>>();
            scopedLogger.LogDebug(
                "FlowScopePlugin: scoped logger resolved for order {OrderId}", order.Id);
        }
        finally
        {
            scopePlugin.Dispose();
        }

        // Publish background event (fire-and-forget)
        _ = context.PublishInBackground(new OrderCreatedEvent(order.Id, user.Id, totalAmount), context.CancellationToken);

        return new CreateOrderResponse(order.Id, totalAmount, order.Status, message);
    }
}

// ===== EVENTS =====

public record OrderCreatedEvent(Guid OrderId, Guid UserId, decimal TotalAmount);

/// <summary>
/// Background event handler for <see cref="OrderCreatedEvent"/>.
/// Demonstrates <see cref="IEventHandler{TEvent}"/>: handles domain events published
/// via <see cref="FlowContext.PublishInBackground{TEvent}"/>.
/// </summary>
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Background: processing OrderCreated event for order {OrderId}, user {UserId}, total ${TotalAmount:F2}",
            eventData.OrderId,
            eventData.UserId,
            eventData.TotalAmount);

        // Simulate async work (e.g. send notification, update read model)
        await Task.Delay(1, cancellationToken);
    }
}

// ===== FLOW =====

[FlowDefinition]
public class CreateOrderFlow : FlowDefinition<CreateOrderRequest, CreateOrderResponse>
{
    protected override void Configure(IFlowBuilder<CreateOrderRequest, CreateOrderResponse> flow)
    {
        flow
            // Specifications run in order - any can interrupt with FlowInterrupt
            .Check<ValidateUserForOrderSpecification>()
            .Check<ValidateOrderItemsSpecification>()
            .Check<CheckOrderValueSpecification>()
            // FeatureFlagPlugin: gate the new order workflow behind a feature flag
            .Check<FeatureFlagOrderSpecification>()

            // Policies (cross-cutting concerns)
            .Use<LoggingPolicy<CreateOrderRequest, CreateOrderResponse>>()
            .Use<ValidationPolicy<CreateOrderRequest, CreateOrderResponse>>()

            // OnInterrupt: required — without it Fail() returns null and result.OrderId throws
            .OnInterrupt(interrupt =>
                throw new FlowInterruptException(
                    interrupt.Message ?? "Request failed", interrupt.StatusCode))

            // Handler (business logic)
            .Handle<CreateOrderHandler>();
    }
}

// ===== MODULE =====

[FlowModule]
public class OrderModule : IFlowModule
{
    public void Register(IServiceCollection services)
    {
        services.AddFlow<CreateOrderFlow, CreateOrderRequest, CreateOrderResponse>();
        services.AddSingleton<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders")
            .WithTags("Orders");

        orders.MapPost("/", async (
            [FromBody] CreateOrderRequest request,
            CreateOrderFlow flow,
            HttpContext httpContext) =>
        {
            var result = await flow.ExecuteAsync(request, httpContext);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order with validation")
        .WithDescription("Demonstrates complex validation pipeline with FlowInterrupt, IdempotencyPlugin, TenantPlugin, AuditPlugin and FeatureFlagPlugin")
        .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
