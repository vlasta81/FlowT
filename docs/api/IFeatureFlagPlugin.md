## IFeatureFlagPlugin Interface

Built\-in plugin that evaluates feature flags for the current flow execution using
[Microsoft\.FeatureManagement\.IVariantFeatureManager](https://learn.microsoft.com/en-us/dotnet/api/microsoft.featuremanagement.ivariantfeaturemanager 'Microsoft\.FeatureManagement\.IVariantFeatureManager') from `Microsoft.FeatureManagement`\.
Results are cached per feature name for the lifetime of the flow so that the same
feature is never evaluated more than once per pipeline execution\.

```csharp
public interface IFeatureFlagPlugin
```

Derived  
&#8627; [FeatureFlagPlugin](FeatureFlagPlugin.md 'FlowT\.Plugins\.FeatureFlagPlugin')

### Remarks

Prerequisites — register feature management before registering the plugin:

```csharp
// Program.cs
builder.Services.AddFeatureManagement();           // reads from appsettings.json
// — or —
builder.Services.AddFeatureManagement()
                .WithTargeting();                  // adds audience-targeting support

services.AddFlowPlugin<IFeatureFlagPlugin, FeatureFlagPlugin>();
```

Configuration (`appsettings.json`):

```csharp
{
  "FeatureManagement": {
    "NewCheckout": true,
    "BetaSearch": false
  }
}
```

Usage inside a flow handler or specification:

```csharp
var ff = context.Plugin<IFeatureFlagPlugin>();

// Simple on/off gate
if (!await ff.IsEnabledAsync("NewCheckout"))
    return FlowInterrupt<CheckoutResponse>.Fail("Feature not available.", 403);

// Contextual gate (e.g. targeting a specific user or tenant)
bool enabled = await ff.IsEnabledAsync("BetaSearch", new TargetingContext
{
    UserId = context.GetUserId(),
    Groups = new[] { "Beta" }
});

// Read from cache populated earlier in the same flow
if (ff.TryGetCached("NewCheckout", out bool cached))
    logger.LogDebug("Cache hit: NewCheckout={Value}", cached);
```

| Properties | |
| :--- | :--- |
| [Cache](IFeatureFlagPlugin.Cache.md 'FlowT\.Plugins\.IFeatureFlagPlugin\.Cache') | Returns a read\-only view of all feature flag results evaluated during the current flow\. Keys are feature names; values are the last cached result for that feature\. |

| Methods | |
| :--- | :--- |
| [IsEnabledAsync\(string, CancellationToken\)](IFeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken) 'FlowT\.Plugins\.IFeatureFlagPlugin\.IsEnabledAsync\(string, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled\. The result is cached per feature name for the duration of the current flow execution\. |
| [IsEnabledAsync&lt;TContext&gt;\(string, TContext, CancellationToken\)](IFeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken) 'FlowT\.Plugins\.IFeatureFlagPlugin\.IsEnabledAsync\<TContext\>\(string, TContext, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled for the given contextual value \(e\.g\. a `TargetingContext` with `UserId` / `Groups`\)\. The result is cached per feature name for the duration of the current flow execution\. |
| [TryGetCached\(string, bool\)](IFeatureFlagPlugin.TryGetCached.PQSZEZ2NK94F8EXHUNDIM13O7.md 'FlowT\.Plugins\.IFeatureFlagPlugin\.TryGetCached\(string, bool\)') | Attempts to read a previously cached feature flag result without calling [Microsoft\.FeatureManagement\.IVariantFeatureManager](https://learn.microsoft.com/en-us/dotnet/api/microsoft.featuremanagement.ivariantfeaturemanager 'Microsoft\.FeatureManagement\.IVariantFeatureManager')\. |
