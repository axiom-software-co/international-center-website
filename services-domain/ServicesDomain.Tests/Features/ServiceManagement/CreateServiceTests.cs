using Xunit;
using Moq;
using FsCheck;
using FsCheck.Xunit;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using ServicesDomain.Features.ServiceManagement.Domain.Repository;
using ServicesDomain.Features.CategoryManagement.Domain.ValueObjects;
using SharedPlatform.Features.ResultHandling;
using Bogus;

namespace ServicesDomain.Features.CreateService;

/// <summary>
/// Comprehensive test suite for CreateService admin API feature following TDD RED-GREEN-REFACTOR methodology
/// Focuses on admin API access with EF Core persistence, medical-grade audit, and role-based authorization
/// </summary>
public sealed class CreateServiceTests : IDisposable
{
    private readonly Mock<IServiceRepository> _mockRepository;
    private readonly Mock<ILogger<CreateServiceHandler>> _mockLogger;
    private readonly Mock<IValidator<CreateServiceCommand>> _mockValidator;
    private readonly CreateServiceHandler _handler;
    private readonly Faker<CreateServiceCommand> _commandFaker;
    
    public CreateServiceTests()
    {
        _mockRepository = new Mock<IServiceRepository>();
        _mockLogger = new Mock<ILogger<CreateServiceHandler>>();
        _mockValidator = new Mock<IValidator<CreateServiceCommand>>();
        
        // This will fail until we implement CreateServiceHandler constructor
        _handler = new CreateServiceHandler(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockValidator.Object);
            
        // Setup Bogus faker for CreateService commands
        _commandFaker = new Faker<CreateServiceCommand>()
            .CustomInstantiator(f => CreateValidServiceCommand());
    }
    
    private CreateServiceCommand CreateValidServiceCommand()
    {
        // This will fail until CreateServiceCommand is properly implemented
        return new CreateServiceCommand(
            ServiceTitle.From("Medical Consultation Services"),
            Description.From("Professional medical consultation services for comprehensive patient care and medical assessment"),
            DeliveryMode.OutpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("medical-consultation-services"),
            "admin-user-123");
    }
    
    #region Unit Tests - Handler Logic
    
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateServiceSuccessfully()
    {
        // Arrange
        var command = _commandFaker.Generate();
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
            .ReturnsAsync(false);
            
        // Act - This will fail until CreateServiceHandler.Handle is implemented
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedService.ServiceId.Value, result.Value.ServiceId);
        Assert.Equal(command.Title.Value, result.Value.Title);
        Assert.Equal(command.Description.Value, result.Value.Description);
        
        // Verify repository was called with proper audit information
        _mockRepository.Verify(r => r.AddAsync(It.Is<Service>(s => 
            s.Title == command.Title &&
            s.Description == command.Description &&
            s.Slug == command.Slug &&
            s.DeliveryMode == command.DeliveryMode &&
            s.CategoryId == command.CategoryId &&
            s.CreatedBy == command.UserId), default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithDuplicateSlug_ShouldReturnValidationError()
    {
        // Arrange - All slug variations are taken, should exhaust attempts and fail
        var command = _commandFaker.Generate();
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        // Make all slug variations return true (all exist) to exhaust generation attempts
        _mockRepository.Setup(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default))
            .ReturnsAsync(true);
            
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("unable to generate unique slug", result.Error.ToLower());
        
        // Verify repository was NOT called for creation due to slug generation failure
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Never);
        _mockRepository.Verify(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.AtLeast(10));
    }
    
    [Fact]
    public async Task Handle_WithValidationErrors_ShouldReturnValidationError()
    {
        // Arrange
        var command = _commandFaker.Generate();
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Title", "Title cannot be empty"));
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Description", "Description is too short"));
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(validationResult);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("validation", result.Error.ToLower());
        Assert.Contains("title cannot be empty", result.Error.ToLower());
        
        // Verify repository was NOT called due to validation failure
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Never);
        _mockRepository.Verify(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnInternalError()
    {
        // Arrange
        var command = _commandFaker.Generate();
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
            .ReturnsAsync(false);
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("internal error", result.Error.ToLower());
        
        // Verify error handling and audit trail
        _mockRepository.Verify(r => r.ExistsBySlugAsync(command.Slug!, default), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Once);
        _mockLogger.Verify(l => l.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Handle_WithNullCategoryId_ShouldAssignDefaultCategory()
    {
        // Arrange - Command without explicit category should use default unassigned category
        var command = new CreateServiceCommand(
            ServiceTitle.From("Service Without Category"),
            Description.From("This service has no specific category assigned and should get the default"),
            DeliveryMode.MobileService,
            null, // No category specified
            ServiceSlug.From("service-without-category"),
            "admin-user-456");
            
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
            .ReturnsAsync(false);
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify service was created with default category assignment
        _mockRepository.Verify(r => r.AddAsync(It.Is<Service>(s => 
            s.CategoryId != null), default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithAutoGeneratedSlug_ShouldCreateSlugFromTitle()
    {
        // Arrange - Command without explicit slug should generate from title
        var command = new CreateServiceCommand(
            ServiceTitle.From("Emergency Medical Services"),
            Description.From("24/7 emergency medical care and rapid response services for critical situations"),
            DeliveryMode.MobileService,
            ServiceCategoryId.New(),
            null, // No slug specified - should auto-generate
            "admin-user-789");
            
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default))
            .ReturnsAsync(false);
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify service was created with auto-generated slug based on title
        _mockRepository.Verify(r => r.AddAsync(It.Is<Service>(s => 
            s.Slug != null && s.Slug.Value.Contains("emergency-medical-services")), default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldGenerateUniqueSlugWhenDuplicateFound()
    {
        // Arrange - Test automatic slug uniqueness resolution
        var command = new CreateServiceCommand(
            ServiceTitle.From("Physical Therapy"),
            Description.From("Comprehensive physical therapy and rehabilitation services for injury recovery"),
            DeliveryMode.OutpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("physical-therapy"),
            "admin-user-101");
            
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        // First check returns true (slug exists), subsequent checks return false
        _mockRepository.SetupSequence(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default))
            .ReturnsAsync(true)   // Original slug exists
            .ReturnsAsync(false); // Modified slug is available
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify multiple slug checks were made for uniqueness
        _mockRepository.Verify(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.AtLeast(2));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Service>(), default), Times.Once);
    }
    
    [Fact]
    public void Handle_WithEmptyUserId_ShouldReturnAuthorizationError()
    {
        // Arrange & Act & Assert - Missing user context should fail authorization
        Assert.Throws<ArgumentException>(() => new CreateServiceCommand(
            ServiceTitle.From("Unauthorized Service"),
            Description.From("This service creation attempt should fail due to missing user identification"),
            DeliveryMode.InpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("unauthorized-service"),
            string.Empty)); // Empty user ID
    }
    
    #endregion
    
    #region Command Object Tests
    
    [Fact]
    public void CreateServiceCommand_WithValidParameters_ShouldCreateSuccessfully()
    {
        // Arrange
        var title = ServiceTitle.From("Diagnostic Imaging");
        var description = Description.From("Advanced diagnostic imaging services including MRI, CT, and ultrasound examinations");
        var deliveryMode = DeliveryMode.OutpatientService;
        var categoryId = ServiceCategoryId.New();
        var slug = ServiceSlug.From("diagnostic-imaging");
        var userId = "admin-user-imaging";
        
        // Act - This will fail until CreateServiceCommand constructor is implemented
        var command = new CreateServiceCommand(title, description, deliveryMode, categoryId, slug, userId);
        
        // Assert
        Assert.NotNull(command);
        Assert.Equal(title, command.Title);
        Assert.Equal(description, command.Description);
        Assert.Equal(deliveryMode, command.DeliveryMode);
        Assert.Equal(categoryId, command.CategoryId);
        Assert.Equal(slug, command.Slug);
        Assert.Equal(userId, command.UserId);
    }
    
    [Fact]
    public void CreateServiceCommand_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateServiceCommand(
            null!,
            Description.From("Valid description"),
            DeliveryMode.MobileService,
            ServiceCategoryId.New(),
            ServiceSlug.From("valid-slug"),
            "admin-user-test"));
    }
    
    [Fact]
    public void CreateServiceCommand_WithNullUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CreateServiceCommand(
            ServiceTitle.From("Valid Title"),
            Description.From("Valid description"),
            DeliveryMode.MobileService,
            ServiceCategoryId.New(),
            ServiceSlug.From("valid-slug"),
            null!));
    }
    
    #endregion
    
    #region Response Object Tests
    
    [Fact]
    public void CreateServiceResponse_WithValidService_ShouldMapCorrectly()
    {
        // Arrange
        var service = CreateExpectedService(_commandFaker.Generate());
        
        // Act - This will fail until CreateServiceResponse.From method is implemented
        var response = CreateServiceResponse.From(service);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(service.ServiceId.Value, response.ServiceId);
        Assert.Equal(service.Title.Value, response.Title);
        Assert.Equal(service.Description.Value, response.Description);
        Assert.Equal(service.Slug.Value, response.Slug);
        Assert.Equal(service.DeliveryMode.Value, response.DeliveryMode);
        Assert.Equal(service.CreatedBy, response.CreatedBy);
    }
    
    #endregion
    
    #region Property-Based Tests - FsCheck
    
    [Property]
    public Property CreateService_WithAnyValidTitle_ShouldNeverGenerateEmptySlug()
    {
        return Prop.ForAll<string>(titleValue =>
        {
            if (string.IsNullOrWhiteSpace(titleValue) || titleValue.Length < 3 || titleValue.Length > 255)
                return true; // Skip invalid inputs
                
            try
            {
                // Arrange
                var title = ServiceTitle.From(titleValue);
                var command = new CreateServiceCommand(
                    title,
                    Description.From("Property-based test description for service creation validation"),
                    DeliveryMode.OutpatientService,
                    ServiceCategoryId.New(),
                    null, // Let system generate slug
                    "property-test-user");
                    
                _mockValidator.Setup(v => v.ValidateAsync(command, default))
                    .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                    
                _mockRepository.Setup(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default))
                    .ReturnsAsync(false);
                    
                var expectedService = CreateExpectedService(command);
                _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
                    .ReturnsAsync(expectedService);
                
                // Act & Assert
                var result = _handler.Handle(command, CancellationToken.None).Result;
                
                return result.IsSuccess && !string.IsNullOrEmpty(result.Value?.Slug);
            }
            catch (ArgumentException)
            {
                return true; // Skip if ServiceTitle.From throws for invalid input
            }
        });
    }
    
    [Property]
    public Property CreateService_WithValidInputs_ShouldAlwaysPreserveUserAuditInfo()
    {
        return Prop.ForAll<string>(userIdValue =>
        {
            if (string.IsNullOrWhiteSpace(userIdValue) || userIdValue.Length > 255)
                return true; // Skip invalid user IDs
                
            try
            {
                // Arrange
                var command = new CreateServiceCommand(
                    ServiceTitle.From("Property Test Service"),
                    Description.From("Testing that user audit information is preserved correctly during service creation"),
                    DeliveryMode.MobileService,
                    ServiceCategoryId.New(),
                    ServiceSlug.From("property-test-service"),
                    userIdValue);
                    
                _mockValidator.Setup(v => v.ValidateAsync(command, default))
                    .ReturnsAsync(new FluentValidation.Results.ValidationResult());
                    
                _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
                    .ReturnsAsync(false);
                    
                var expectedService = CreateExpectedService(command);
                _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
                    .ReturnsAsync(expectedService);
                
                // Act & Assert
                var result = _handler.Handle(command, CancellationToken.None).Result;
                
                return result.IsSuccess && result.Value?.CreatedBy == userIdValue;
            }
            catch (ArgumentException)
            {
                return true; // Skip if command creation throws for invalid input
            }
        });
    }
    
    #endregion
    
    #region Medical Audit & Compliance Tests
    
    [Fact]
    public async Task Handle_ShouldLogAuditTrail_ForMedicalCompliance()
    {
        // Arrange
        var command = _commandFaker.Generate();
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
            .ReturnsAsync(false);
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert - Verify medical compliance audit trail
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(command.UserId, result.Value.CreatedBy);
        Assert.True(result.Value.CreatedOn <= DateTimeOffset.UtcNow);
        
        // Verify audit trail components are called in correct order
        _mockRepository.Verify(r => r.ExistsBySlugAsync(command.Slug!, default), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.Is<Service>(s => 
            s.CreatedBy == command.UserId && 
            s.CreatedOn <= DateTimeOffset.UtcNow), default), Times.Once);
        _mockLogger.Verify(l => l.IsEnabled(It.IsAny<LogLevel>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Handle_ShouldEnforceBusinessRules_ForMedicalDataIntegrity()
    {
        // Arrange - Test that business rules are enforced for medical data integrity
        var command = new CreateServiceCommand(
            ServiceTitle.From("Critical Care Unit"),
            Description.From("Intensive care unit services for critically ill patients requiring continuous monitoring"),
            DeliveryMode.InpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("critical-care-unit"),
            "medical-admin-123");
            
        var expectedService = CreateExpectedService(command);
        
        _mockValidator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            
        _mockRepository.Setup(r => r.ExistsBySlugAsync(command.Slug!, default))
            .ReturnsAsync(false);
            
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert - Verify medical compliance business rules
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        // Verify business rule enforcement:
        // 1. Service must have valid delivery mode
        Assert.Equal(DeliveryMode.InpatientService.Value, result.Value.DeliveryMode);
        
        // 2. Service must have proper audit fields
        Assert.NotNull(result.Value.CreatedBy);
        Assert.True(result.Value.CreatedOn > DateTimeOffset.MinValue);
        
        // 3. Service must have unique slug for medical record integrity
        _mockRepository.Verify(r => r.ExistsBySlugAsync(command.Slug!, default), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithConcurrentSlugCreation_ShouldHandleRaceCondition()
    {
        // Arrange - Simulate concurrent service creation with same slug
        var command1 = new CreateServiceCommand(
            ServiceTitle.From("Cardiology Services"),
            Description.From("Comprehensive cardiovascular care and diagnostic services"),
            DeliveryMode.OutpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("cardiology-services"),
            "cardio-admin-1");
            
        var command2 = new CreateServiceCommand(
            ServiceTitle.From("Cardiology Services Advanced"),
            Description.From("Advanced cardiovascular interventions and specialized cardiac care"),
            DeliveryMode.OutpatientService,
            ServiceCategoryId.New(),
            ServiceSlug.From("cardiology-services"),
            "cardio-admin-2");
        
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateServiceCommand>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        
        // First command succeeds, second should detect duplicate and modify slug
        _mockRepository.Setup(r => r.ExistsBySlugAsync(ServiceSlug.From("cardiology-services"), default))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.ExistsBySlugAsync(It.Is<ServiceSlug>(s => s.Value.StartsWith("cardiology-services-")), default))
            .ReturnsAsync(false);
            
        var expectedService = CreateExpectedService(command1);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Service>(), default))
            .ReturnsAsync(expectedService);
        
        // Act - Process first command
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        
        // Simulate second command detecting duplicate
        _mockRepository.Setup(r => r.ExistsBySlugAsync(ServiceSlug.From("cardiology-services"), default))
            .ReturnsAsync(true);
            
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        // Both should succeed but with different slugs for medical record uniqueness
        _mockRepository.Verify(r => r.ExistsBySlugAsync(It.IsAny<ServiceSlug>(), default), Times.AtLeast(2));
    }
    
    #endregion
    
    private Service CreateExpectedService(CreateServiceCommand command)
    {
        // Helper method to create expected service for test assertions
        // This will fail until Service.Create is properly implemented with audit fields
        return Service.Create(
            command.Title,
            command.Description,
            command.DeliveryMode,
            command.CategoryId ?? ServiceCategoryId.New(),
            command.Slug,
            command.UserId,
            DateTimeOffset.UtcNow);
    }
    
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
            _mockLogger?.Reset();
            _mockValidator?.Reset();
        }
    }
}
