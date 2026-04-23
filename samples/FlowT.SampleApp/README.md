# FlowT Sample Application

Modern ASP.NET Core 10 sample application demonstrating **FlowT** orchestration library features.

---

## 🎯 Features Demonstrated

### ✅ Core FlowT Features
- **Modular Architecture** - `IFlowModule` with `[FlowModule]` attribute
- **Flow Definitions** - `FlowDefinition<TRequest, TResponse>` with `[FlowDefinition]`
- **Specifications** - Type-safe validation with `FlowInterrupt<object?>` (specs run before policies/handler)
- **`.OnInterrupt()`** - Maps specification `Fail()`/`Stop()` results to typed responses
- **Policies** - Cross-cutting concerns (logging, caching, validation)
- **Handlers** - Business logic with safe scoped service resolution
- **FlowContext** - Named keys, service resolution, events, timing
- **Plugin system** - `context.Plugin<T>()` for per-flow cached plugin access (e.g. feature flags)
- **Streaming responses** - `PagedStreamResponse<T>` with `IAsyncEnumerable<T>` and `MapFlow` endpoint extension

### ✅ Best Practices
- **Singleton Handlers** - Safe usage with `context.Service<T>()` for scoped dependencies
- **Thread Safety** - No mutable state in handlers
- **FlowInterrupt** - Type-safe specification results without exception overhead
- **`.OnInterrupt()` — required for `Fail()`** — without it, failure returns `null` silently
- **Named Keys** - Multiple values of same type in FlowContext
- **Cancellation** - Proper `CancellationToken` propagation
- **Roslyn Analyzers** - Compile-time safety checks

---

## 🚀 Quick Start

### Run the Application

```bash
cd samples/FlowT.SampleApp
dotnet run
```

**Scalar UI:** https://localhost:7000/scalar (or http://localhost:5000/scalar)  
**OpenAPI spec:** https://localhost:7000/openapi/v1.json

---

## 📚 Modules

### 1. User Module (`Features/Users`)

CRUD operations for user management.

**Endpoints:**
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

**Demonstrates:**
- ✅ FlowInterrupt for validation (email format, uniqueness)
- ✅ Multiple specifications per flow
- ✅ Named keys for storing validated data
- ✅ Reusing data from specifications in handlers
- ✅ `.OnInterrupt()` with **throw pattern** — `CreateUserResponse` is a clean DTO, so `FlowInterruptException` is thrown and converted to the correct HTTP status code by `FlowInterruptExceptionHandler`

**Example Request:**
```bash
# Create user
curl -X POST https://localhost:7000/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "name": "John Doe",
    "phoneNumber": "+1234567890"
  }'
```

**Key Files:**
- `UserContracts.cs` - Request/Response DTOs
- `UserSpecifications.cs` - Validation with FlowInterrupt
- `UserHandlers.cs` - Business logic
- `UserFlows.cs` - Pipeline configuration
- `UserModule.cs` - Module registration and endpoints

---

### 2. Product Module (`Features/Products`)

Simple product catalog management with streaming support.

**Endpoints:**
- `GET /api/products` - List all products (pre-seeded data)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product
- `GET /api/products/stream` - Stream paginated products (`PagedStreamResponse<T>` demo)

**Demonstrates:**
- ✅ Simple flow with minimal policies
- ✅ Basic validation
- ✅ In-memory repository with seed data
- ✅ **Streaming with `PagedStreamResponse<T>`** — metadata (total count, page info) sent first, items streamed progressively via `IAsyncEnumerable<T>`
- ✅ **`MapFlow` extension** — auto-detects `IStreamableResponse` and calls `Results.Stream()` internally

**Example Request:**
```bash
# List products
curl https://localhost:7000/api/products

# Create product
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Monitor",
    "description": "27-inch 4K monitor",
    "price": 349.99,
    "stockQuantity": 30
  }'

# Stream products (paginated)
curl "https://localhost:7000/api/products/stream?page=0&pageSize=10"
```

**Key File:**
- `ProductModule.cs` - Complete module in single file (simple example, includes streaming)

---

### 3. Order Module (`Features/Orders`)

Complex order creation with multi-step validation.

**Endpoints:**
- `POST /api/orders` - Create order

**Demonstrates:**
- ✅ **Multi-step validation pipeline**
  1. Feature flag check (new workflow enabled?)
  2. User exists and is active
  3. Products exist with sufficient stock
  4. Business rule: Orders > $1000 require approval
- ✅ **Plugin system** (`context.Plugin<T>()`) — `FeatureFlagOrderSpecification` calls `IFeatureFlagPlugin.IsEnabledAsync()` with per-flow result caching
- ✅ **FlowInterrupt at multiple stages**
  - 503 if new order workflow feature flag is disabled
  - 404 if user/product not found
  - 403 if user is inactive
  - 409 if insufficient stock
  - 400 for invalid quantities
- ✅ **`.OnInterrupt()` with throw pattern** — `FlowInterruptException` carries the original status code; `FlowInterruptExceptionHandler` returns the correct HTTP response
- ✅ **Named keys for passing data between pipeline stages**
- ✅ **Background events** (`PublishInBackground`)
- ✅ **Stock updates** after order creation

**Example Request:**
```bash
# Create order (requires user and products to exist)
curl -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "<user-guid>",
    "items": [
      { "productId": "<product-guid>", "quantity": 2 },
      { "productId": "<product-guid>", "quantity": 1 }
    ]
  }'
```

**Validation Flow:**
```
Request
  ↓
FeatureFlagOrderSpecification (NewOrderWorkflow enabled? → 503 if not)
  ↓ (pass)
ValidateUserForOrderSpecification (user exists? active?)
  ↓ (pass) → store user in context with key "order:user"
ValidateOrderItemsSpecification (products exist? stock ok?)
  ↓ (pass) → store validated items in context
CheckOrderValueSpecification (total > $1000?)
  ↓ (pass) → store total + approval flag
LoggingPolicy (log start/end with timing)
  ↓
ValidationPolicy (store validation metadata)
  ↓
CreateOrderHandler (create order, update stock, publish event)
  ↓
Response
```

**Key File:**
- `OrderModule.cs` - Complex validation pipeline example

---

## 🎨 Policies (Cross-Cutting Concerns)

### LoggingPolicy
- Logs request start/end
- Measures execution time with `context.StartTimer()` and `using`
- Logs errors with FlowId correlation

### ValidationPolicy
- Stores validation metadata using named keys
- Demonstrates `context.Set(value, key: "...")`

### CachingPolicy
- Creates request-scoped cache using `GetOrAdd` with named keys
- Demonstrates lazy initialization pattern

---

## 🔍 FlowContext Usage Examples

### Named Keys
```csharp
// Store multiple values of same type
context.Set(adminUser, key: "admin");
context.Set(guestUser, key: "guest");

// Retrieve with key
if (context.TryGet<User>(out var admin, key: "admin"))
{
    // Use admin user
}
```

### Service Resolution (Scoped)
```csharp
// ✅ Safe in singleton handlers
var userRepo = context.Service<IUserRepository>();
var user = await userRepo.GetByIdAsync(id, context.CancellationToken);
```

### Events
```csharp
// Fire and forget (background)
context.PublishInBackground(new OrderCreatedEvent(orderId, userId, total));

// Wait for completion (synchronous)
await context.PublishAsync(new CriticalEvent(data));
```

### Timing
```csharp
// Timer stops automatically when using block exits
using (context.StartTimer("database-query"))
{
    data = await db.Query();
}
// elapsed time is stored in context under the given key
```

### Plugins
```csharp
// Access a plugin — result is cached for the lifetime of this flow
var ff = context.Plugin<IFeatureFlagPlugin>();
var enabled = await ff.IsEnabledAsync("NewOrderWorkflow", context.CancellationToken);

if (!enabled)
{
    return FlowInterrupt<object?>.Fail(
        "Feature is currently disabled.",
        StatusCodes.Status503ServiceUnavailable
    );
}
```

### Flow Identification
```csharp
var flowId = context.GetFlowIdString(); // For logging/correlation
var started = context.StartedAt; // UTC timestamp

_logger.LogInformation("[{FlowId}] Processing request", flowId);
```

---

## 🛡️ Compile-Time Safety (Roslyn Analyzers)

The sample app demonstrates **safe patterns** that pass FlowT analyzers:

### ✅ GOOD: Scoped Services via Context
```csharp
public class CreateUserHandler : IFlowHandler<CreateUserRequest, CreateUserResponse>
{
    // ✅ Only singleton dependencies in constructor
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<CreateUserResponse> HandleAsync(CreateUserRequest request, FlowContext context)
    {
        // ✅ Resolve scoped service per-request
        var userRepo = context.Service<IUserRepository>();
        // ...
    }
}
```

### ❌ BAD: Captive Scoped Dependency
```csharp
// ❌ FlowT003: Build fails! Analyzer prevents captive dependency
public class BadHandler : IFlowHandler<Request, Response>
{
    private readonly IUserRepository _repo; // ❌ ERROR!

    public BadHandler(IUserRepository repo) // ❌ Scoped in singleton!
    {
        _repo = repo;
    }
}
```

---

## 📂 Project Structure

```
FlowT.SampleApp/
├── Program.cs                      # Application entry point
├── FlowInterruptException.cs       # Exception thrown from OnInterrupt mapper
├── FlowInterruptExceptionHandler.cs # IExceptionHandler → maps to correct HTTP status
├── Domain/                         # Domain entities
│   ├── User.cs
│   ├── Product.cs
│   └── Order.cs
├── Infrastructure/                 # Repositories (in-memory)
│   ├── IUserRepository.cs
│   └── IProductRepository.cs
├── Policies/                       # Cross-cutting concerns
│   ├── LoggingPolicy.cs
│   ├── ValidationPolicy.cs
│   └── CachingPolicy.cs
├── Features/                       # Feature modules
│   ├── Users/                      # User module (detailed example)
│   │   ├── UserContracts.cs        # Request/Response DTOs
│   │   ├── UserSpecifications.cs   # Validations with FlowInterrupt
│   │   ├── UserHandlers.cs         # Business logic
│   │   ├── UserFlows.cs            # Pipeline configurations
│   │   └── UserModule.cs           # Module + endpoints
│   ├── Products/                   # Product module (simple example)
│   │   └── ProductModule.cs        # All-in-one file
│   └── Orders/                     # Order module (complex example)
│       └── OrderModule.cs          # Complex validation pipeline
└── README.md                       # This file
```

---

## 🔄 Typical Request Flow

```
1. HTTP Request → Minimal API endpoint
2. Inject FlowDefinition (e.g., CreateUserFlow)
3. Call flow.ExecuteAsync(request, services, ct)
4. FlowT creates FlowContext automatically
5. Pipeline executes:
   a. Specifications (Check<T>) - can interrupt with FlowInterrupt
   b. Policies (Use<T>) - cross-cutting concerns
   c. Handler (Handle<T>) - business logic
6. Response returned → JSON serialization → HTTP Response
```

---

## 🧪 Testing Patterns

### Unit Testing Handlers
```csharp
[Fact]
public async Task CreateUser_ShouldCreateUser()
{
    // Arrange
    var services = new ServiceCollection()
        .AddSingleton<IUserRepository, InMemoryUserRepository>()
        .AddLogging()
        .BuildServiceProvider();

    var context = new FlowContext
    {
        Services = services,
        CancellationToken = CancellationToken.None
    };

    var handler = new CreateUserHandler(services.GetRequiredService<ILogger<CreateUserHandler>>());
    var request = new CreateUserRequest("test@example.com", "Test User");

    // Act
    var response = await handler.HandleAsync(request, context);

    // Assert
    Assert.NotEqual(Guid.Empty, response.Id);
    Assert.Equal("test@example.com", response.Email);
}
```

### Integration Testing Flows
```csharp
[Fact]
public async Task CreateUserFlow_WithInvalidEmail_ShouldThrowFlowInterruptException()
{
    // Arrange
    var services = BuildTestServices();
    var flow = services.GetRequiredService<CreateUserFlow>();
    var request = new CreateUserRequest("invalid-email", "Test");

    // Act & Assert
    // OnInterrupt throws FlowInterruptException when specs Fail()
    // FlowInterruptExceptionHandler converts it to the correct HTTP status code
    var ex = await Assert.ThrowsAsync<FlowInterruptException>(
        () => flow.ExecuteAsync(request, services, CancellationToken.None).AsTask());

    Assert.Equal(400, ex.StatusCode);
    Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
}
```

---

## 📖 Learn More

### Documentation
- **[FlowT Main Documentation](../../README.md)** - Overview and quick start
- **[FlowContext Guide](../../docs/FLOWCONTEXT.md)** - Complete API reference
- **[Best Practices](../../docs/BEST_PRACTICES.md)** - Thread-safety and performance
- **[Analyzer Rules](../../src/FlowT.Analyzers/README.md)** - Compile-time safety

### Benchmarks
- **[Performance Results](../../benchmarks/FlowT.Benchmarks/README.md)** - FlowT vs competitors
- **[Extreme Benchmarks](../../benchmarks/FlowT.Benchmarks/EXTREME_README.md)** - Stress tests

---

## 💡 Key Takeaways

1. ✅ **Use `[FlowModule]` and `[FlowDefinition]` attributes** for auto-discovery
2. ✅ **Resolve scoped services with `context.Service<T>()`** (never in constructor)
3. ✅ **Specs return `FlowInterrupt<object?>`** — always add `.OnInterrupt()` when specs can `Fail()`
4. ✅ **Named keys for storing multiple values of same type** (`context.Set(value, key: "...")`)
5. ✅ **Specifications can access repositories** (they run before handler)
6. ✅ **Pass data between pipeline stages via context** (avoids duplicate queries)
7. ✅ **Singleton handlers are thread-safe** when following FlowT patterns
8. ✅ **Analyzers catch mistakes at compile-time** (captive dependencies, thread-safety)
9. ✅ **Use `context.Plugin<T>()`** for per-flow cached plugin access (feature flags, rate limiting, etc.)
10. ✅ **Use `PagedStreamResponse<T>` + `MapFlow`** for streaming endpoints with progressive `IAsyncEnumerable<T>` delivery

---

## 🤝 Contributing

Found an issue or have a suggestion? Open an issue at:
https://github.com/vlasta81/FlowT/issues

---

**Built with ❤️ using FlowT v1.2.0**
