## IdempotencyPlugin Class

Default implementation of [IIdempotencyPlugin](IIdempotencyPlugin.md 'FlowT\.Plugins\.IIdempotencyPlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class IdempotencyPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IIdempotencyPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; IdempotencyPlugin

Implements [IIdempotencyPlugin](IIdempotencyPlugin.md 'FlowT\.Plugins\.IIdempotencyPlugin')

| Properties | |
| :--- | :--- |
| [HasKey](IdempotencyPlugin.HasKey.md 'FlowT\.Plugins\.IdempotencyPlugin\.HasKey') | Gets a value indicating whether an idempotency key was present in the request\. `false` in non\-HTTP scenarios or when the `X-Idempotency-Key` header is absent\. |
| [Key](IdempotencyPlugin.Key.md 'FlowT\.Plugins\.IdempotencyPlugin\.Key') | Gets the idempotency key from the `X-Idempotency-Key` request header, or `null` when the header is absent or in non\-HTTP scenarios\. |
