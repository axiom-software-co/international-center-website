using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public class JwtStrategy : IAuthenticationStrategy
{
    public string Name => "JWT";
    
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
        return context.Request.Headers.ContainsKey("Authorization") &&
               context.Request.Headers["Authorization"].ToString().StartsWith("Bearer ");
    }
}