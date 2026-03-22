using FlowT.Extensions;
using FlowT.SampleApp;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<FlowInterruptExceptionHandler>();

// Register FlowT modules (auto-discovers all [FlowModule] classes)
builder.Services.AddFlowModules(typeof(Program).Assembly);

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    message = "FlowT Sample API",
    version = "1.1.0",
    features = new[]
    {
        "Modular architecture with IFlowModule",
        "FlowInterrupt for type-safe error handling",
        "Named keys in FlowContext",
        "Roslyn analyzers for compile-time safety",
        "High-performance singleton handlers"
    },
    endpoints = new
    {
        users = new[] 
        { 
            "GET /api/users - List all users",
            "GET /api/users/{id} - Get user by ID",
            "POST /api/users - Create new user",
            "PUT /api/users/{id} - Update user",
            "DELETE /api/users/{id} - Delete user"
        },
        products = new[]
        {
            "GET /api/products - List all products",
            "GET /api/products/{id} - Get product by ID",
            "POST /api/products - Create new product"
        },
        orders = new[]
        {
            "POST /api/orders - Create order with validation"
        }
    },
    documentation = new[]
    {
        "Scalar UI: /scalar (modern OpenAPI documentation)",
        "OpenAPI spec: /openapi/v1.json",
        "GitHub: https://github.com/vlasta81/FlowT",
        "Docs: https://github.com/vlasta81/FlowT/tree/master/docs"
    }
}))
.WithName("Root")
.WithSummary("API information and available endpoints");

// Map all module endpoints (auto-discovery)
app.MapFlowModules();

app.Run();
