using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Features.Authorization;

public static class AuthorizationPolicies
{
    public const string Public = "Public";
    public const string Admin = "Admin";
    public const string RequireAuthentication = "RequireAuthentication";
    public const string AdminAccess = "AdminAccess";
    public const string PublicAccess = "PublicAccess";
    
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(Public, policy => policy.RequireAssertion(_ => true));
        options.AddPolicy(Admin, policy => policy.RequireRole("Admin"));
        options.AddPolicy(RequireAuthentication, policy => policy.RequireAuthenticatedUser());
        options.AddPolicy(AdminAccess, policy => policy.RequireRole("Admin", "SuperAdmin"));
        options.AddPolicy(PublicAccess, policy => policy.RequireAssertion(_ => true));
    }
}