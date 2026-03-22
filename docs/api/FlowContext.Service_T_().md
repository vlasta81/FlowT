## FlowContext\.Service\<T\>\(\) Method

Resolves a required service from the dependency injection container\.
This is a convenience method for [Microsoft\.Extensions\.DependencyInjection\.ServiceProviderServiceExtensions\.GetRequiredService&lt;&gt;\.IServiceProvider\)](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceproviderserviceextensions.getrequiredservice--1#microsoft-extensions-dependencyinjection-serviceproviderserviceextensions-getrequiredservice--1(system-iserviceprovider) 'Microsoft\.Extensions\.DependencyInjection\.ServiceProviderServiceExtensions\.GetRequiredService\`\`1\(System\.IServiceProvider\)')\.

```csharp
public T Service<T>()
    where T : notnull;
```
#### Type parameters

<a name='FlowT.FlowContext.Service_T_().T'></a>

`T`

The type of service to resolve\.

#### Returns
[T](FlowContext.Service_T_().md#FlowT.FlowContext.Service_T_().T 'FlowT\.FlowContext\.Service\<T\>\(\)\.T')  
The resolved service instance\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown when the service is not registered\.

### Remarks
Use this method to resolve scoped services \(e\.g\., DbContext\) in singleton handlers\.
This ensures each request gets its own scoped service instance\.
Example: `var db = context.Service<DbContext>();`