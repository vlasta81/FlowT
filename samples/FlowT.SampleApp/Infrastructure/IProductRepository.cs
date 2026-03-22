using FlowT.SampleApp.Domain;

namespace FlowT.SampleApp.Infrastructure;

/// <summary>
/// In-memory product repository (for demo purposes)
/// </summary>
public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<bool> UpdateStockAsync(Guid productId, int newQuantity, CancellationToken ct = default);
}

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public InMemoryProductRepository()
    {
        // Seed initial data
        _products.AddRange(new[]
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                StockQuantity = 50,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Mouse",
                Description = "Wireless mouse",
                Price = 29.99m,
                StockQuantity = 200,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 79.99m,
                StockQuantity = 100,
                CreatedAt = DateTimeOffset.UtcNow
            }
        });
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _products.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _products.Add(product);
            return product;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int newQuantity, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return false;

            var updated = product with { StockQuantity = newQuantity };
            var index = _products.FindIndex(p => p.Id == productId);
            _products[index] = updated;
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
