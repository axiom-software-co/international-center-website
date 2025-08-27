using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.Cors;

public class CorsService : ICorsService
{
    public Task ApplyCorsAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
    
    public bool IsOriginAllowed(string origin)
    {
        throw new NotImplementedException();
    }
}