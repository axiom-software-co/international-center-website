namespace SharedPlatform.Shared.Exceptions;

public abstract class PlatformException : Exception
{
    protected PlatformException(string message) : base(message)
    {
    }
    
    protected PlatformException(string message, Exception innerException) : base(message, innerException)
    {
    }
}