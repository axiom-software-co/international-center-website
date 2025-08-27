using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ApiGateway.Shared.Abstractions;

namespace ApiGateway.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseGatewayPipeline(this WebApplication app)
    {
        throw new NotImplementedException();
    }
    
    public static WebApplication UseGatewayMiddleware(this WebApplication app)
    {
        var middlewareProvider = app.Services.GetService<IMiddlewareProvider>();
        return middlewareProvider?.ConfigureMiddleware(app) as WebApplication ?? app;
    }
    
    public static WebApplication UseGatewayHealthChecks(this WebApplication app)
    {
        throw new NotImplementedException();
    }
    
    public static WebApplication UseGatewayErrorHandling(this WebApplication app)
    {
        throw new NotImplementedException();
    }
}