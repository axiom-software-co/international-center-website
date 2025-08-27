using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Features.Authentication;

public static class AuthenticationPolicies
{
    public const string Anonymous = "Anonymous";
    public const string Authenticated = "Authenticated";
    public const string AdminOnly = "AdminOnly";
    public const string PublicApiAccess = "PublicApiAccess";
    
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(Anonymous, policy => policy.RequireAssertion(_ => true));
        options.AddPolicy(Authenticated, policy => policy.RequireAuthenticatedUser());
        options.AddPolicy(AdminOnly, policy => policy.RequireRole("Admin"));
        options.AddPolicy(PublicApiAccess, policy => policy.RequireAssertion(_ => true));
    }
}