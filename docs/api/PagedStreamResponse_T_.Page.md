## PagedStreamResponse\<T\>\.Page Property

Gets or initializes the zero\-based page index\.

```csharp
public int Page { get; init; }
```

#### Property Value
[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

### Remarks
Page numbering starts at 0\. For example:
- Page = 0: First page (items 0-99 for PageSize=100)
- Page = 1: Second page (items 100-199 for PageSize=100)
- Page = N: Items from (N * PageSize) to ((N + 1) * PageSize - 1)