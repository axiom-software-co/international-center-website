using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public class OperationResult
{
    private readonly ImmutableArray<Error> _validationErrors;
    private readonly bool _isSuccess;
    private readonly Error _error;
    
    // Static cache for common empty validation error arrays
    private static readonly ImmutableArray<Error> EmptyValidationErrors = ImmutableArray<Error>.Empty;
    
    protected OperationResult(bool isSuccess, Error error, IEnumerable<Error>? validationErrors = null)
    {
        _isSuccess = isSuccess;
        _error = error;
        _validationErrors = validationErrors?.ToImmutableArray() ?? EmptyValidationErrors;
        
        // Observability tracking for operation results
        using var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("operation.isSuccess", isSuccess);
            activity.SetTag("operation.errorType", error.Type.ToString());
            activity.SetTag("operation.validationErrorCount", _validationErrors.Length);
            if (!isSuccess && !string.IsNullOrEmpty(error.Code))
            {
                activity.SetTag("operation.errorCode", error.Code);
            }
        }
    }
    
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public Error Error => _error;
    
    public IReadOnlyList<Error> ValidationErrors => _validationErrors;
    public bool HasValidationErrors => _validationErrors.Length > 0;
    public int ValidationErrorCount => _validationErrors.Length;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationResult Success() => new(true, Error.None);
    
    public static OperationResult WithValidationErrors(IEnumerable<Error> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);
        var errorArray = validationErrors.ToImmutableArray();
        
        if (errorArray.IsEmpty)
        {
            throw new ArgumentException("At least one validation error is required", nameof(validationErrors));
        }
        
        var primaryError = Error.Validation("VALIDATION", $"{errorArray.Length} validation error(s) occurred");
        return new OperationResult(false, primaryError, errorArray);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationResult Failure(Error error) => new(false, error);
    
    public static implicit operator OperationResult(Error error) => Failure(error);
}

public class OperationResult<T> : OperationResult
{
    private readonly T? _value;
    
    private OperationResult(T value, bool isSuccess, Error error, IEnumerable<Error>? validationErrors = null) 
        : base(isSuccess, error, validationErrors)
    {
        _value = value;
    }
    
    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access value of a failed operation result.");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationResult<T> Success(T value) => new(value, true, Error.None);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new OperationResult<T> WithValidationErrors(IEnumerable<Error> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);
        var errorArray = validationErrors.ToImmutableArray();
        
        if (errorArray.IsEmpty)
        {
            throw new ArgumentException("At least one validation error is required", nameof(validationErrors));
        }
        
        var primaryError = Error.Validation("VALIDATION", $"{errorArray.Length} validation error(s) occurred");
        return new OperationResult<T>(default!, false, primaryError, errorArray);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new OperationResult<T> Failure(Error error) => new(default!, false, error);
    
    public static implicit operator OperationResult<T>(T value) => Success(value);
    public static implicit operator OperationResult<T>(Error error) => Failure(error);
    
    /// <summary>
    /// Gets detailed operation metadata for audit and observability
    /// </summary>
    public OperationMetadata GetMetadata() => new(
        IsSuccess,
        Error.Code,
        Error.Type,
        ValidationErrorCount,
        ValidationErrors.Select(e => e.Code).ToImmutableArray(),
        typeof(T).Name
    );
}

/// <summary>
/// Immutable operation metadata for audit and observability
/// </summary>
public sealed record OperationMetadata(
    bool IsSuccess,
    string ErrorCode,
    ErrorType ErrorType,
    int ValidationErrorCount,
    ImmutableArray<string> ValidationErrorCodes,
    string ValueTypeName
) {
    public bool HasComplexFailure => !IsSuccess && (ErrorType != ErrorType.Validation || ValidationErrorCount > 1);
    public bool IsValidationOnly => !IsSuccess && ErrorType == ErrorType.Validation;
    public bool HasCriticalError => ErrorType is ErrorType.Unexpected or ErrorType.Unauthorized or ErrorType.Forbidden;
}