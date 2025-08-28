using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public sealed class Email : BaseValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));
        
    private const int MaxEmailLength = 254; // RFC 5321 standard
    
    public string Value { get; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email From(string value)
    {
        return TryFrom(value, out var email) 
            ? email 
            : throw new ArgumentException(GetValidationErrorMessage(value), nameof(value));
    }
    
    public static bool TryFrom(string? value, [NotNullWhen(true)] out Email? email)
    {
        email = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        if (value.Length > MaxEmailLength)
            return false;
            
        if (!IsValidEmailFormat(value))
            return false;
            
        email = new Email(value.ToLowerInvariant());
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidEmailFormat(string value)
    {
        try
        {
            return EmailRegex.IsMatch(value);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    
    private static string GetValidationErrorMessage(string? value)
    {
        return value switch
        {
            null => "Email cannot be null.",
            "" => "Email cannot be empty.",
            _ when value.Length > MaxEmailLength => $"Email cannot exceed {MaxEmailLength} characters.",
            _ => $"'{value}' is not a valid email address."
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(Email email) => email.Value;
    
    public string Domain => Value.Substring(Value.IndexOf('@') + 1);
    
    public string LocalPart => Value.Substring(0, Value.IndexOf('@'));
}