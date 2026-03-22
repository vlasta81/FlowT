#### [FlowT](index.md 'index')
### [FlowT](FlowT.md 'FlowT').[FlowContext](FlowContext.md 'FlowT\.FlowContext')

## FlowContext\.GetOrAdd Method

| Overloads | |
| :--- | :--- |
| [GetOrAdd&lt;T,TArg&gt;\(TArg, Func&lt;TArg,T&gt;, string\)](FlowContext.GetOrAdd.md#FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string) 'FlowT\.FlowContext\.GetOrAdd\<T,TArg\>\(TArg, System\.Func\<TArg,T\>, string\)') | Gets an existing value from the context or adds a new one using the provided factory with an argument\. This overload avoids closure allocation when the factory needs a parameter\. |
| [GetOrAdd&lt;T&gt;\(Func&lt;T&gt;, string\)](FlowContext.GetOrAdd.md#FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string) 'FlowT\.FlowContext\.GetOrAdd\<T\>\(System\.Func\<T\>, string\)') | Gets an existing value from the context or adds a new one using the provided factory\. This method is thread\-safe\. |

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string)'></a>

## FlowContext\.GetOrAdd\<T,TArg\>\(TArg, Func\<TArg,T\>, string\) Method

Gets an existing value from the context or adds a new one using the provided factory with an argument\.
This overload avoids closure allocation when the factory needs a parameter\.

```csharp
public T GetOrAdd<T,TArg>(TArg arg, System.Func<TArg,T> factory, string? key=null);
```
#### Type parameters

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).T'></a>

`T`

The type of value to get or add\.

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).TArg'></a>

`TArg`

The type of argument to pass to the factory\.
#### Parameters

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).arg'></a>

`arg` [TArg](FlowContext.md#FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).TArg 'FlowT\.FlowContext\.GetOrAdd\<T,TArg\>\(TArg, System\.Func\<TArg,T\>, string\)\.TArg')

The argument to pass to the factory if a new value needs to be created\.

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).factory'></a>

`factory` [System\.Func&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')[TArg](FlowContext.md#FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).TArg 'FlowT\.FlowContext\.GetOrAdd\<T,TArg\>\(TArg, System\.Func\<TArg,T\>, string\)\.TArg')[,](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')[T](FlowContext.md#FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).T 'FlowT\.FlowContext\.GetOrAdd\<T,TArg\>\(TArg, System\.Func\<TArg,T\>, string\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-2 'System\.Func\`2')

A function to create the value if it doesn't exist\.

<a name='FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Optional string key for storing multiple values of the same type\.

#### Returns
[T](FlowContext.md#FlowT.FlowContext.GetOrAdd_T,TArg_(TArg,System.Func_TArg,T_,string).T 'FlowT\.FlowContext\.GetOrAdd\<T,TArg\>\(TArg, System\.Func\<TArg,T\>, string\)\.T')  
The existing or newly created value\.

<a name='FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string)'></a>

## FlowContext\.GetOrAdd\<T\>\(Func\<T\>, string\) Method

Gets an existing value from the context or adds a new one using the provided factory\.
This method is thread\-safe\.

```csharp
public T GetOrAdd<T>(System.Func<T> factory, string? key=null);
```
#### Type parameters

<a name='FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).T'></a>

`T`

The type of value to get or add\.
#### Parameters

<a name='FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).factory'></a>

`factory` [System\.Func&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-1 'System\.Func\`1')[T](FlowContext.md#FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).T 'FlowT\.FlowContext\.GetOrAdd\<T\>\(System\.Func\<T\>, string\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.func-1 'System\.Func\`1')

A function to create the value if it doesn't exist\.

<a name='FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).key'></a>

`key` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

Optional string key for storing multiple values of the same type\.

#### Returns
[T](FlowContext.md#FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).T 'FlowT\.FlowContext\.GetOrAdd\<T\>\(System\.Func\<T\>, string\)\.T')  
The existing or newly created value\.

### Remarks
When [key](FlowContext.md#FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).key 'FlowT\.FlowContext\.GetOrAdd\<T\>\(System\.Func\<T\>, string\)\.key') is null, uses the default key for type [T](FlowContext.md#FlowT.FlowContext.GetOrAdd_T_(System.Func_T_,string).T 'FlowT\.FlowContext\.GetOrAdd\<T\>\(System\.Func\<T\>, string\)\.T')\.
Use named keys for multiple instances: GetOrAdd\(\(\) =\> new Cache\(\), "users"\)\.

---
Generated by [DefaultDocumentation](https://github.com/Doraku/DefaultDocumentation 'https://github\.com/Doraku/DefaultDocumentation')