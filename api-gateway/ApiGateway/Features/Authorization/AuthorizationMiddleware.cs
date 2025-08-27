using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.Authorization;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IAuthorizationStrategy> _strategies;
    
    public AuthorizationMiddleware(RequestDelegate next, IEnumerable<IAuthorizationStrategy> strategies)
    {
        _next = next;
        _strategies = strategies;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}