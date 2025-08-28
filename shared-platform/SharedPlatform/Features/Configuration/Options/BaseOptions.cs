using SharedPlatform.Features.ResultHandling;

namespace SharedPlatform.Features.Configuration.Options;

public abstract class BaseOptions
{
    public virtual Result Validate()
    {
        return Result.Success();
    }
}