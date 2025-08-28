using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public readonly struct Result<T> : IEquatable<Result<T>>
{
    private readonly T? _value;
    private readonly bool _isSuccess;
    private readonly Error _error;
    
    private Result(T value, bool isSuccess, Error error)
    {
        _value = value;
        _isSuccess = isSuccess;
        _error = error;
    }
    
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public Error Error => _error;
    
    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access value of a failed result.");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success(T value) => new(value, true, Error.None);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure(Error error) => new(default!, false, error);
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
    
    public static implicit operator Result(Result<T> result) => 
        result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);
    
    public bool Equals(Result<T> other) => 
        _isSuccess == other._isSuccess && 
        EqualityComparer<Error>.Default.Equals(_error, other._error) &&
        EqualityComparer<T?>.Default.Equals(_value, other._value);
    
    public override int GetHashCode() => HashCode.Combine(_isSuccess, _error, _value);
    
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);
    
    public static bool operator !=(Result<T> left, Result<T> right) => !(left == right);
}