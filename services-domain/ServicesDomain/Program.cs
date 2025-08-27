using ServicesDomain.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddServicesDomainServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseServicesDomainPipeline();

app.Run();
