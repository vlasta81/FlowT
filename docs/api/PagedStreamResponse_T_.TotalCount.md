## PagedStreamResponse\<T\>\.TotalCount Property

Gets or initializes the total number of items across all pages\.

```csharp
public int TotalCount { get; init; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

### Remarks
This should be the total count of items matching the query, not just the current page\.
Used by clients to calculate total pages and display pagination UI\.