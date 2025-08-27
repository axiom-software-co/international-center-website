using SharedPlatform.Features.Authentication.Models;

namespace SharedPlatform.Features.Authentication.Abstractions;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string token, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> AuthenticateAsync(AuthenticationContext context, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
}