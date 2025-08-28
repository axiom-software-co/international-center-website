using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SharedPlatform.Features.ResultHandling;

public class PagedResult<T> : IEnumerable<T>
{
    private readonly IReadOnlyCollection<T> _items;
    private readonly bool _isSuccess;
    private readonly Error _error;
    
    private PagedResult(IEnumerable<T> items, bool isSuccess, Error error, int totalCount, int pageNumber, int pageSize)
    {
        // Materialize the enumerable for performance and predictability
        _items = items as IReadOnlyCollection<T> ?? items.ToList().AsReadOnly();
        _isSuccess = isSuccess;
        _error = error;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        
        // Observability tracking for paging operations
        using var activity = Activity.Current;
        if (activity != null && isSuccess)
        {
            activity.SetTag("paging.totalCount", totalCount);
            activity.SetTag("paging.pageNumber", pageNumber);
            activity.SetTag("paging.pageSize", pageSize);
            activity.SetTag("paging.itemCount", _items.Count);
            activity.SetTag("paging.hasNextPage", HasNextPage);
            activity.SetTag("paging.hasPreviousPage", HasPreviousPage);
        }
    }
    
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    public Error Error => _error;
    
    public IReadOnlyCollection<T> Items => IsSuccess 
        ? _items 
        : throw new InvalidOperationException("Cannot access items of a failed paged result.");
    
    public int ItemCount => IsSuccess ? _items.Count : 0;
        
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PagedResult<T> Success(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        
        return new PagedResult<T>(items, true, Error.None, totalCount, pageNumber, pageSize);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PagedResult<T> Empty()
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), true, Error.None, 0, 1, 0);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PagedResult<T> Failure(Error error)
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), false, error, 0, 0, 0);
    }
    
    public static implicit operator PagedResult<T>(Error error) => Failure(error);
    
    /// <summary>
    /// Gets paging metadata for audit and monitoring
    /// </summary>
    public PagingMetadata GetMetadata() => new(
        TotalCount, 
        PageNumber, 
        PageSize, 
        TotalPages, 
        ItemCount,
        HasNextPage, 
        HasPreviousPage
    );
    
    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Immutable paging metadata for audit and observability
/// </summary>
public sealed record PagingMetadata(
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    int ItemCount,
    bool HasNextPage,
    bool HasPreviousPage
) {
    public double LoadFactor => TotalCount == 0 ? 0 : (double)ItemCount / PageSize;
    public int RemainingItems => Math.Max(0, TotalCount - (PageNumber * PageSize));
    public bool IsComplete => !HasNextPage;
    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => !HasNextPage;
}