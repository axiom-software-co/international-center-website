using System.Security.Claims;

namespace SharedPlatform.Features.Authentication.Models;

public class UserPrincipal
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
    public bool IsAuthenticated { get; init; }
    public string? AuthenticationType { get; init; }
    
    public static UserPrincipal FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        return new UserPrincipal
        {
            Id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            FirstName = principal.FindFirst(ClaimTypes.GivenName)?.Value,
            LastName = principal.FindFirst(ClaimTypes.Surname)?.Value,
            Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            Claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value),
            IsAuthenticated = principal.Identity?.IsAuthenticated ?? false,
            AuthenticationType = principal.Identity?.AuthenticationType
        };
    }
    
    public ClaimsPrincipal ToClaimsPrincipal()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Id),
            new(ClaimTypes.Name, UserName),
            new(ClaimTypes.Email, Email)
        };
        
        if (!string.IsNullOrEmpty(FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, FirstName));
            
        if (!string.IsNullOrEmpty(LastName))
            claims.Add(new Claim(ClaimTypes.Surname, LastName));
        
        claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(Claims.Select(kvp => new Claim(kvp.Key, kvp.Value)));
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType));
    }
}