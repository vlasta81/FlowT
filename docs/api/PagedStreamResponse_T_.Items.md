## PagedStreamResponse\<T\>\.Items Property

Gets or initializes the async enumerable sequence of items for the current page\.

```csharp
public System.Collections.Generic.IAsyncEnumerable<T> Items { get; init; }
```

#### Property Value
[System\.Collections\.Generic\.IAsyncEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1 'System\.Collections\.Generic\.IAsyncEnumerable\`1')[T](PagedStreamResponse_T_.md#FlowT.Abstractions.PagedStreamResponse_T_.T 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1 'System\.Collections\.Generic\.IAsyncEnumerable\`1')

### Remarks

This should be an [System\.Collections\.Generic\.IAsyncEnumerable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1 'System\.Collections\.Generic\.IAsyncEnumerable\`1') that yields items progressively,
typically from a database query or other async data source. Items are serialized
one-by-one as they become available, enabling progressive response delivery.

<strong>Important:</strong> The enumerable is consumed only once during serialization.
            Ensure it produces exactly [PageSize](PagedStreamResponse_T_.PageSize.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.PageSize') items (or fewer for the last page).

Defaults to an empty async enumerable if not set.