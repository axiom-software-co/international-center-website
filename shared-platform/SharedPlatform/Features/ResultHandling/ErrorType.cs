namespace SharedPlatform.Features.ResultHandling;

public enum ErrorType
{
    None,
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Unexpected
}