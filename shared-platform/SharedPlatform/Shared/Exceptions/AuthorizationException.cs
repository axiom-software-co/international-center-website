namespace SharedPlatform.Shared.Exceptions;

public class AuthorizationException : PlatformException
{
    public AuthorizationException(string message) : base(message)
    {
    }
    
    public AuthorizationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}