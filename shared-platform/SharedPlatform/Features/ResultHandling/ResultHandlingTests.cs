using Xunit;
using FsCheck;
using FsCheck.Xunit;

namespace SharedPlatform.Features.ResultHandling;

public class ResultHandlingTests
{
    // Result Base Class Tests
    
    [Fact]
    public void Result_Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();
        
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }
    
    [Fact]
    public void Result_Failure_ShouldCreateFailedResult()
    {
        var error = Error.Validation("TEST_001", "Test validation error");
        var result = Result.Failure(error);
        
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
    
    [Fact]
    public void Result_ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        var error = Error.NotFound("TEST_002", "Resource not found");
        Result result = error;
        
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }
    
    // Result<T> Generic Tests
    
    [Fact]
    public void ResultT_Success_ShouldCreateSuccessfulResultWithValue()
    {
        var value = "test-value";
        var result = Result<string>.Success(value);
        
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
        Assert.Equal(Error.None, result.Error);
    }
    
    [Fact]
    public void ResultT_Failure_ShouldCreateFailedResult()
    {
        var error = Error.Conflict("TEST_003", "Conflict detected");
        var result = Result<string>.Failure(error);
        
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
    
    [Fact]
    public void ResultT_Value_OnFailure_ShouldThrowInvalidOperationException()
    {
        var error = Error.Failure("TEST_004", "Operation failed");
        var result = Result<string>.Failure(error);
        
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access value of a failed result", exception.Message);
    }
    
    [Fact]
    public void ResultT_ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
    {
        const int value = 42;
        Result<int> result = value;
        
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }
    
    [Fact]
    public void ResultT_ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        var error = Error.Unauthorized("TEST_005", "Access denied");
        Result<string> result = error;
        
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }
    
    // Error Tests
    
    [Fact]
    public void Error_None_ShouldHaveCorrectProperties()
    {
        var error = Error.None;
        
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Equal(ErrorType.None, error.Type);
    }
    
    [Theory]
    [InlineData("VAL_001", "Validation failed", ErrorType.Validation)]
    [InlineData("NOT_FOUND_001", "Resource not found", ErrorType.NotFound)]
    [InlineData("CONFLICT_001", "Resource already exists", ErrorType.Conflict)]
    [InlineData("FAILURE_001", "Operation failed", ErrorType.Failure)]
    [InlineData("UNEXPECTED_001", "Unexpected error occurred", ErrorType.Unexpected)]
    public void Error_FactoryMethods_ShouldCreateCorrectErrorTypes(string code, string message, ErrorType expectedType)
    {
        var error = expectedType switch
        {
            ErrorType.Validation => Error.Validation(code, message),
            ErrorType.NotFound => Error.NotFound(code, message),
            ErrorType.Conflict => Error.Conflict(code, message),
            ErrorType.Failure => Error.Failure(code, message),
            ErrorType.Unexpected => Error.Unexpected(code, message),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedType))
        };
        
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(expectedType, error.Type);
    }
    
    // OperationResult Tests (Complex Operations)
    
    [Fact]
    public void OperationResult_Success_ShouldCreateSuccessfulResult()
    {
        var result = OperationResult.Success();
        
        Assert.True(result.IsSuccess);
        Assert.False(result.HasValidationErrors);
        Assert.Empty(result.ValidationErrors);
    }
    
    [Fact]
    public void OperationResult_WithValidationErrors_ShouldCreateFailedResult()
    {
        var validationErrors = new[]
        {
            Error.Validation("FIELD_001", "First field is required"),
            Error.Validation("FIELD_002", "Second field is invalid")
        };
        
        var result = OperationResult.WithValidationErrors(validationErrors);
        
        Assert.False(result.IsSuccess);
        Assert.True(result.HasValidationErrors);
        Assert.Equal(2, result.ValidationErrors.Count);
        Assert.Contains(validationErrors[0], result.ValidationErrors);
        Assert.Contains(validationErrors[1], result.ValidationErrors);
    }
    
    [Fact]
    public void OperationResult_WithSingleError_ShouldCreateFailedResult()
    {
        var error = Error.Failure("OP_001", "Operation failed");
        var result = OperationResult.Failure(error);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }
    
    [Fact]
    public void OperationResultT_Success_ShouldCreateSuccessfulResultWithValue()
    {
        var value = new { Id = 1, Name = "Test" };
        var result = OperationResult<object>.Success(value);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.False(result.HasValidationErrors);
    }
    
    // PagedResult Tests
    
    [Fact]
    public void PagedResult_Success_ShouldCreatePagedResultWithCorrectMetadata()
    {
        var items = new[] { "item1", "item2", "item3" };
        var result = PagedResult<string>.Success(items, totalCount: 10, pageNumber: 2, pageSize: 3);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(items, result.Items);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(4, result.TotalPages); // Math.Ceiling(10.0 / 3.0)
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }
    
    [Fact]
    public void PagedResult_FirstPage_ShouldNotHavePreviousPage()
    {
        var items = new[] { "item1", "item2" };
        var result = PagedResult<string>.Success(items, totalCount: 5, pageNumber: 1, pageSize: 2);
        
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
    
    [Fact]
    public void PagedResult_LastPage_ShouldNotHaveNextPage()
    {
        var items = new[] { "item5" };
        var result = PagedResult<string>.Success(items, totalCount: 5, pageNumber: 3, pageSize: 2);
        
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }
    
    [Fact]
    public void PagedResult_Empty_ShouldReturnEmptyPage()
    {
        var result = PagedResult<string>.Empty();
        
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(0, result.PageSize);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
    
    // ResultExtensions Tests
    
    [Fact]
    public void ResultExtensions_Match_ShouldExecuteCorrectAction()
    {
        var successResult = Result<int>.Success(42);
        var failureResult = Result<int>.Failure(Error.Failure("TEST", "Test error"));
        
        var successOutput = successResult.Match(
            value => $"Success: {value}",
            error => $"Error: {error.Message}"
        );
        
        var failureOutput = failureResult.Match(
            value => $"Success: {value}",
            error => $"Error: {error.Message}"
        );
        
        Assert.Equal("Success: 42", successOutput);
        Assert.Equal("Error: Test error", failureOutput);
    }
    
    [Fact]
    public void ResultExtensions_Bind_ShouldChainOperations()
    {
        var result = Result<int>.Success(5)
            .Bind(x => x > 0 ? Result<int>.Success(x * 2) : Result<int>.Failure(Error.Validation("NEG", "Negative number")))
            .Bind(x => x < 20 ? Result<int>.Success(x + 1) : Result<int>.Failure(Error.Validation("TOO_BIG", "Number too big")));
        
        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value);
    }
    
    [Fact]
    public void ResultExtensions_Map_ShouldTransformValue()
    {
        var result = Result<int>.Success(5)
            .Map(x => x.ToString())
            .Map(s => $"Value: {s}");
        
        Assert.True(result.IsSuccess);
        Assert.Equal("Value: 5", result.Value);
    }
    
    // Property-Based Tests
    
    [Property]
    public Property Result_Success_IsAlwaysSuccessful()
    {
        return Prop.ForAll<int>(value =>
        {
            var result = Result<int>.Success(value);
            return result.IsSuccess && !result.IsFailure && result.Value.Equals(value);
        });
    }
    
    [Property]
    public Property Error_CreatedWithFactoryMethods_HasCorrectType()
    {
        return Prop.ForAll(Arb.From<string>(), Arb.From<string>(), (code, message) =>
        {
            var validationError = Error.Validation(code, message);
            var notFoundError = Error.NotFound(code, message);
            var conflictError = Error.Conflict(code, message);
            
            return validationError.Type == ErrorType.Validation &&
                   notFoundError.Type == ErrorType.NotFound &&
                   conflictError.Type == ErrorType.Conflict;
        });
    }
}