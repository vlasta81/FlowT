## FeatureFlagPlugin Class

Default implementation of [IFeatureFlagPlugin](IFeatureFlagPlugin.md 'FlowT\.Plugins\.IFeatureFlagPlugin')\.
Resolves [Microsoft\.FeatureManagement\.IVariantFeatureManager](https://learn.microsoft.com/en-us/dotnet/api/microsoft.featuremanagement.ivariantfeaturemanager 'Microsoft\.FeatureManagement\.IVariantFeatureManager') from the DI container via constructor injection
and inherits from [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') to get access to [FlowContext](FlowContext.md 'FlowT\.FlowContext')\.

```csharp
public class FeatureFlagPlugin : FlowT.Abstractions.FlowPlugin, FlowT.Plugins.IFeatureFlagPlugin
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; [FlowPlugin](FlowPlugin.md 'FlowT\.Abstractions\.FlowPlugin') &#129106; FeatureFlagPlugin

Implements [IFeatureFlagPlugin](IFeatureFlagPlugin.md 'FlowT\.Plugins\.IFeatureFlagPlugin')

| Constructors | |
| :--- | :--- |
| [FeatureFlagPlugin\(IVariantFeatureManager\)](FeatureFlagPlugin..ctor.L1U582XFGDLAG0YUXKE4I56M8.md 'FlowT\.Plugins\.FeatureFlagPlugin\.FeatureFlagPlugin\(Microsoft\.FeatureManagement\.IVariantFeatureManager\)') | Initializes a new instance of [FeatureFlagPlugin](FeatureFlagPlugin.md 'FlowT\.Plugins\.FeatureFlagPlugin')\. |

| Properties | |
| :--- | :--- |
| [Cache](FeatureFlagPlugin.Cache.md 'FlowT\.Plugins\.FeatureFlagPlugin\.Cache') | Returns a read\-only view of all feature flag results evaluated during the current flow\. Keys are feature names; values are the last cached result for that feature\. |

| Methods | |
| :--- | :--- |
| [IsEnabledAsync\(string, CancellationToken\)](FeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.FeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken) 'FlowT\.Plugins\.FeatureFlagPlugin\.IsEnabledAsync\(string, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled\. The result is cached per feature name for the duration of the current flow execution\. |
| [IsEnabledAsync&lt;TContext&gt;\(string, TContext, CancellationToken\)](FeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.FeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken) 'FlowT\.Plugins\.FeatureFlagPlugin\.IsEnabledAsync\<TContext\>\(string, TContext, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled for the given contextual value \(e\.g\. a `TargetingContext` with `UserId` / `Groups`\)\. The result is cached per feature name for the duration of the current flow execution\. |
| [TryGetCached\(string, bool\)](FeatureFlagPlugin.TryGetCached.2QXFRJSTSI8AY1A19G6KYNTLC.md 'FlowT\.Plugins\.FeatureFlagPlugin\.TryGetCached\(string, bool\)') | Attempts to read a previously cached feature flag result without calling [Microsoft\.FeatureManagement\.IVariantFeatureManager](https://learn.microsoft.com/en-us/dotnet/api/microsoft.featuremanagement.ivariantfeaturemanager 'Microsoft\.FeatureManagement\.IVariantFeatureManager')\. |
