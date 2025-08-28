using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.Authentication.Configuration;

public class JwtOptions : BaseOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public TimeSpan AccessTokenExpiration { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan RefreshTokenExpiration { get; set; } = TimeSpan.FromDays(7);
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableTokenBlacklist { get; set; } = true;

    public override Result Validate()
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(SecretKey))
            validationErrors.Add("SecretKey is required");
        else if (SecretKey.Length < 32)
            validationErrors.Add("SecretKey must be at least 32 characters for security");

        if (string.IsNullOrWhiteSpace(Issuer))
            validationErrors.Add("Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            validationErrors.Add("Audience is required");

        if (AccessTokenExpiration <= TimeSpan.Zero)
            validationErrors.Add("AccessTokenExpiration must be greater than zero");

        if (RefreshTokenExpiration <= TimeSpan.Zero)
            validationErrors.Add("RefreshTokenExpiration must be greater than zero");

        if (AccessTokenExpiration >= RefreshTokenExpiration)
            validationErrors.Add("RefreshTokenExpiration must be greater than AccessTokenExpiration");

        if (CacheExpiration <= TimeSpan.Zero)
            validationErrors.Add("CacheExpiration must be greater than zero");

        return validationErrors.Count == 0
            ? Result.Success()
            : Error.Validation("JwtOptions.Validation", string.Join("; ", validationErrors));
    }
}