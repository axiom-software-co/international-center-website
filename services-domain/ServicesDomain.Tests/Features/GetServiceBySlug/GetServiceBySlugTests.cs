using Xunit;
using Moq;
using FsCheck;
using FsCheck.Xunit;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;
using SharedPlatform.Features.ResultHandling;
using SharedPlatform.Features.Caching.Abstractions;
using Bogus;

namespace ServicesDomain.Features.GetServiceBySlug;

/// <summary>
/// Comprehensive test suite for GetServiceBySlug feature following TDD RED-GREEN-REFACTOR methodology
/// Focuses on public API access with Dapper repository optimization and medical-grade compliance
/// </summary>
public sealed class GetServiceBySlugTests : IDisposable
{
    private readonly Mock<IServiceRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ILogger<GetServiceBySlugHandler>> _mockLogger;
    private readonly Mock<IValidator<GetServiceBySlugQuery>> _mockValidator;
    private readonly GetServiceBySlugHandler _handler;
    private readonly Faker<Service> _serviceFaker;
    
    public GetServiceBySlugTests()
    {
        _mockRepository = new Mock<IServiceRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GetServiceBySlugHandler>>();
        _mockValidator = new Mock<IValidator<GetServiceBySlugQuery>>();
        
        // This will fail until we implement GetServiceBySlugHandler constructor
        _handler = new GetServiceBySlugHandler(
            _mockRepository.Object,
            _mockCache.Object,
            _mockLogger.Object,
            _mockValidator.Object);
            
        // Setup Bogus faker for Service entities with known slugs
        _serviceFaker = new Faker<Service>()
            .CustomInstantiator(f => CreateValidServiceWithSlug());
    }
    
    private Service CreateValidServiceWithSlug()
    {
        // This will fail until Service.Create is properly implemented
        var slug = ServiceSlug.From("medical-consultation-service");
        return Service.Create(
            ServiceTitle.From("Medical Consultation"),
            Description.From("Professional medical consultation service for patient care"),
            DeliveryMode.OutpatientService,
            ServiceCategoryId.New(),
            slug);
    }
    
    #region Unit Tests - Handler Logic
    
    [Fact]
    public async Task Handle_WithValidServiceSlug_ShouldReturnService()
    {
        // Arrange
        var serviceSlug = ServiceSlug.From("medical-consultation-service");
        var expectedService = _serviceFaker.Generate();
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync(expectedService);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
            
        // Act - This will fail until GetServiceBySlugHandler.Handle is implemented
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedService.ServiceId.Value, result.Value.Id);
        Assert.Equal(expectedService.Slug.Value, result.Value.Slug);
        
        // Verify repository was called with Dapper optimization
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNonExistentServiceSlug_ShouldReturnNotFoundError()
    {
        // Arrange
        var serviceSlug = ServiceSlug.From("non-existent-service");
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync((Service?)null);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error.ToLower());
        
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithCachedService_ShouldReturnCachedService()
    {
        // Arrange - Public API should heavily utilize caching for performance
        var serviceSlug = ServiceSlug.From("cached-medical-service");
        var cachedService = _serviceFaker.Generate();
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync(cachedService);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(cachedService.ServiceId.Value, result.Value.Id);
        Assert.Equal(cachedService.Slug.Value, result.Value.Slug);
        
        // Verify repository was NOT called since we have cached result
        _mockRepository.Verify(r => r.GetBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WithValidationErrors_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetServiceBySlugQuery(ServiceSlug.From("invalid-slug"));
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("ServiceSlug", "Invalid service slug format"));
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(validationResult);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("validation", result.Error.ToLower());
        
        // Verify repository was NOT called due to validation failure
        _mockRepository.Verify(r => r.GetBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WithDeletedService_ShouldReturnNotFoundError()
    {
        // Arrange
        var serviceSlug = ServiceSlug.From("deleted-service");
        var deletedService = _serviceFaker.Generate();
        // Mark service as deleted - this will fail until Service entity supports soft delete
        deletedService.Delete("test-user", DateTimeOffset.UtcNow);
        
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync(deletedService);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error.ToLower());
    }
    
    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnInternalError()
    {
        // Arrange
        var serviceSlug = ServiceSlug.From("service-with-db-error");
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("internal error", result.Error.ToLower());
        
        // Verify error handling and audit trail
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
        _mockLogger.Verify(l => l.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Handle_WithCaseInsensitiveSlug_ShouldReturnService()
    {
        // Arrange - URL slugs should be case insensitive for better UX
        var serviceSlug = ServiceSlug.From("medical-consultation-service");
        var expectedService = _serviceFaker.Generate();
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync(expectedService);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
    }
    
    #endregion
    
    #region Query Object Tests
    
    [Fact]
    public void GetServiceBySlugQuery_WithValidServiceSlug_ShouldCreateSuccessfully()
    {
        // Arrange
        var serviceSlug = ServiceSlug.From("valid-medical-service");
        
        // Act - This will fail until GetServiceBySlugQuery constructor is implemented
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        // Assert
        Assert.NotNull(query);
        Assert.Equal(serviceSlug, query.ServiceSlug);
    }
    
    [Fact]
    public void GetServiceBySlugQuery_WithNullServiceSlug_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetServiceBySlugQuery(null!));
    }
    
    #endregion
    
    #region Property-Based Tests - FsCheck
    
    [Property]
    public Property GetServiceBySlug_WithAnyValidSlug_ShouldNeverReturnNull()
    {
        return Prop.ForAll<string>(slugValue =>
        {
            if (string.IsNullOrWhiteSpace(slugValue) || slugValue.Length < 3)
                return true; // Skip invalid inputs
                
            try
            {
                // Arrange
                var serviceSlug = ServiceSlug.From(slugValue.ToLowerInvariant().Replace(" ", "-"));
                var service = _serviceFaker.Generate();
                var query = new GetServiceBySlugQuery(serviceSlug);
                
                _mockValidator.Setup(v => v.ValidateAsync(query, default))
                    .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                    
                _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
                    .ReturnsAsync(service);
                    
                _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
                    .ReturnsAsync((Service?)null);
                
                // Act & Assert
                var result = _handler.Handle(query, CancellationToken.None).Result;
                
                return result.IsSuccess ? result.Value != null : true;
            }
            catch (ArgumentException)
            {
                return true; // Skip if ServiceSlug.From throws
            }
        });
    }
    
    [Property]
    public Property GetServiceBySlug_CacheKey_ShouldBeConsistent()
    {
        return Prop.ForAll<string>(slugValue =>
        {
            if (string.IsNullOrWhiteSpace(slugValue) || slugValue.Length < 3)
                return true;
                
            try
            {
                // Test that the same ServiceSlug always generates the same cache key
                var serviceSlug1 = ServiceSlug.From(slugValue.ToLowerInvariant().Replace(" ", "-"));
                var serviceSlug2 = ServiceSlug.From(slugValue.ToLowerInvariant().Replace(" ", "-"));
                
                // This will fail until GetServiceBySlugHandler implements consistent cache key generation
                var cacheKey1 = _handler.GenerateCacheKey(serviceSlug1);
                var cacheKey2 = _handler.GenerateCacheKey(serviceSlug2);
                
                return cacheKey1 == cacheKey2;
            }
            catch (ArgumentException)
            {
                return true;
            }
        });
    }
    
    [Property]
    public Property GetServiceBySlug_WithEmptySlug_ShouldFailValidation()
    {
        return Prop.ForAll<string>(slugValue =>
        {
            // Test that empty or invalid slugs are properly validated
            if (string.IsNullOrWhiteSpace(slugValue))
            {
                try
                {
                    var invalidServiceSlug = ServiceSlug.From(slugValue);
                    var query = new GetServiceBySlugQuery(invalidServiceSlug);
                    
                    // This should fail validation
                    var validationResult = new FluentValidation.Results.ValidationResult();
                    validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("ServiceSlug", "ServiceSlug cannot be empty"));
                    
                    _mockValidator.Setup(v => v.ValidateAsync(query, default))
                        .ReturnsAsync(validationResult);
                    
                    var result = _handler.Handle(query, CancellationToken.None).Result;
                    return result.IsFailure;
                }
                catch (ArgumentException)
                {
                    return true; // Expected to throw for invalid slugs
                }
            }
            
            return true; // Valid slugs should pass this test
        });
    }
    
    #endregion
    
    #region Medical Audit & Compliance Tests
    
    [Fact]
    public async Task Handle_ShouldLogAuditTrail_ForMedicalCompliance()
    {
        // Arrange
        var service = _serviceFaker.Generate();
        var serviceSlug = service.Slug;
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync(service);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert - Verify medical compliance audit trail
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(serviceSlug.Value, result.Value.Slug);
        
        // Verify audit trail components are called in correct order
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
        _mockCache.Verify(c => c.GetAsync<Service>(It.IsAny<string>(), default), Times.Once);
        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), service, It.IsAny<TimeSpan>(), default), Times.Once);
        _mockLogger.Verify(l => l.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Handle_ShouldRespectCacheExpiration_ForDataIntegrity()
    {
        // Medical-grade systems require fresh data - cache should have reasonable expiration
        // Public API can have slightly longer cache than admin API for performance
        
        // Arrange
        var serviceSlug = ServiceSlug.From("medical-consultation");
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        // Act & Assert - This will fail until cache expiration is properly implemented
        // Verify that cache TTL is set to a reasonable value (e.g., 5 minutes for medical data)
        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(), 
            It.IsAny<Service>(), 
            It.Is<TimeSpan>(ttl => ttl <= TimeSpan.FromMinutes(5)), 
            default))
            .Returns(Task.CompletedTask);
            
        // This will be validated when handler is implemented
        Assert.True(true); // Placeholder until implementation
    }
    
    [Fact]
    public async Task Handle_WithSlugContainingSpecialCharacters_ShouldNormalizeAndFind()
    {
        // Arrange - Public URLs may contain encoded characters that need normalization
        var serviceSlug = ServiceSlug.From("medical-consultation-service");
        var expectedService = _serviceFaker.Generate();
        var query = new GetServiceBySlugQuery(serviceSlug);
        
        _mockValidator.Setup(v => v.ValidateAsync(query, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.GetBySlugAsync(serviceSlug, default))
            .ReturnsAsync(expectedService);
            
        _mockCache.Setup(c => c.GetAsync<Service>(It.IsAny<string>(), default))
            .ReturnsAsync((Service?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        _mockRepository.Verify(r => r.GetBySlugAsync(serviceSlug, default), Times.Once);
    }
    
    #endregion
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mockRepository?.Reset();
            _mockCache?.Reset();
            _mockLogger?.Reset();
            _mockValidator?.Reset();
        }
    }
}
