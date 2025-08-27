using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using ApiGateway.Shared.Models;

namespace ApiGateway.Shared.Extensions;

public static class PolicyExtensions
{
    public static IServiceCollection AddGatewayPolicies(this IServiceCollection services)
    {
        throw new NotImplementedException();
    }
    
    public static AuthorizationPolicyBuilder RequireGatewayRole(this AuthorizationPolicyBuilder builder, string role)
    {
        return builder.RequireRole(role);
    }
    
    public static AuthorizationPolicyBuilder RequireGatewayPermission(this AuthorizationPolicyBuilder builder, string permission)
    {
        throw new NotImplementedException();
    }
    
    public static void ApplyPolicyDefinition(this AuthorizationPolicyBuilder builder, PolicyDefinition definition)
    {
        throw new NotImplementedException();
    }
}