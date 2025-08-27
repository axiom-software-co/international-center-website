namespace SharedPlatform.Features.Authentication.Models;

public class AuthenticationContext
{
    public string? Token { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? AuthenticationScheme { get; init; }
    public Dictionary<string, object> Properties { get; init; } = new();
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    
    public static AuthenticationContext FromToken(string token, string? scheme = null)
    {
        return new AuthenticationContext
        {
            Token = token,
            AuthenticationScheme = scheme
        };
    }
    
    public static AuthenticationContext FromCredentials(string username, string password, string? scheme = null)
    {
        return new AuthenticationContext
        {
            Username = username,
            Password = password,
            AuthenticationScheme = scheme
        };
    }
}