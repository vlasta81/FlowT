## ICorrelationPlugin Interface

Built\-in plugin that provides a stable correlation ID for the current flow execution\.
The ID is resolved once per flow and cached\.

```csharp
public interface ICorrelationPlugin
```

Derived  
&#8627; [CorrelationPlugin](CorrelationPlugin.md 'FlowT\.Plugins\.CorrelationPlugin')

### Remarks
Resolution order:
1. Value of the `X-Correlation-Id` request header (when an [HttpContext](FlowContext.HttpContext.md 'FlowT\.FlowContext\.HttpContext') is present).
2. The flow's own ID via [FlowIdString](FlowContext.FlowIdString.md 'FlowT\.FlowContext\.FlowIdString') (non-HTTP scenarios or missing header).

Register via `services.AddFlowPlugin<ICorrelationPlugin, CorrelationPlugin>()`\.

Usage:

```csharp
var correlation = context.Plugin<ICorrelationPlugin>();
logger.LogInformation("[{CorrelationId}] Processing request", correlation.CorrelationId);
```

| Properties | |
| :--- | :--- |
| [CorrelationId](ICorrelationPlugin.CorrelationId.md 'FlowT\.Plugins\.ICorrelationPlugin\.CorrelationId') | Gets the correlation ID for the current flow execution\. Resolved from the `X-Correlation-Id` request header when available; falls back to the flow's own ID \([FlowIdString](FlowContext.FlowIdString.md 'FlowT\.FlowContext\.FlowIdString')\)\. |
