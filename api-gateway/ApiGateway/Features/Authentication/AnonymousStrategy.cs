using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public class AnonymousStrategy : IAuthenticationStrategy
{
    public string Name => "Anonymous";
    
    public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult<ClaimsPrincipal?>(principal);
    }
    
    public Task ChallengeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }
    
    public bool CanHandle(HttpContext context)
    {
        return true; // Can always handle anonymous requests
    }
}