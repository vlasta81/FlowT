## IdempotencyPlugin\.HasKey Property

Gets a value indicating whether an idempotency key was present in the request\.
`false` in non\-HTTP scenarios or when the `X-Idempotency-Key` header is absent\.

```csharp
public bool HasKey { get; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')