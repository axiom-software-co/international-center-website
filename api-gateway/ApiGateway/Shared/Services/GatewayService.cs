using Microsoft.AspNetCore.Http;
using ApiGateway.Shared.Abstractions;

namespace ApiGateway.Shared.Services;

public class GatewayService : IGatewayService
{
    public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task ProcessRequestAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<string> GetGatewayVersionAsync()
    {
        throw new NotImplementedException();
    }
}