## PagedStreamResponse\<T\> Class

Generic paginated streaming response that automatically serializes pagination metadata and progressively streams items\.
Extends [StreamableResponse](StreamableResponse.md 'FlowT\.Abstractions\.StreamableResponse') to provide zero\-boilerplate pagination support with optimal memory usage\.

```csharp
public class PagedStreamResponse<T> : FlowT.Abstractions.StreamableResponse
```
#### Type parameters

<a name='FlowT.Abstractions.PagedStreamResponse_T_.T'></a>

`T`

The type of items in the paginated collection\.

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [StreamableResponse](StreamableResponse.md 'FlowT\.Abstractions\.StreamableResponse') &#129106; PagedStreamResponse\<T\>

### Remarks

This class provides a convenient way to return paginated data from flows without manually implementing
the streaming logic. It automatically handles:
- Pagination metadata (totalCount, page, pageSize, hasMore)
- Progressive item streaming via [System\.Collections\.Generic\.IAsyncEnumerable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1 'System\.Collections\.Generic\.IAsyncEnumerable\`1')
- Automatic JSON structure generation
- Memory-efficient serialization (items are not buffered)

<strong>Example JSON Output:</strong>

```csharp
{
  "totalCount": 10000,
  "page": 0,
  "pageSize": 100,
  "hasMore": true,
  "items": [
    {"id": 1, "name": "Item 1"},
    {"id": 2, "name": "Item 2"},
    ...
  ]
}
```

<strong>Usage Example:</strong>

```csharp
public class GetUsersHandler : IFlowHandler<GetUsersRequest, PagedStreamResponse<UserDto>>
{
    public async ValueTask<PagedStreamResponse<UserDto>> HandleAsync(
        GetUsersRequest request,
        FlowContext context)
    {
        // ✅ Resolve repository per-request (safe in singleton handlers)
        var repository = context.Service<IUserRepository>();
        var totalCount = await repository.CountAsync(context.CancellationToken);
        var items = repository.StreamAsync(request.Page, request.PageSize, context.CancellationToken);

        return new PagedStreamResponse<UserDto>
        {
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Items = items
        };
    }
}
```

<strong>Performance Characteristics:</strong>
- <strong>Memory:</strong> O(1) - Only current item buffered, not entire collection
- <strong>Latency:</strong> First byte sent immediately after metadata (~50 KB buffer)
- <strong>Throughput:</strong> 2-3× faster than buffered responses for large datasets
- <strong>Allocations:</strong> ~99% reduction vs List<T> for collections > 1000 items

<strong>When to Use:</strong>
- Paginated API endpoints (user lists, product catalogs, search results)
- Large result sets where client needs pagination metadata
- Standardized pagination structure across all endpoints

<strong>When NOT to Use:</strong>
- Small datasets (<100 items) - use List<T> for simplicity
- Custom JSON structure - use [StreamableResponse](StreamableResponse.md 'FlowT\.Abstractions\.StreamableResponse') or [IStreamableResponse](IStreamableResponse.md 'FlowT\.Contracts\.IStreamableResponse') directly
- Non-paginated infinite streams - use pure IAsyncEnumerable<T>

| Properties | |
| :--- | :--- |
| [HasMore](PagedStreamResponse_T_.HasMore.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.HasMore') | Gets a value indicating whether there are more pages available after the current page\. |
| [Items](PagedStreamResponse_T_.Items.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.Items') | Gets or initializes the async enumerable sequence of items for the current page\. |
| [Page](PagedStreamResponse_T_.Page.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.Page') | Gets or initializes the zero\-based page index\. |
| [PageSize](PagedStreamResponse_T_.PageSize.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.PageSize') | Gets or initializes the maximum number of items per page\. |
| [TotalCount](PagedStreamResponse_T_.TotalCount.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.TotalCount') | Gets or initializes the total number of items across all pages\. |

| Methods | |
| :--- | :--- |
| [WriteItemsAsync\(Utf8JsonWriter, CancellationToken\)](PagedStreamResponse_T_.WriteItemsAsync.3BUEEYED65S2UZWDAXNIZRQW4.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.WriteItemsAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Progressively writes items from [Items](PagedStreamResponse_T_.Items.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.Items') to the JSON array\. |
| [WriteMetadataAsync\(Utf8JsonWriter, CancellationToken\)](PagedStreamResponse_T_.WriteMetadataAsync.D471ZQWC2C2GZ4FCBUEGXT52C.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.WriteMetadataAsync\(System\.Text\.Json\.Utf8JsonWriter, System\.Threading\.CancellationToken\)') | Writes pagination metadata \(totalCount, page, pageSize, hasMore\) to the JSON stream\. |
