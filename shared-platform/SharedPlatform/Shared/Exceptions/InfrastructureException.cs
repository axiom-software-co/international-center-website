namespace SharedPlatform.Shared.Exceptions;

public class InfrastructureException : PlatformException
{
    public InfrastructureException(string message) : base(message)
    {
    }
    
    public InfrastructureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}