## PagedStreamResponse\<T\>\.PageSize Property

Gets or initializes the maximum number of items per page\.

```csharp
public int PageSize { get; init; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

### Remarks
This represents the requested page size\. The actual number of items in [Items](PagedStreamResponse_T_.Items.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>\.Items')
may be less than this value for the last page or if fewer items are available\.