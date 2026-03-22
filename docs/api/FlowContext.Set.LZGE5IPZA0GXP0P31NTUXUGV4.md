## FlowContext\.Set\<T\>\(T, string\) Method

Stores a value in the context's shared state, keyed by its type and optional string key\.
This method is thread\-safe and optimized using [System\.Runtime\.InteropServices\.CollectionsMarshal](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal 'System\.Runtime\.InteropServices\.CollectionsMarshal') for minimal allocations\.

```csharp
public void Set<T>(T value, string? key=null);
```
#### Type parameters

<a name='FlowT.FlowContext.Set_T_(T,string).T'></a>

`T`

The type of value to store \(used as part of the composite key\)\.
#### Parameters

<a name='FlowT.FlowContext.Set_T_(T,string).value'></a>

`value` [T](FlowContext.Set.LZGE5IPZA0GXP0P31NTUXUGV4.md#FlowT.FlowContext.Set_T_(T,string).T 'FlowT\.FlowContext\.Set\<T\>\(T, string\)\.T')

The value to store\.

<a name='FlowT.FlowContext.Set_T_(T,string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Optional string key to store multiple values of the same type under different keys\.

### Remarks
When [key](FlowContext.Set.LZGE5IPZA0GXP0P31NTUXUGV4.md#FlowT.FlowContext.Set_T_(T,string).key 'FlowT\.FlowContext\.Set\<T\>\(T, string\)\.key') is null, only one value of type [T](FlowContext.Set.LZGE5IPZA0GXP0P31NTUXUGV4.md#FlowT.FlowContext.Set_T_(T,string).T 'FlowT\.FlowContext\.Set\<T\>\(T, string\)\.T') can be stored\.
Use named keys to store multiple instances: Set\(user1, "admin"\), Set\(user2, "guest"\)\.