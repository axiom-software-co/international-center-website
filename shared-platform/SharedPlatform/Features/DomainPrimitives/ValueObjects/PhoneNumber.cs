using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SharedPlatform.Features.DomainPrimitives.ValueObjects;

public sealed class PhoneNumber : BaseValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(50));
        
    private static readonly char[] CharsToRemove = [' ', '-', '(', ')', '.'];
    private const int MinPhoneLength = 7; // +1234567
    private const int MaxPhoneLength = 16; // +123456789012345
    
    public string Value { get; }
    
    private PhoneNumber(string value)
    {
        Value = value;
    }
    
    public static PhoneNumber From(string value)
    {
        return TryFrom(value, out var phoneNumber) 
            ? phoneNumber 
            : throw new ArgumentException(GetValidationErrorMessage(value), nameof(value));
    }
    
    public static bool TryFrom(string? value, [NotNullWhen(true)] out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;
            
        var cleanValue = CleanPhoneNumber(value);
        
        if (cleanValue.Length < MinPhoneLength || cleanValue.Length > MaxPhoneLength)
            return false;
            
        if (!IsValidPhoneFormat(cleanValue))
            return false;
            
        phoneNumber = new PhoneNumber(cleanValue);
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanPhoneNumber(string value)
    {
        Span<char> chars = stackalloc char[value.Length];
        var writeIndex = 0;
        
        foreach (var c in value)
        {
            if (!CharsToRemove.Contains(c))
            {
                chars[writeIndex++] = c;
            }
        }
        
        return new string(chars[..writeIndex]);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPhoneFormat(string value)
    {
        try
        {
            return PhoneRegex.IsMatch(value);
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
            null => "Phone number cannot be null.",
            "" => "Phone number cannot be empty.",
            _ when value.Length < MinPhoneLength => "Phone number is too short.",
            _ when value.Length > MaxPhoneLength => "Phone number is too long.",
            _ => $"'{value}' is not a valid phone number. Use international format (+1234567890)."
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    
    public string CountryCode => Value.StartsWith('+') && Value.Length > 1 
        ? Value.Substring(1, Math.Min(3, Value.Length - 1)) 
        : string.Empty;
        
    public string DisplayFormat => Value.Length switch
    {
        >= 11 => $"{Value[..3]} ({Value[3..6]}) {Value[6..9]}-{Value[9..]}",
        >= 10 => $"{Value[..3]} {Value[3..6]}-{Value[6..]}",
        _ => Value
    };
}