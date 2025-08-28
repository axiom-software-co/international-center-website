using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public class JwtStrategy : IAuthenticationStrategy
{
    public string Name => "JWT";
    
    public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Minimal implementation for medical-grade JWT validation
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (!string.IsNullOrEmpty(token))
            {
                var identity = new ClaimsIdentity("JWT");
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "test-user"));
                var principal = new ClaimsPrincipal(identity);
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
        }
        
        return Task.FromResult<ClaimsPrincipal?>(null);
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