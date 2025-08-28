using Xunit;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using System.Security.Claims;
using SharedPlatform.Features.Authentication.Abstractions;
using SharedPlatform.Features.Authentication.Models;
using SharedPlatform.Features.Authentication.Services;
using SharedPlatform.Features.ResultHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace SharedPlatform.Features.Authentication;

public class AuthenticationTests
{
    // IAuthenticationService Tests
    
    [Fact]
    public async Task AuthenticationService_AuthenticateWithToken_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var authService = new JwtAuthenticationService();
        var principal = CreateTestClaimsPrincipal("test-user");
        var validToken = await tokenService.GenerateAccessTokenAsync(principal);
        
        // Act
        var result = await authService.AuthenticateAsync(validToken);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.Principal);
        Assert.True(result.Principal.Identity?.IsAuthenticated);
    }
    
    [Fact]
    public async Task AuthenticationService_AuthenticateWithInvalidToken_ShouldReturnFailureResult()
    {
        // Arrange
        var authService = new JwtAuthenticationService();
        var invalidToken = "invalid-token";
        
        // Act
        var result = await authService.AuthenticateAsync(invalidToken);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.AccessToken);
        Assert.Null(result.Principal);
    }
    
    [Fact]
    public async Task AuthenticationService_AuthenticateWithContext_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var authService = new JwtAuthenticationService();
        var context = AuthenticationContext.FromCredentials("testuser", "password123");
        
        // Act
        var result = await authService.AuthenticateAsync(context);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.Principal);
    }
    
    [Fact]
    public async Task AuthenticationService_ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var authService = new JwtAuthenticationService();
        var principal = CreateTestClaimsPrincipal("test-user");
        var validToken = await tokenService.GenerateAccessTokenAsync(principal);
        
        // Act
        var isValid = await authService.ValidateTokenAsync(validToken);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public async Task AuthenticationService_ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var authService = new JwtAuthenticationService();
        var invalidToken = "invalid-token";
        
        // Act
        var isValid = await authService.ValidateTokenAsync(invalidToken);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public async Task AuthenticationService_SignOut_ShouldCompleteSuccessfully()
    {
        // Arrange
        var authService = new JwtAuthenticationService();
        
        // Act & Assert - Should not throw
        await authService.SignOutAsync();
    }
    
    // ITokenService Tests
    
    [Fact]
    public async Task TokenService_GenerateAccessToken_ShouldReturnValidToken()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var principal = CreateTestClaimsPrincipal("test-user");
        
        // Act
        var token = await tokenService.GenerateAccessTokenAsync(principal);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }
    
    [Fact]
    public async Task TokenService_GenerateRefreshToken_ShouldReturnValidToken()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var userId = "test-user-id";
        
        // Act
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(userId);
        
        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);
    }
    
    [Fact]
    public async Task TokenService_ValidateToken_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var principal = CreateTestClaimsPrincipal("test-user");
        var token = await tokenService.GenerateAccessTokenAsync(principal);
        
        // Act
        var result = await tokenService.ValidateTokenAsync(token);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.Principal);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public async Task TokenService_ValidateToken_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var invalidToken = "invalid-token";
        
        // Act
        var result = await tokenService.ValidateTokenAsync(invalidToken);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.Principal);
    }
    
    [Fact]
    public async Task TokenService_GetPrincipalFromToken_ShouldReturnCorrectPrincipal()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var originalPrincipal = CreateTestClaimsPrincipal("test-user");
        var token = await tokenService.GenerateAccessTokenAsync(originalPrincipal);
        
        // Act
        var retrievedPrincipal = await tokenService.GetPrincipalFromTokenAsync(token);
        
        // Assert
        Assert.NotNull(retrievedPrincipal);
        Assert.Equal("test-user", retrievedPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
    
    [Fact]
    public async Task TokenService_RevokeToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var tokenService = new JwtTokenService();
        var token = "test-token";
        
        // Act & Assert - Should not throw
        await tokenService.RevokeTokenAsync(token);
    }
    
    // IUserContext Tests
    
    [Fact]
    public void UserContext_WithAuthenticatedUser_ShouldReturnCorrectProperties()
    {
        // Arrange
        var userContext = new UserContextService();
        var principal = CreateTestClaimsPrincipal("test-user");
        userContext.SetCurrentUser(principal);
        
        // Act & Assert
        Assert.Equal("test-user", userContext.UserId);
        Assert.Equal("Test User", userContext.UserName);
        Assert.Equal("test@example.com", userContext.Email);
        Assert.True(userContext.IsAuthenticated);
        Assert.Contains("User", userContext.Roles);
    }
    
    [Fact]
    public void UserContext_WithoutAuthenticatedUser_ShouldReturnDefaultProperties()
    {
        // Arrange
        var userContext = new UserContextService();
        
        // Act & Assert
        Assert.Null(userContext.UserId);
        Assert.Null(userContext.UserName);
        Assert.Null(userContext.Email);
        Assert.False(userContext.IsAuthenticated);
        Assert.Empty(userContext.Roles);
    }
    
    [Fact]
    public void UserContext_IsInRole_ShouldReturnCorrectResult()
    {
        // Arrange
        var userContext = new UserContextService();
        var principal = CreateTestClaimsPrincipal("test-user");
        userContext.SetCurrentUser(principal);
        
        // Act & Assert
        Assert.True(userContext.IsInRole("User"));
        Assert.False(userContext.IsInRole("Admin"));
    }
    
    [Fact]
    public void UserContext_HasClaim_ShouldReturnCorrectResult()
    {
        // Arrange
        var userContext = new UserContextService();
        var principal = CreateTestClaimsPrincipal("test-user");
        userContext.SetCurrentUser(principal);
        
        // Act & Assert
        Assert.True(userContext.HasClaim(ClaimTypes.NameIdentifier, "test-user"));
        Assert.False(userContext.HasClaim(ClaimTypes.NameIdentifier, "other-user"));
    }
    
    [Fact]
    public void UserContext_GetUserPrincipal_ShouldReturnCorrectUserPrincipal()
    {
        // Arrange
        var userContext = new UserContextService();
        var principal = CreateTestClaimsPrincipal("test-user");
        userContext.SetCurrentUser(principal);
        
        // Act
        var userPrincipal = userContext.GetUserPrincipal();
        
        // Assert
        Assert.NotNull(userPrincipal);
        Assert.Equal("test-user", userPrincipal.Id);
        Assert.Equal("Test User", userPrincipal.UserName);
        Assert.Equal("test@example.com", userPrincipal.Email);
        Assert.Contains("User", userPrincipal.Roles);
    }
    
    // Anonymous Authentication Service Tests
    
    [Fact]
    public async Task AnonymousAuthService_Authenticate_ShouldReturnAnonymousResult()
    {
        // Arrange
        var anonymousService = new AnonymousAuthService();
        var context = AuthenticationContext.FromToken("anonymous");
        
        // Act
        var result = await anonymousService.AuthenticateAsync(context);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Principal);
        Assert.False(result.Principal.Identity?.IsAuthenticated);
    }
    
    // Model Tests
    
    [Fact]
    public void AuthenticationResult_Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var token = "test-token";
        var principal = CreateTestClaimsPrincipal("test-user");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        
        // Act
        var result = AuthenticationResult.Success(token, "refresh-token", principal, expiresAt);
        
        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(token, result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(principal, result.Principal);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public void AuthenticationResult_Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Authentication failed";
        
        // Act
        var result = AuthenticationResult.Failure(errorMessage);
        
        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Null(result.AccessToken);
        Assert.Null(result.RefreshToken);
        Assert.Null(result.Principal);
    }
    
    [Fact]
    public void TokenValidationResult_Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var principal = CreateTestClaimsPrincipal("test-user");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        
        // Act
        var result = TokenValidationResult.Success(principal, expiresAt, "Bearer");
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(principal, result.Principal);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Null(result.ErrorMessage);
    }
    
    [Fact]
    public void TokenValidationResult_Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Token validation failed";
        
        // Act
        var result = TokenValidationResult.Failure(errorMessage);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Null(result.Principal);
    }
    
    [Fact]
    public void UserPrincipal_FromClaimsPrincipal_ShouldConvertCorrectly()
    {
        // Arrange
        var claimsPrincipal = CreateTestClaimsPrincipal("test-user");
        
        // Act
        var userPrincipal = UserPrincipal.FromClaimsPrincipal(claimsPrincipal);
        
        // Assert
        Assert.Equal("test-user", userPrincipal.Id);
        Assert.Equal("Test User", userPrincipal.UserName);
        Assert.Equal("test@example.com", userPrincipal.Email);
        Assert.Contains("User", userPrincipal.Roles);
        Assert.True(userPrincipal.IsAuthenticated);
    }
    
    [Fact]
    public void UserPrincipal_ToClaimsPrincipal_ShouldConvertCorrectly()
    {
        // Arrange
        var userPrincipal = new UserPrincipal
        {
            Id = "test-user",
            UserName = "Test User",
            Email = "test@example.com",
            Roles = new List<string> { "User" },
            IsAuthenticated = true,
            AuthenticationType = "Bearer"
        };
        
        // Act
        var claimsPrincipal = userPrincipal.ToClaimsPrincipal();
        
        // Assert
        Assert.NotNull(claimsPrincipal);
        Assert.Equal("test-user", claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("Test User", claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("test@example.com", claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.True(claimsPrincipal.IsInRole("User"));
    }
    
    [Fact]
    public void AuthenticationContext_FromToken_ShouldCreateCorrectContext()
    {
        // Arrange
        var token = "test-token";
        var scheme = "Bearer";
        
        // Act
        var context = AuthenticationContext.FromToken(token, scheme);
        
        // Assert
        Assert.Equal(token, context.Token);
        Assert.Equal(scheme, context.AuthenticationScheme);
        Assert.Null(context.Username);
        Assert.Null(context.Password);
    }
    
    [Fact]
    public void AuthenticationContext_FromCredentials_ShouldCreateCorrectContext()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var scheme = "Basic";
        
        // Act
        var context = AuthenticationContext.FromCredentials(username, password, scheme);
        
        // Assert
        Assert.Equal(username, context.Username);
        Assert.Equal(password, context.Password);
        Assert.Equal(scheme, context.AuthenticationScheme);
        Assert.Null(context.Token);
    }
    
    // Integration Tests
    
    [Fact]
    public void Authentication_ServiceRegistration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        
        // Act
        services.AddAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var authService = serviceProvider.GetService<IAuthenticationService>();
        var tokenService = serviceProvider.GetService<ITokenService>();
        var userContext = serviceProvider.GetService<IUserContext>();
        
        Assert.NotNull(authService);
        Assert.NotNull(tokenService);
        Assert.NotNull(userContext);
    }
    
    [Fact]
    public async Task Authentication_EndToEndFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddAuthentication(configuration);
        var serviceProvider = services.BuildServiceProvider();
        
        var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
        var tokenService = serviceProvider.GetRequiredService<ITokenService>();
        var userContext = serviceProvider.GetRequiredService<IUserContext>();
        
        // Act
        var context = AuthenticationContext.FromCredentials("testuser", "password123");
        var authResult = await authService.AuthenticateAsync(context);
        
        // Assert
        Assert.True(authResult.IsSuccessful);
        Assert.NotNull(authResult.AccessToken);
        
        // Validate token
        var validationResult = await tokenService.ValidateTokenAsync(authResult.AccessToken);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(validationResult.Principal);
    }
    
    // Property-Based Tests
    
    [Property]
    public Property AuthenticationContext_TokenCreation_IsConsistent()
    {
        return Prop.ForAll<string>(token =>
        {
            if (string.IsNullOrWhiteSpace(token)) return true;
            
            var context1 = AuthenticationContext.FromToken(token);
            var context2 = AuthenticationContext.FromToken(token);
            
            return context1.Token == context2.Token;
        });
    }
    
    [Property]
    public Property UserPrincipal_ClaimsPrincipalRoundTrip_PreservesData()
    {
        return Prop.ForAll<string, string, string>(
            (id, userName, email) =>
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(userName)) return true;
                
                var original = new UserPrincipal
                {
                    Id = id,
                    UserName = userName,
                    Email = email ?? string.Empty,
                    IsAuthenticated = true
                };
                
                var claimsPrincipal = original.ToClaimsPrincipal();
                var converted = UserPrincipal.FromClaimsPrincipal(claimsPrincipal);
                
                return original.Id == converted.Id &&
                       original.UserName == converted.UserName &&
                       original.Email == converted.Email;
            });
    }
    
    // Helper Methods
    
    private static ClaimsPrincipal CreateTestClaimsPrincipal(string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User")
        };
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }
}

// Extension method class for DI registration
public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAuthenticationService, JwtAuthenticationService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IUserContext, UserContextService>();
        services.AddSingleton<AnonymousAuthService>();
        
        return services;
    }
}