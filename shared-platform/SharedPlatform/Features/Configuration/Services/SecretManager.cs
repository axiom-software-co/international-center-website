using SharedPlatform.Features.ResultHandling;
using System.Collections.Concurrent;

namespace SharedPlatform.Features.Configuration.Services;

public class SecretManager
{
    private readonly ConcurrentDictionary<string, string> _secrets;

    public SecretManager()
    {
        _secrets = new ConcurrentDictionary<string, string>();
    }

    public async Task<Result<string>> GetSecretAsync(string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        await Task.Delay(1);

        if (_secrets.TryGetValue(secretName, out var secret))
        {
            return Result<string>.Success(secret);
        }

        var envSecret = Environment.GetEnvironmentVariable($"SECRET_{secretName.ToUpperInvariant()}");
        if (!string.IsNullOrWhiteSpace(envSecret))
        {
            _secrets.TryAdd(secretName, envSecret);
            return Result<string>.Success(envSecret);
        }

        return Error.NotFound("SecretManager.GetSecret", $"Secret '{secretName}' not found");
    }

    public async Task<Result> SetSecretAsync(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretValue);

        await Task.Delay(1);

        try
        {
            _secrets.AddOrUpdate(secretName, secretValue, (key, oldValue) => secretValue);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Error.Unexpected("SecretManager.SetSecret", $"Failed to set secret '{secretName}': {ex.Message}");
        }
    }

    public async Task<Result> DeleteSecretAsync(string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        await Task.Delay(1);

        try
        {
            if (_secrets.TryRemove(secretName, out _))
            {
                return Result.Success();
            }

            return Error.NotFound("SecretManager.DeleteSecret", $"Secret '{secretName}' not found");
        }
        catch (Exception ex)
        {
            return Error.Unexpected("SecretManager.DeleteSecret", $"Failed to delete secret '{secretName}': {ex.Message}");
        }
    }
}