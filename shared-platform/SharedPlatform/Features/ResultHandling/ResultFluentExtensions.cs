using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public static class ResultFluentExtensions
{
    /// <summary>
    /// Ensures that a condition is met, otherwise returns a failure
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;
            
        return predicate(result.Value) 
            ? result 
            : Result<T>.Failure(error);
    }
    
    /// <summary>
    /// Ensures that a condition is met, otherwise returns a failure with computed error
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Error> errorFactory)
    {
        if (result.IsFailure)
            return result;
            
        return predicate(result.Value) 
            ? result 
            : Result<T>.Failure(errorFactory(result.Value));
    }
    
    /// <summary>
    /// Maps an error to a different error
    /// </summary>
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> errorMapper)
    {
        return result.IsSuccess 
            ? result 
            : Result<T>.Failure(errorMapper(result.Error));
    }
    
    /// <summary>
    /// Combines multiple results into a single result containing all values
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(this Result<T1> result1, Result<T2> result2)
    {
        if (result1.IsFailure)
            return Result<(T1, T2)>.Failure(result1.Error);
            
        if (result2.IsFailure)
            return Result<(T1, T2)>.Failure(result2.Error);
            
        return Result<(T1, T2)>.Success((result1.Value, result2.Value));
    }
    
    /// <summary>
    /// Combines multiple results into a single result containing all values
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(this Result<T1> result1, Result<T2> result2, Result<T3> result3)
    {
        if (result1.IsFailure)
            return Result<(T1, T2, T3)>.Failure(result1.Error);
            
        if (result2.IsFailure)
            return Result<(T1, T2, T3)>.Failure(result2.Error);
            
        if (result3.IsFailure)
            return Result<(T1, T2, T3)>.Failure(result3.Error);
            
        return Result<(T1, T2, T3)>.Success((result1.Value, result2.Value, result3.Value));
    }
    
    /// <summary>
    /// Filters successful results based on a predicate
    /// </summary>
    public static Result<T> Where<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        return result.IsSuccess && predicate(result.Value) 
            ? result 
            : result.IsSuccess 
                ? Result<T>.Failure(error)
                : result;
    }
    
    /// <summary>
    /// Executes an async action if the result is successful
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> asyncAction)
    {
        if (result.IsSuccess)
            await asyncAction(result.Value).ConfigureAwait(false);
        return result;
    }
    
    /// <summary>
    /// Transforms the value of a successful result asynchronously
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<T, TResult>(this Result<T> result, Func<T, Task<TResult>> asyncMapper)
    {
        return result.IsSuccess 
            ? Result<TResult>.Success(await asyncMapper(result.Value).ConfigureAwait(false))
            : Result<TResult>.Failure(result.Error);
    }
    
    /// <summary>
    /// Chains async operations that return Result<T>
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Result<T> result, Func<T, Task<Result<TResult>>> asyncBinder)
    {
        return result.IsSuccess 
            ? await asyncBinder(result.Value).ConfigureAwait(false)
            : Result<TResult>.Failure(result.Error);
    }
    
    /// <summary>
    /// Chains async operations that return Result<T> from Task<Result<T>>
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Task<Result<T>> resultTask, Func<T, Task<Result<TResult>>> asyncBinder)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess 
            ? await asyncBinder(result.Value).ConfigureAwait(false)
            : Result<TResult>.Failure(result.Error);
    }
    
    /// <summary>
    /// Adds audit context and observability to result operations
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> WithAuditContext<T>(this Result<T> result, string? correlationId = null, string? operationName = null)
    {
        using var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("result.isSuccess", result.IsSuccess);
            activity.SetTag("result.errorCode", result.IsFailure ? result.Error.Code : null);
            activity.SetTag("result.errorType", result.IsFailure ? result.Error.Type.ToString() : null);
            activity.SetTag("audit.correlationId", correlationId ?? activity.Id);
            activity.SetTag("audit.operation", operationName);
        }
        return result;
    }
    
    /// <summary>
    /// Adds audit context to non-generic result operations
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result WithAuditContext(this Result result, string? correlationId = null, string? operationName = null)
    {
        using var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("result.isSuccess", result.IsSuccess);
            activity.SetTag("result.errorCode", result.IsFailure ? result.Error.Code : null);
            activity.SetTag("result.errorType", result.IsFailure ? result.Error.Type.ToString() : null);
            activity.SetTag("audit.correlationId", correlationId ?? activity.Id);
            activity.SetTag("audit.operation", operationName);
        }
        return result;
    }
}