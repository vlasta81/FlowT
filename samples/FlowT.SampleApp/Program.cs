using FlowT.Extensions;
using FlowT.Plugins;
using FlowT.SampleApp;
using Microsoft.FeatureManagement;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<FlowInterruptExceptionHandler>();

// Feature flags (reads from appsettings.json "FeatureManagement" section)
builder.Services.AddFeatureManagement();

// Register FlowT plugins — available in every flow via context.Plugin<T>()
builder.Services.AddFlowPlugin<IAuditPlugin, AuditPlugin>();
builder.Services.AddFlowPlugin<ITenantPlugin, TenantPlugin>();
builder.Services.AddFlowPlugin<IIdempotencyPlugin, IdempotencyPlugin>();
builder.Services.AddFlowPlugin<IPerformancePlugin, PerformancePlugin>();
builder.Services.AddFlowPlugin<IFlowScopePlugin, FlowScopePlugin>();
builder.Services.AddFlowPlugin<IFeatureFlagPlugin, FeatureFlagPlugin>();

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
        "High-performance singleton handlers",
        "AuditPlugin — structured audit trail per flow execution",
        "TenantPlugin — automatic tenant resolution from claim/header/route",
        "IdempotencyPlugin — X-Idempotency-Key header support",
        "PerformancePlugin — measure named code sections with Stopwatch",
        "FlowScopePlugin — dedicated DI scope for non-HTTP flows",
        "FeatureFlagPlugin — per-flow feature flag evaluation with caching"
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
            "POST /api/orders - Create order with validation, idempotency, tenant, audit and feature-flag support"
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
