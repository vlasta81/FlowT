namespace FlowT.SampleApp.Domain;

/// <summary>
/// Order entity
/// </summary>
public record Order
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public List<OrderItem> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record OrderItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
