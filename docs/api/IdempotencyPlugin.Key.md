## IdempotencyPlugin\.Key Property

Gets the idempotency key from the `X-Idempotency-Key` request header,
or `null` when the header is absent or in non\-HTTP scenarios\.

```csharp
public string? Key { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')