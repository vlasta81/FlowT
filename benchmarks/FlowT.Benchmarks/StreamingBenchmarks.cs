using BenchmarkDotNet.Attributes;
using FlowT;
using FlowT.Abstractions;
using FlowT.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlowT.Benchmarks;

/// <summary>
/// Benchmarks comparing buffered (List) vs streaming (PagedStreamResponse) responses.
/// Tests memory efficiency and throughput for different dataset sizes.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class StreamingBenchmarks
{
    private IServiceProvider _services = null!;
    private BufferedSmallFlow _bufferedSmallFlow = null!;
    private StreamingSmallFlow _streamingSmallFlow = null!;
    private BufferedMediumFlow _bufferedMediumFlow = null!;
    private StreamingMediumFlow _streamingMediumFlow = null!;
    private BufferedLargeFlow _bufferedLargeFlow = null!;
    private StreamingLargeFlow _streamingLargeFlow = null!;
    private FlowContext _context = null!;
    private ItemRequest _requestSmall = null!;
    private ItemRequest _requestMedium = null!;
    private ItemRequest _requestLarge = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register repository
        services.AddSingleton<IItemRepository, InMemoryItemRepository>();

        // Register flows
        services.AddSingleton<BufferedSmallFlow>();
        services.AddSingleton<StreamingSmallFlow>();
        services.AddSingleton<BufferedMediumFlow>();
        services.AddSingleton<StreamingMediumFlow>();
        services.AddSingleton<BufferedLargeFlow>();
        services.AddSingleton<StreamingLargeFlow>();

        _services = services.BuildServiceProvider();
        _bufferedSmallFlow = _services.GetRequiredService<BufferedSmallFlow>();
        _streamingSmallFlow = _services.GetRequiredService<StreamingSmallFlow>();
        _bufferedMediumFlow = _services.GetRequiredService<BufferedMediumFlow>();
        _streamingMediumFlow = _services.GetRequiredService<StreamingMediumFlow>();
        _bufferedLargeFlow = _services.GetRequiredService<BufferedLargeFlow>();
        _streamingLargeFlow = _services.GetRequiredService<StreamingLargeFlow>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _requestSmall = new ItemRequest { Page = 0, PageSize = 100 };
        _requestMedium = new ItemRequest { Page = 0, PageSize = 1000 };
        _requestLarge = new ItemRequest { Page = 0, PageSize = 10000 };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    #region Small Dataset (100 items)

    [Benchmark(Baseline = true, Description = "Buffered 100 items")]
    public async Task<int> BufferedSmall_Execute()
    {
        var response = await _bufferedSmallFlow.ExecuteAsync(_requestSmall, _context);
        return response.Count;
    }

    [Benchmark(Description = "Streaming 100 items")]
    public async Task<int> StreamingSmall_Execute()
    {
        var response = await _streamingSmallFlow.ExecuteAsync(_requestSmall, _context);
        var count = 0;
        await foreach (var _ in response.Items)
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Medium Dataset (1,000 items)

    [Benchmark(Description = "Buffered 1,000 items")]
    public async Task<int> BufferedMedium_Execute()
    {
        var response = await _bufferedMediumFlow.ExecuteAsync(_requestMedium, _context);
        return response.Count;
    }

    [Benchmark(Description = "Streaming 1,000 items")]
    public async Task<int> StreamingMedium_Execute()
    {
        var response = await _streamingMediumFlow.ExecuteAsync(_requestMedium, _context);
        var count = 0;
        await foreach (var _ in response.Items)
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Large Dataset (10,000 items)

    [Benchmark(Description = "Buffered 10,000 items")]
    public async Task<int> BufferedLarge_Execute()
    {
        var response = await _bufferedLargeFlow.ExecuteAsync(_requestLarge, _context);
        return response.Count;
    }

    [Benchmark(Description = "Streaming 10,000 items")]
    public async Task<int> StreamingLarge_Execute()
    {
        var response = await _streamingLargeFlow.ExecuteAsync(_requestLarge, _context);
        var count = 0;
        await foreach (var _ in response.Items)
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Models

    public record ItemRequest
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
    }

    public record Item(int Id, string Name, string Description);

    #endregion

    #region Repository

    public interface IItemRepository
    {
        Task<int> CountAsync(CancellationToken ct);
        IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize, CancellationToken ct);
        Task<List<Item>> GetItemsAsync(int page, int pageSize, CancellationToken ct);
    }

    public class InMemoryItemRepository : IItemRepository
    {
        private readonly List<Item> _items;

        public InMemoryItemRepository()
        {
            // Generate 10,000 items for benchmarks
            _items = Enumerable.Range(1, 10000)
                .Select(i => new Item(i, $"Item {i}", $"Description for item {i}"))
                .ToList();
        }

        public Task<int> CountAsync(CancellationToken ct) => Task.FromResult(_items.Count);

        public async IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize, CancellationToken ct)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize);

            foreach (var item in items)
            {
                await Task.Yield(); // Simulate async database query
                yield return item;
            }
        }

        public Task<List<Item>> GetItemsAsync(int page, int pageSize, CancellationToken ct)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize).ToList();
            return Task.FromResult(items);
        }
    }

    #endregion

    #region Buffered Flows

    public class BufferedSmallFlow : FlowDefinition<ItemRequest, List<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, List<Item>> flow)
        {
            flow.Handle<BufferedHandler>();
        }
    }

    public class BufferedMediumFlow : FlowDefinition<ItemRequest, List<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, List<Item>> flow)
        {
            flow.Handle<BufferedHandler>();
        }
    }

    public class BufferedLargeFlow : FlowDefinition<ItemRequest, List<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, List<Item>> flow)
        {
            flow.Handle<BufferedHandler>();
        }
    }

    public class BufferedHandler : IFlowHandler<ItemRequest, List<Item>>
    {
        private readonly IItemRepository _repository;

        public BufferedHandler(IItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<List<Item>> HandleAsync(ItemRequest request, FlowContext context)
        {
            return await _repository.GetItemsAsync(request.Page, request.PageSize, context.CancellationToken);
        }
    }

    #endregion

    #region Streaming Flows

    public class StreamingSmallFlow : FlowDefinition<ItemRequest, PagedStreamResponse<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, PagedStreamResponse<Item>> flow)
        {
            flow.Handle<StreamingHandler>();
        }
    }

    public class StreamingMediumFlow : FlowDefinition<ItemRequest, PagedStreamResponse<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, PagedStreamResponse<Item>> flow)
        {
            flow.Handle<StreamingHandler>();
        }
    }

    public class StreamingLargeFlow : FlowDefinition<ItemRequest, PagedStreamResponse<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, PagedStreamResponse<Item>> flow)
        {
            flow.Handle<StreamingHandler>();
        }
    }

    public class StreamingHandler : IFlowHandler<ItemRequest, PagedStreamResponse<Item>>
    {
        private readonly IItemRepository _repository;

        public StreamingHandler(IItemRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<PagedStreamResponse<Item>> HandleAsync(ItemRequest request, FlowContext context)
        {
            var totalCount = await _repository.CountAsync(context.CancellationToken);
            var items = _repository.StreamItemsAsync(request.Page, request.PageSize, context.CancellationToken);

            return new PagedStreamResponse<Item>
            {
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            };
        }
    }

    #endregion
}
