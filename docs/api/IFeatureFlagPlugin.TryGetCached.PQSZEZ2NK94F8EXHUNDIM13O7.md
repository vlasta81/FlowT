## IFeatureFlagPlugin\.TryGetCached\(string, bool\) Method

Attempts to read a previously cached feature flag result without calling
[Microsoft\.FeatureManagement\.IVariantFeatureManager](https://learn.microsoft.com/en-us/dotnet/api/microsoft.featuremanagement.ivariantfeaturemanager 'Microsoft\.FeatureManagement\.IVariantFeatureManager')\.

```csharp
bool TryGetCached(string feature, out bool value);
```
#### Parameters

<a name='FlowT.Plugins.IFeatureFlagPlugin.TryGetCached(string,bool).feature'></a>

`feature` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The feature flag name\.

<a name='FlowT.Plugins.IFeatureFlagPlugin.TryGetCached(string,bool).value'></a>

`value` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

When this method returns `true`, the cached value\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
`true` if a cached value exists; otherwise `false`\.