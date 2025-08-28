using Microsoft.Extensions.Configuration;
using SharedPlatform.Features.Configuration.Abstractions;
using SharedPlatform.Features.Configuration.Options;
using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.Configuration.Services;

public class OptionsProvider : IOptionsProvider
{
    private readonly IConfiguration _configuration;

    public OptionsProvider() : this(new ConfigurationService().GetConfiguration())
    {
    }

    public OptionsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Result<T> GetOptions<T>() where T : class, new()
    {
        try
        {
            var options = new T();
            var sectionName = typeof(T).Name;
            var section = _configuration.GetSection(sectionName);
            
            section.Bind(options);

            if (options is BaseOptions baseOptions)
            {
                var validationResult = baseOptions.Validate();
                if (validationResult.IsFailure)
                    return Result<T>.Failure(validationResult.Error);
            }

            return Result<T>.Success(options);
        }
        catch (Exception ex)
        {
            return Error.Unexpected("OptionsProvider.GetOptions", $"Failed to get options for type {typeof(T).Name}: {ex.Message}");
        }
    }

    public Result ValidateOptions<T>(T options)
    {
        try
        {
            if (options == null)
                return Error.Validation("OptionsProvider.ValidateOptions", "Options cannot be null");

            if (options is BaseOptions baseOptions)
                return baseOptions.Validate();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Error.Unexpected("OptionsProvider.ValidateOptions", $"Failed to validate options: {ex.Message}");
        }
    }
}