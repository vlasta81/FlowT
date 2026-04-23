## CorrelationPlugin\.CorrelationId Property

Gets the correlation ID for the current flow execution\.
Resolved from the `X-Correlation-Id` request header when available;
falls back to the flow's own ID \([FlowIdString](FlowContext.FlowIdString.md 'FlowT\.FlowContext\.FlowIdString')\)\.

```csharp
public string CorrelationId { get; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')