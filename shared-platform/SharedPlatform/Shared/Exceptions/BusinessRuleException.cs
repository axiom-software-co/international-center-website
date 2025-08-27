namespace SharedPlatform.Shared.Exceptions;

public class BusinessRuleException : DomainException
{
    public string RuleName { get; }
    
    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
    }
    
    public BusinessRuleException(string ruleName, string message, Exception innerException) : base(message, innerException)
    {
        RuleName = ruleName;
    }
}