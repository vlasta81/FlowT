## IIdempotencyPlugin Interface

Built\-in plugin that reads and caches the idempotency key for the current flow execution\.
The key is read once from the `X-Idempotency-Key` request header and shared across
all pipeline stages \(specifications, policies, handler\)\.

```csharp
public interface IIdempotencyPlugin
```

Derived  
&#8627; [IdempotencyPlugin](IdempotencyPlugin.md 'FlowT\.Plugins\.IdempotencyPlugin')

### Remarks
Register via `services.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>()`\.

Usage:

```csharp
var idempotency = context.Plugin<IIdempotencyPlugin>();
if (idempotency.HasKey && await store.ExistsAsync(idempotency.Key!))
    return cachedResponse;

// ... process request ...
await store.StoreAsync(idempotency.Key!, response);
```

| Properties | |
| :--- | :--- |
| [HasKey](IIdempotencyPlugin.HasKey.md 'FlowT\.Plugins\.IIdempotencyPlugin\.HasKey') | Gets a value indicating whether an idempotency key was present in the request\. `false` in non\-HTTP scenarios or when the `X-Idempotency-Key` header is absent\. |
| [Key](IIdempotencyPlugin.Key.md 'FlowT\.Plugins\.IIdempotencyPlugin\.Key') | Gets the idempotency key from the `X-Idempotency-Key` request header, or `null` when the header is absent or in non\-HTTP scenarios\. |
