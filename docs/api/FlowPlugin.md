## FlowPlugin Class

Abstract base class for plugins that need direct access to the [FlowContext](FlowContext.md 'FlowT\.FlowContext')\.
Inherit from this class to get an automatically injected [Context](FlowPlugin.Context.md 'FlowT\.Abstractions\.FlowPlugin\.Context') property
without exposing initialization details to external callers\.

```csharp
public abstract class FlowPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; FlowPlugin

Derived  
&#8627; [AuditPlugin](AuditPlugin.md 'FlowT\.Plugins\.AuditPlugin')  
&#8627; [CorrelationPlugin](CorrelationPlugin.md 'FlowT\.Plugins\.CorrelationPlugin')  
&#8627; [FeatureFlagPlugin](FeatureFlagPlugin.md 'FlowT\.Plugins\.FeatureFlagPlugin')  
&#8627; [FlowScopePlugin](FlowScopePlugin.md 'FlowT\.Plugins\.FlowScopePlugin')  
&#8627; [IdempotencyPlugin](IdempotencyPlugin.md 'FlowT\.Plugins\.IdempotencyPlugin')  
&#8627; [PerformancePlugin](PerformancePlugin.md 'FlowT\.Plugins\.PerformancePlugin')  
&#8627; [RetryStatePlugin](RetryStatePlugin.md 'FlowT\.Plugins\.RetryStatePlugin')  
&#8627; [TenantPlugin](TenantPlugin.md 'FlowT\.Plugins\.TenantPlugin')  
&#8627; [TransactionPlugin](TransactionPlugin.md 'FlowT\.Plugins\.TransactionPlugin')  
&#8627; [UserIdentityPlugin](UserIdentityPlugin.md 'FlowT\.Plugins\.UserIdentityPlugin')

### Remarks

When [Plugin&lt;T&gt;\(\)](FlowContext.Plugin_T_().md 'FlowT\.FlowContext\.Plugin\<T\>\(\)') resolves a plugin that inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin'),
it automatically binds the current [FlowContext](FlowContext.md 'FlowT\.FlowContext') before returning the instance.
The [Context](FlowPlugin.Context.md 'FlowT\.Abstractions\.FlowPlugin\.Context') property is available for all subsequent method calls within the same flow execution.

The plugin is <strong>PerFlow</strong> — one instance per [FlowContext](FlowContext.md 'FlowT\.FlowContext') execution,
shared across all pipeline stages (specifications, policies, handler).

Plugins that do not need [FlowContext](FlowContext.md 'FlowT\.FlowContext') access do not have to inherit from this class.\<example\>
  \<code\>
             public class AuditPlugin : FlowPlugin, IAuditPlugin
             \{
                 private readonly ILogger&lt;AuditPlugin&gt; \_logger;
            
                 public AuditPlugin\(ILogger&lt;AuditPlugin&gt; logger\) =&gt; \_logger = logger;
            
                 public void Record\(string action\)
                 \{
                     \_logger\.LogInformation\("\[\{FlowId\}\] \{Action\}", Context\.GetFlowIdString\(\), action\);
                     Context\.Set\(new AuditEntry\(action, Context\.StartedAt\)\);
                     Context\.PublishInBackground\(new AuditEvent\(action\), Context\.CancellationToken\);
                 \}
             \}
             \</code\>
\</example\>

| Properties | |
| :--- | :--- |
| [Context](FlowPlugin.Context.md 'FlowT\.Abstractions\.FlowPlugin\.Context') | Gets the [FlowContext](FlowContext.md 'FlowT\.FlowContext') this plugin is bound to\. Available after the plugin is resolved via [Plugin&lt;T&gt;\(\)](FlowContext.Plugin_T_().md 'FlowT\.FlowContext\.Plugin\<T\>\(\)')\. |
