namespace ApiGateway.Features.ErrorHandling;

public class ErrorConfiguration
{
    public bool IncludeStackTrace { get; set; }
    public bool IncludeDetails { get; set; }
    public string[] AllowedDetailEnvironments { get; set; } = { "Development", "Testing" };
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public Dictionary<Type, int> ExceptionStatusCodes { get; set; } = new()
    {
        { typeof(ArgumentException), 400 },
        { typeof(UnauthorizedAccessException), 401 },
        { typeof(InvalidOperationException), 400 },
        { typeof(NotImplementedException), 501 },
        { typeof(TimeoutException), 408 }
    };
}
