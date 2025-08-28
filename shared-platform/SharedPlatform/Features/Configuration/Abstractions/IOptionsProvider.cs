using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.Configuration.Abstractions;

public interface IOptionsProvider
{
    Result<T> GetOptions<T>() where T : class, new();
    Result ValidateOptions<T>(T options);
}