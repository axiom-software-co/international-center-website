using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    private static readonly ConcurrentDictionary<string, Error> _errorPool = new();
    
    public static Error None { get; } = new(string.Empty, string.Empty, ErrorType.None);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Validation(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Validation);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error NotFound(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.NotFound);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Conflict(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Conflict);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Unauthorized(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Unauthorized);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Forbidden(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Forbidden);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Failure(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Failure);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Unexpected(string code, string message) => 
        GetOrCreatePooled(code, message, ErrorType.Unexpected);
    
    private static Error GetOrCreatePooled(string code, string message, ErrorType type)
    {
        var key = $"{code}|{message}|{type}";
        return _errorPool.GetOrAdd(key, _ => new Error(code, message, type));
    }
    
    public static void ClearPool() => _errorPool.Clear();
    
    public static int PoolSize => _errorPool.Count;
}