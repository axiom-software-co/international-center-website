namespace SharedPlatform.Shared.Exceptions;

public class AuthenticationException : PlatformException
{
    public AuthenticationException(string message) : base(message)
    {
    }
    
    public AuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}