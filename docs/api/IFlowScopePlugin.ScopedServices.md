## IFlowScopePlugin\.ScopedServices Property

Gets the [System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider') from the dedicated scope for this flow\.
The scope is created lazily on the first access to this property\.

```csharp
System.IServiceProvider ScopedServices { get; }
```

#### Property Value
[System\.IServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider 'System\.IServiceProvider')