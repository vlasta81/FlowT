## CorrelationPlugin Class

Default implementation of [ICorrelationPlugin](ICorrelationPlugin.md 'FlowT\.Plugins\.ICorrelationPlugin')\.
Inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') so that [FlowContext](FlowContext.md 'FlowT\.FlowContext') is injected automatically\.

```csharp
public class CorrelationPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.ICorrelationPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; CorrelationPlugin

Implements [ICorrelationPlugin](ICorrelationPlugin.md 'FlowT\.Plugins\.ICorrelationPlugin')

| Properties | |
| :--- | :--- |
| [CorrelationId](CorrelationPlugin.CorrelationId.md 'FlowT\.Plugins\.CorrelationPlugin\.CorrelationId') | Gets the correlation ID for the current flow execution\. Resolved from the `X-Correlation-Id` request header when available; falls back to the flow's own ID \([FlowIdString](FlowContext.FlowIdString.md 'FlowT\.FlowContext\.FlowIdString')\)\. |
