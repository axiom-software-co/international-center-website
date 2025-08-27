using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public class EntraIdStrategy : IAuthenticationStrategy
{
    public string Name => "EntraID";
    
    public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task ChallengeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Bearer";
        return Task.CompletedTask;
    }
    
    public bool CanHandle(HttpContext context)
    {
        throw new NotImplementedException();
    }
}