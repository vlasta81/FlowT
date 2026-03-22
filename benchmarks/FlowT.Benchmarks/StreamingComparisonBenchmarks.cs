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
/// Demonstrates the ACTUAL overhead of streaming vs buffered responses.
/// Compares "Pure Sync" (no Task.Yield) vs "Async Simulation" (with Task.Yield).
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class StreamingComparisonBenchmarks
{
    private IServiceProvider _services = null!;
    private BufferedFlow _bufferedFlow = null!;
    private StreamingSyncFlow _streamingSyncFlow = null!;
    private StreamingAsyncFlow _streamingAsyncFlow = null!;
    private FlowContext _context = null!;
    private ItemRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register repositories
        services.AddSingleton<ISyncRepository, SyncRepository>();
        services.AddSingleton<IAsyncRepository, AsyncRepository>();

        // Register flows
        services.AddSingleton<BufferedFlow>();
        services.AddSingleton<StreamingSyncFlow>();
        services.AddSingleton<StreamingAsyncFlow>();

        _services = services.BuildServiceProvider();
        _bufferedFlow = _services.GetRequiredService<BufferedFlow>();
        _streamingSyncFlow = _services.GetRequiredService<StreamingSyncFlow>();
        _streamingAsyncFlow = _services.GetRequiredService<StreamingAsyncFlow>();

        _context = new FlowContext
        {
            Services = _services,
            CancellationToken = CancellationToken.None
        };

        _request = new ItemRequest { Page = 0, PageSize = 1000 };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        (_services as IDisposable)?.Dispose();
    }

    [Benchmark(Baseline = true, Description = "Buffered (List<T>)")]
    public async Task<int> Buffered_1000Items()
    {
        var response = await _bufferedFlow.ExecuteAsync(_request, _context);
        return response.Count;
    }

    [Benchmark(Description = "Streaming (Sync - no Task.Yield)")]
    public async Task<int> StreamingSync_1000Items()
    {
        var response = await _streamingSyncFlow.ExecuteAsync(_request, _context);
        var count = 0;
        await foreach (var _ in response.Items)
        {
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Streaming (Async - with Task.Yield)")]
    public async Task<int> StreamingAsync_1000Items()
    {
        var response = await _streamingAsyncFlow.ExecuteAsync(_request, _context);
        var count = 0;
        await foreach (var _ in response.Items)
        {
            count++;
        }
        return count;
    }

    #region Models

    public record ItemRequest
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
    }

    public record Item(int Id, string Name);

    #endregion

    #region Repositories

    // Synchronous streaming (no Task.Yield - shows real overhead)
    public interface ISyncRepository
    {
        IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize);
        Task<List<Item>> GetItemsAsync(int page, int pageSize);
    }

    public class SyncRepository : ISyncRepository
    {
        private readonly List<Item> _items;

        public SyncRepository()
        {
            _items = Enumerable.Range(1, 10000)
                .Select(i => new Item(i, $"Item {i}"))
                .ToList();
        }

        public async IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize);

            foreach (var item in items)
            {
                // NO Task.Yield() - pure synchronous iteration
                yield return item;
            }
            await Task.CompletedTask; // Satisfy async signature
        }

        public Task<List<Item>> GetItemsAsync(int page, int pageSize)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize).ToList();
            return Task.FromResult(items);
        }
    }

    // Asynchronous streaming (with Task.Yield - simulates real async I/O)
    public interface IAsyncRepository
    {
        IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize);
    }

    public class AsyncRepository : IAsyncRepository
    {
        private readonly List<Item> _items;

        public AsyncRepository()
        {
            _items = Enumerable.Range(1, 10000)
                .Select(i => new Item(i, $"Item {i}"))
                .ToList();
        }

        public async IAsyncEnumerable<Item> StreamItemsAsync(int page, int pageSize)
        {
            var skip = page * pageSize;
            var items = _items.Skip(skip).Take(pageSize);

            foreach (var item in items)
            {
                await Task.Yield(); // Simulate async database query
                yield return item;
            }
        }
    }

    #endregion

    #region Flows

    public class BufferedFlow : FlowDefinition<ItemRequest, List<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, List<Item>> flow)
        {
            flow.Handle<BufferedHandler>();
        }
    }

    public class BufferedHandler : IFlowHandler<ItemRequest, List<Item>>
    {
        private readonly ISyncRepository _repository;

        public BufferedHandler(ISyncRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask<List<Item>> HandleAsync(ItemRequest request, FlowContext context)
        {
            return await _repository.GetItemsAsync(request.Page, request.PageSize);
        }
    }

    public class StreamingSyncFlow : FlowDefinition<ItemRequest, PagedStreamResponse<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, PagedStreamResponse<Item>> flow)
        {
            flow.Handle<StreamingSyncHandler>();
        }
    }

    public class StreamingSyncHandler : IFlowHandler<ItemRequest, PagedStreamResponse<Item>>
    {
        private readonly ISyncRepository _repository;

        public StreamingSyncHandler(ISyncRepository repository)
        {
            _repository = repository;
        }

        public ValueTask<PagedStreamResponse<Item>> HandleAsync(ItemRequest request, FlowContext context)
        {
            var items = _repository.StreamItemsAsync(request.Page, request.PageSize);

            return ValueTask.FromResult(new PagedStreamResponse<Item>
            {
                TotalCount = 10000,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            });
        }
    }

    public class StreamingAsyncFlow : FlowDefinition<ItemRequest, PagedStreamResponse<Item>>
    {
        protected override void Configure(IFlowBuilder<ItemRequest, PagedStreamResponse<Item>> flow)
        {
            flow.Handle<StreamingAsyncHandler>();
        }
    }

    public class StreamingAsyncHandler : IFlowHandler<ItemRequest, PagedStreamResponse<Item>>
    {
        private readonly IAsyncRepository _repository;

        public StreamingAsyncHandler(IAsyncRepository repository)
        {
            _repository = repository;
        }

        public ValueTask<PagedStreamResponse<Item>> HandleAsync(ItemRequest request, FlowContext context)
        {
            var items = _repository.StreamItemsAsync(request.Page, request.PageSize);

            return ValueTask.FromResult(new PagedStreamResponse<Item>
            {
                TotalCount = 10000,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            });
        }
    }

    #endregion
}
