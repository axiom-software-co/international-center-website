using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public readonly struct Result : IEquatable<Result>
{
    private readonly bool _isSuccess;
    private readonly Error _error;
    
    private Result(bool isSuccess, Error error)
    {
        _isSuccess = isSuccess;
        _error = error;
    }
    
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public Error Error => _error;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(true, Error.None);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(Error error) => new(false, error);
    
    public static implicit operator Result(Error error) => Failure(error);
    
    public override bool Equals(object? obj) => obj is Result other && Equals(other);
    
    public bool Equals(Result other) => 
        _isSuccess == other._isSuccess && 
        EqualityComparer<Error>.Default.Equals(_error, other._error);
    
    public override int GetHashCode() => HashCode.Combine(_isSuccess, _error);
    
    public static bool operator ==(Result left, Result right) => left.Equals(right);
    
    public static bool operator !=(Result left, Result right) => !(left == right);
}