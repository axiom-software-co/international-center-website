using Microsoft.AspNetCore.Http;
using ApiGateway.Shared.Models;

namespace ApiGateway.Shared.Services;

public class GatewayContextService
{
    public GatewayContext CreateContext(HttpContext httpContext)
    {
        throw new NotImplementedException();
    }
    
    public Task EnrichContextAsync(GatewayContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<GatewayRequest> CreateRequestAsync(HttpContext httpContext)
    {
        throw new NotImplementedException();
    }
    
    public Task<GatewayResponse> CreateResponseAsync(HttpContext httpContext)
    {
        throw new NotImplementedException();
    }
}