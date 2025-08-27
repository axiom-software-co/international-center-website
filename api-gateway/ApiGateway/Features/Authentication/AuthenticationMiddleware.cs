using Microsoft.AspNetCore.Http;

namespace ApiGateway.Features.Authentication;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IAuthenticationStrategy> _strategies;
    
    public AuthenticationMiddleware(RequestDelegate next, IEnumerable<IAuthenticationStrategy> strategies)
    {
        _next = next;
        _strategies = strategies;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}