using Aspire.Hosting;
using AspireHost.Shared.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

// Configure environment-specific settings
builder.ConfigureEnvironment();

// Add infrastructure resources (PostgreSQL, Redis, RabbitMQ, etc.)
builder.AddInfrastructureResources();

// Add International Center services
builder.AddInternationalCenterServices();

// Add health orchestration
builder.AddHealthOrchestration();

// Add observability
builder.AddObservability();

builder.Build().Run();