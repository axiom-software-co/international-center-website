namespace SharedPlatform.Shared.Exceptions;

public class DomainException : PlatformException
{
    public DomainException(string message) : base(message)
    {
    }
    
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}