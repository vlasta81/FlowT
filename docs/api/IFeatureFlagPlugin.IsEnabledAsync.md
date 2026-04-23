#### [FlowT](index.md 'index')
### [FlowT\.Plugins](FlowT.Plugins.md 'FlowT\.Plugins').[IFeatureFlagPlugin](IFeatureFlagPlugin.md 'FlowT\.Plugins\.IFeatureFlagPlugin')

## IFeatureFlagPlugin\.IsEnabledAsync Method

| Overloads | |
| :--- | :--- |
| [IsEnabledAsync\(string, CancellationToken\)](IFeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken) 'FlowT\.Plugins\.IFeatureFlagPlugin\.IsEnabledAsync\(string, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled\. The result is cached per feature name for the duration of the current flow execution\. |
| [IsEnabledAsync&lt;TContext&gt;\(string, TContext, CancellationToken\)](IFeatureFlagPlugin.IsEnabledAsync.md#FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken) 'FlowT\.Plugins\.IFeatureFlagPlugin\.IsEnabledAsync\<TContext\>\(string, TContext, System\.Threading\.CancellationToken\)') | Checks whether the named feature flag is enabled for the given contextual value \(e\.g\. a `TargetingContext` with `UserId` / `Groups`\)\. The result is cached per feature name for the duration of the current flow execution\. |

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken)'></a>

## IFeatureFlagPlugin\.IsEnabledAsync\(string, CancellationToken\) Method

Checks whether the named feature flag is enabled\.
The result is cached per feature name for the duration of the current flow execution\.

```csharp
System.Threading.Tasks.ValueTask<bool> IsEnabledAsync(string feature, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken).feature'></a>

`feature` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the feature flag to evaluate\.

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync(string,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
`true` if the feature is enabled; otherwise `false`\.

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken)'></a>

## IFeatureFlagPlugin\.IsEnabledAsync\<TContext\>\(string, TContext, CancellationToken\) Method

Checks whether the named feature flag is enabled for the given contextual value
\(e\.g\. a `TargetingContext` with `UserId` / `Groups`\)\.
The result is cached per feature name for the duration of the current flow execution\.

```csharp
System.Threading.Tasks.ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext featureContext, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Type parameters

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken).TContext'></a>

`TContext`

The type of the context object passed to feature filters\.
#### Parameters

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken).feature'></a>

`feature` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the feature flag to evaluate\.

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken).featureContext'></a>

`featureContext` [TContext](IFeatureFlagPlugin.md#FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken).TContext 'FlowT\.Plugins\.IFeatureFlagPlugin\.IsEnabledAsync\<TContext\>\(string, TContext, System\.Threading\.CancellationToken\)\.TContext')

The context used by contextual feature filters\.

<a name='FlowT.Plugins.IFeatureFlagPlugin.IsEnabledAsync_TContext_(string,TContext,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A cancellation token to observe\.

#### Returns
[System\.Threading\.Tasks\.ValueTask&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1 'System\.Threading\.Tasks\.ValueTask\`1')  
`true` if the feature is enabled; otherwise `false`\.

---
Generated by [DefaultDocumentation](https://github.com/Doraku/DefaultDocumentation 'https://github\.com/Doraku/DefaultDocumentation')