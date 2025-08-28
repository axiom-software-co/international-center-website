using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public static class ResultExtensions
{
    /// <summary>
    /// Executes one of two functions based on the result state
    /// </summary>
    public static TResult Match<T, TResult>(this Result<T> result, 
        Func<T, TResult> onSuccess, 
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }
    
    /// <summary>
    /// Executes one of two actions based on the result state
    /// </summary>
    public static void Match<T>(this Result<T> result, 
        Action<T> onSuccess, 
        Action<Error> onFailure)
    {
        if (result.IsSuccess)
            onSuccess(result.Value);
        else
            onFailure(result.Error);
    }
    
    /// <summary>
    /// Transforms the value of a successful result
    /// </summary>
    public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> mapper)
    {
        return result.IsSuccess 
            ? Result<TResult>.Success(mapper(result.Value))
            : Result<TResult>.Failure(result.Error);
    }
    
    /// <summary>
    /// Chains operations that return Result<T>
    /// </summary>
    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> binder)
    {
        return result.IsSuccess 
            ? binder(result.Value)
            : Result<TResult>.Failure(result.Error);
    }
    
    /// <summary>
    /// Chains operations that return Result
    /// </summary>
    public static Result Bind<T>(this Result<T> result, Func<T, Result> binder)
    {
        return result.IsSuccess 
            ? binder(result.Value)
            : Result.Failure(result.Error);
    }
    
    /// <summary>
    /// Executes an action if the result is successful, returns the original result
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }
    
    /// <summary>
    /// Returns the value if successful, otherwise returns the default value
    /// </summary>
    public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue = default!)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }
    
    /// <summary>
    /// Returns the value if successful, otherwise computes and returns a default value
    /// </summary>
    public static T GetValueOrDefault<T>(this Result<T> result, Func<Error, T> defaultValueFactory)
    {
        return result.IsSuccess ? result.Value : defaultValueFactory(result.Error);
    }
    
    /// <summary>
    /// Measures execution time for performance monitoring
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> WithPerformanceMonitoring<T>(this Result<T> result, string operationName, TimeSpan executionTime)
    {
        using var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("performance.operation", operationName);
            activity.SetTag("performance.executionTimeMs", executionTime.TotalMilliseconds);
            activity.SetTag("performance.isSlowQuery", executionTime.TotalMilliseconds > 1000);
        }
        return result;
    }
    
    /// <summary>
    /// Creates a performance-monitored result with execution timing
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> TimedResult<T>(Func<Result<T>> operation, string operationName)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = operation();
        stopwatch.Stop();
        return result.WithPerformanceMonitoring(operationName, stopwatch.Elapsed);
    }
    
    /// <summary>
    /// Creates a performance-monitored async result with execution timing
    /// </summary>
    public static async Task<Result<T>> TimedResultAsync<T>(Func<Task<Result<T>>> asyncOperation, string operationName)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await asyncOperation().ConfigureAwait(false);
        stopwatch.Stop();
        return result.WithPerformanceMonitoring(operationName, stopwatch.Elapsed);
    }
}