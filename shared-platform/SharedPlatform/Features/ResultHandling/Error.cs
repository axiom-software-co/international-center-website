namespace SharedPlatform.Features.ResultHandling;

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static Error None => new(string.Empty, string.Empty, ErrorType.None);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}