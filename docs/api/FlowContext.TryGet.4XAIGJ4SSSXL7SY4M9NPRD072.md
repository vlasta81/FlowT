## FlowContext\.TryGet\<T\>\(T, string\) Method

Attempts to retrieve a value from the context's shared state\.
Uses a double\-check lock pattern for optimal read performance in the common case\.

```csharp
public bool TryGet<T>(out T value, string? key=null);
```
#### Type parameters

<a name='FlowT.FlowContext.TryGet_T_(T,string).T'></a>

`T`

The type of value to retrieve\.
#### Parameters

<a name='FlowT.FlowContext.TryGet_T_(T,string).value'></a>

`value` [T](FlowContext.TryGet.4XAIGJ4SSSXL7SY4M9NPRD072.md#FlowT.FlowContext.TryGet_T_(T,string).T 'FlowT\.FlowContext\.TryGet\<T\>\(T, string\)\.T')

When this method returns, contains the value if found; otherwise, the default value for [T](FlowContext.TryGet.4XAIGJ4SSSXL7SY4M9NPRD072.md#FlowT.FlowContext.TryGet_T_(T,string).T 'FlowT\.FlowContext\.TryGet\<T\>\(T, string\)\.T')\.

<a name='FlowT.FlowContext.TryGet_T_(T,string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Optional string key to retrieve a specific named value of type [T](FlowContext.TryGet.4XAIGJ4SSSXL7SY4M9NPRD072.md#FlowT.FlowContext.TryGet_T_(T,string).T 'FlowT\.FlowContext\.TryGet\<T\>\(T, string\)\.T')\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
`true` if the value was found; otherwise, `false`\.

### Remarks
When [key](FlowContext.TryGet.4XAIGJ4SSSXL7SY4M9NPRD072.md#FlowT.FlowContext.TryGet_T_(T,string).key 'FlowT\.FlowContext\.TryGet\<T\>\(T, string\)\.key') is null, retrieves the default value stored for type [T](FlowContext.TryGet.4XAIGJ4SSSXL7SY4M9NPRD072.md#FlowT.FlowContext.TryGet_T_(T,string).T 'FlowT\.FlowContext\.TryGet\<T\>\(T, string\)\.T')\.
Use named keys to retrieve specific instances: TryGet\(out user, "admin"\)\.