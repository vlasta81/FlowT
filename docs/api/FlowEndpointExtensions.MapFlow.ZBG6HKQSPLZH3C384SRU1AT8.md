## FlowEndpointExtensions\.MapFlow\<TFlow,TRequest,TResponse\>\(this IEndpointRouteBuilder, string, string\) Method

Maps a flow to an HTTP endpoint with automatic request binding and response serialization\.
The flow is resolved from dependency injection and executed with a new [FlowContext](FlowContext.md 'FlowT\.FlowContext')\.

```csharp
public static Microsoft.AspNetCore.Builder.RouteHandlerBuilder MapFlow<TFlow,TRequest,TResponse>(this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app, string pattern, string httpMethod)
    where TFlow : FlowT.Contracts.IFlow<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull;
```
#### Type parameters

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).TFlow'></a>

`TFlow`

The type of flow to execute \(must implement [IFlow&lt;TRequest,TResponse&gt;](IFlow_TRequest,TResponse_.md 'FlowT\.Contracts\.IFlow\<TRequest,TResponse\>')\)\.

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).TRequest'></a>

`TRequest`

The type of request the flow processes\.

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).TResponse'></a>

`TResponse`

The type of response the flow produces\.
#### Parameters

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).app'></a>

`app` [Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.iendpointroutebuilder 'Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder')

The endpoint route builder\.

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).pattern'></a>

`pattern` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The route pattern \(e\.g\., "/users", "/orders/\{id\}"\)\.

<a name='FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).httpMethod'></a>

`httpMethod` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The HTTP method \(e\.g\., "GET", "POST", "PUT", "DELETE"\)\.

#### Returns
[Microsoft\.AspNetCore\.Builder\.RouteHandlerBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.routehandlerbuilder 'Microsoft\.AspNetCore\.Builder\.RouteHandlerBuilder')  
A [Microsoft\.AspNetCore\.Builder\.RouteHandlerBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.routehandlerbuilder 'Microsoft\.AspNetCore\.Builder\.RouteHandlerBuilder') for further endpoint configuration \(authorization, tags, etc\.\)\.

### Remarks

The method automatically applies metadata from [FlowEndpointInfoAttribute](FlowEndpointInfoAttribute.md 'FlowT\.Attributes\.FlowEndpointInfoAttribute') if present on the flow class.
Request parameters are bound from route values, query string, or request body based on ASP.NET Core conventions.

<strong>Automatic Response Dispatch:</strong>

`MapFlow` inspects [TResponse](FlowEndpointExtensions.MapFlow.ZBG6HKQSPLZH3C384SRU1AT8.md#FlowT.Extensions.FlowEndpointExtensions.MapFlow_TFlow,TRequest,TResponse_(thisMicrosoft.AspNetCore.Routing.IEndpointRouteBuilder,string,string).TResponse 'FlowT\.Extensions\.FlowEndpointExtensions\.MapFlow\<TFlow,TRequest,TResponse\>\(this Microsoft\.AspNetCore\.Routing\.IEndpointRouteBuilder, string, string\)\.TResponse') at registration time and selects the correct
            serialization path — no manual `Results.Stream()` or `Results.File()` is needed:
            
- <strong>Standard response</strong> — serialized as JSON via `Results.Json()`
- <strong>
    <see cref="T:FlowT.Contracts.IStreamableResponse"/>
  </strong> (e.g. [PagedStreamResponse&lt;T&gt;](PagedStreamResponse_T_.md 'FlowT\.Abstractions\.PagedStreamResponse\<T\>')) — `Results.Stream()` with chunked transfer encoding and progressive delivery
- <strong>
    <see cref="T:FlowT.Abstractions.FileStreamResponse"/>
  </strong> — `Results.File()` with Content-Disposition, ETag, Last-Modified, and range request support

<strong>Example:</strong>

```csharp
// ✅ Standard — JSON serialized automatically
app.MapFlow<GetOrderFlow, GetOrderRequest, GetOrderResponse>("/orders/{id}", "GET");

// ✅ Streaming — Results.Stream() invoked automatically (no WriteToStreamAsync boilerplate)
app.MapFlow<StreamProductsFlow, StreamProductsRequest, PagedStreamResponse<ProductDto>>("/products/stream", "GET");

// ✅ File download — Results.File() invoked automatically (no manual stream wiring)
app.MapFlow<ExportUsersFlow, ExportUsersRequest, FileStreamResponse>("/users/export", "GET");
```

All three forms are compatible with route groups and support further chaining:

```csharp
var api = app.MapGroup("/api").RequireAuthorization();
api.MapFlow<StreamProductsFlow, StreamProductsRequest, PagedStreamResponse<ProductDto>>("/products/stream", "GET")
   .WithSummary("Stream paginated products")
   .WithTags("Products");
```