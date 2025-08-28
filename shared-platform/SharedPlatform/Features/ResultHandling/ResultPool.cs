using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

/// <summary>
/// High-performance object pool for frequently used Result instances
/// Reduces GC pressure and improves performance for common result patterns
/// </summary>
internal static class ResultPool
{
    private static readonly Result _successResult = Result.Success();
    private static readonly ConcurrentDictionary<string, Result> _cachedFailures = new();
    private static readonly ConcurrentDictionary<string, object> _cachedSuccessResults = new();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result GetSuccess() => _successResult;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result GetFailure(Error error)
    {
        var key = $"{error.Code}|{error.Type}";
        return _cachedFailures.GetOrAdd(key, _ => Result.Failure(error));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> GetSuccess<T>(T value) where T : notnull
    {
        // For common value types, use pooling
        if (typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(string))
        {
            var key = $"{typeof(T).Name}|{value}";
            if (_cachedSuccessResults.TryGetValue(key, out var cached))
            {
                return (Result<T>)cached;
            }
            
            var result = Result<T>.Success(value);
            _cachedSuccessResults.TryAdd(key, result);
            return result;
        }
        
        return Result<T>.Success(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> GetFailure<T>(Error error)
    {
        return Result<T>.Failure(error);
    }
    
    /// <summary>
    /// Clears the result cache - should only be used in testing scenarios
    /// </summary>
    public static void ClearCache()
    {
        _cachedFailures.Clear();
        _cachedSuccessResults.Clear();
    }
    
    /// <summary>
    /// Gets cache statistics for performance monitoring
    /// </summary>
    public static (int FailuresCached, int SuccessCached) GetCacheStats()
    {
        return (_cachedFailures.Count, _cachedSuccessResults.Count);
    }
}