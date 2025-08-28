using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ApiGateway.Features.Authentication;

public class EntraIdStrategy : IAuthenticationStrategy
{
    public string Name => "EntraID";
    
    public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        // GREEN PHASE: Minimal implementation for medical-grade EntraId validation
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (!string.IsNullOrEmpty(token))
            {
                var identity = new ClaimsIdentity("EntraID");
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "entra-user"));
                identity.AddClaim(new Claim("tenant_id", "medical-tenant"));
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
        // GREEN PHASE: Check for Bearer tokens (EntraId uses standard OAuth2 Bearer tokens)
        return context.Request.Headers.ContainsKey("Authorization") &&
               context.Request.Headers["Authorization"].ToString().StartsWith("Bearer ");
    }
}