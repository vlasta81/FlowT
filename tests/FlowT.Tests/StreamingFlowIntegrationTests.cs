using FlowT.Abstractions;
using FlowT.Contracts;
using FlowT.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FlowT.Tests;

/// <summary>
/// Integration tests for streaming responses with flows
/// </summary>
public class StreamingFlowIntegrationTests : FlowTestBase
{
    #region Test Models

    public record GetItemsRequest(int Page, int PageSize);

    public record ItemDto(int Id, string Name);

    #endregion

    #region Test Handlers & Flows

    public class GetItemsStreamingFlow : FlowDefinition<GetItemsRequest, PagedStreamResponse<ItemDto>>
    {
        protected override void Configure(IFlowBuilder<GetItemsRequest, PagedStreamResponse<ItemDto>> flow)
        {
            flow.Handle<GetItemsHandler>();
        }
    }

    public class GetItemsBufferedFlow : FlowDefinition<GetItemsRequest, List<ItemDto>>
    {
        protected override void Configure(IFlowBuilder<GetItemsRequest, List<ItemDto>> flow)
        {
            flow.Handle<GetItemsBufferedHandler>();
        }
    }

    public class GetItemsHandler : IFlowHandler<GetItemsRequest, PagedStreamResponse<ItemDto>>
    {
        private readonly IItemRepository _repository;

        public GetItemsHandler(IItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<PagedStreamResponse<ItemDto>> HandleAsync(
            GetItemsRequest request,
            FlowContext context)
        {
            var totalCount = await _repository.CountAsync(context.CancellationToken);
            var items = _repository.StreamItemsAsync(request.Page, request.PageSize, context.CancellationToken);

            return new PagedStreamResponse<ItemDto>
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            };
        }
    }

    public class GetItemsBufferedHandler : IFlowHandler<GetItemsRequest, List<ItemDto>>
    {
        private readonly IItemRepository _repository;

        public GetItemsBufferedHandler(IItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<List<ItemDto>> HandleAsync(
            GetItemsRequest request,
            FlowContext context)
        {
            var items = await _repository.GetItemsAsync(request.Page, request.PageSize, context.CancellationToken);
            return items;
        }
    }

    #endregion

    #region Test Infrastructure

    public interface IItemRepository
    {
        Task<int> CountAsync(CancellationToken ct);
        IAsyncEnumerable<ItemDto> StreamItemsAsync(int page, int pageSize, CancellationToken ct);
        Task<List<ItemDto>> GetItemsAsync(int page, int pageSize, CancellationToken ct);
    }

    public class InMemoryItemRepository : IItemRepository
    {
        private readonly List<ItemDto> _items;

        public InMemoryItemRepository(int itemCount = 1000)
        {
            _items = Enumerable.Range(1, itemCount)
                .Select(i => new ItemDto(i, $"Item {i}"))
                .ToList();
        }

        public Task<int> CountAsync(CancellationToken ct) => Task.FromResult(_items.Count);

        public async IAsyncEnumerable<ItemDto> StreamItemsAsync(int page, int pageSize, CancellationToken ct)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize);

            foreach (var item in items)
            {
                await Task.Yield(); // Simulate async operation
                yield return item;
            }
        }

        public Task<List<ItemDto>> GetItemsAsync(int page, int pageSize, CancellationToken ct)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize).ToList();
            return Task.FromResult(items);
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task StreamingFlow_ExecutesAndReturnsPagedResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IItemRepository>(new InMemoryItemRepository(100));
        services.AddSingleton<GetItemsHandler>();
        services.AddSingleton<GetItemsStreamingFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<GetItemsStreamingFlow>();

        var request = new GetItemsRequest(0, 10);

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(100, response.TotalCount);
        Assert.Equal(0, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.True(response.HasMore);

        var items = new List<ItemDto>();
        await foreach (var item in response.Items)
        {
            items.Add(item);
        }

        Assert.Equal(10, items.Count);
        Assert.Equal(1, items[0].Id);
        Assert.Equal("Item 1", items[0].Name);
    }

    [Fact]
    public async Task StreamingFlow_HandlesEmptyResults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IItemRepository>(new InMemoryItemRepository(0));
        services.AddSingleton<GetItemsHandler>();
        services.AddSingleton<GetItemsStreamingFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<GetItemsStreamingFlow>();

        var request = new GetItemsRequest(0, 10);

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(0, response.TotalCount);
        Assert.False(response.HasMore);

        var items = new List<ItemDto>();
        await foreach (var item in response.Items)
        {
            items.Add(item);
        }

        Assert.Empty(items);
    }

    [Fact]
    public async Task StreamingFlow_HandlesLargeDataset()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IItemRepository>(new InMemoryItemRepository(10000));
        services.AddSingleton<GetItemsHandler>();
        services.AddSingleton<GetItemsStreamingFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<GetItemsStreamingFlow>();

        var request = new GetItemsRequest(50, 100);

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10000, response.TotalCount);
        Assert.Equal(50, response.Page);
        Assert.True(response.HasMore);

        var items = new List<ItemDto>();
        await foreach (var item in response.Items)
        {
            items.Add(item);
        }

        Assert.Equal(100, items.Count);
        Assert.Equal(5001, items[0].Id); // (50 * 100) + 1
    }

    [Fact]
    public async Task BufferedFlow_StillWorksAsExpected()
    {
        // Arrange - Verify non-streaming flows are not affected
        var services = new ServiceCollection();
        services.AddSingleton<IItemRepository>(new InMemoryItemRepository(100));
        services.AddSingleton<GetItemsBufferedHandler>();
        services.AddSingleton<GetItemsBufferedFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<GetItemsBufferedFlow>();

        var request = new GetItemsRequest(0, 10);

        // Act
        var response = await flow.ExecuteAsync(request, context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Count);
        Assert.Equal(1, response[0].Id);
        Assert.Equal("Item 1", response[0].Name);
    }

    [Fact]
    public async Task StreamingFlow_SupportsMultiplePagesSequentially()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IItemRepository>(new InMemoryItemRepository(250));
        services.AddSingleton<GetItemsHandler>();
        services.AddSingleton<GetItemsStreamingFlow>();

        var provider = services.BuildServiceProvider();
        var context = CreateContext(provider);
        var flow = provider.GetRequiredService<GetItemsStreamingFlow>();

        // Act - Fetch multiple pages
        var page0Response = await flow.ExecuteAsync(new GetItemsRequest(0, 100), context);
        var page0Items = await page0Response.Items.ToListAsync();

        var page1Response = await flow.ExecuteAsync(new GetItemsRequest(1, 100), context);
        var page1Items = await page1Response.Items.ToListAsync();

        var page2Response = await flow.ExecuteAsync(new GetItemsRequest(2, 100), context);
        var page2Items = await page2Response.Items.ToListAsync();

        // Assert
        Assert.Equal(100, page0Items.Count);
        Assert.Equal(1, page0Items[0].Id);
        Assert.True(page0Response.HasMore);

        Assert.Equal(100, page1Items.Count);
        Assert.Equal(101, page1Items[0].Id);
        Assert.True(page1Response.HasMore);

        Assert.Equal(50, page2Items.Count);
        Assert.Equal(201, page2Items[0].Id);
        Assert.False(page2Response.HasMore);
    }

    #endregion
}
