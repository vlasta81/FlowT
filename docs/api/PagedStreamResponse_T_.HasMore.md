## PagedStreamResponse\<T\>\.HasMore Property

Gets a value indicating whether there are more pages available after the current page\.

```csharp
public bool HasMore { get; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

### Remarks
Calculated as: \(Page \+ 1\) \* PageSize \< TotalCount

This is a convenience property for clients to determine if they should display
a "Next Page" button or make additional requests.