using FluentValidation;
using FluentValidation.Results;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.GetService;

/// <summary>
/// FluentValidation validator for GetServiceQuery
/// Ensures medical-grade data integrity and validation standards
/// </summary>
public sealed class GetServiceQueryValidator : AbstractValidator<GetServiceQuery>
{
    public GetServiceQueryValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotNull()
            .WithMessage("Service ID is required")
            .WithErrorCode("GETSERVICE_001");
            
        RuleFor(x => x.ServiceId.Value)
            .NotEqual(Guid.Empty)
            .WithMessage("Service ID cannot be empty")
            .WithErrorCode("GETSERVICE_002")
            .When(x => x.ServiceId != null);
    }
}

/// <summary>
/// Validation extensions for GetService feature
/// </summary>
public static class GetServiceValidationExtensions
{
    /// <summary>
    /// Validates ServiceId format and constraints
    /// </summary>
    public static bool IsValidServiceId(this Guid serviceId)
    {
        return serviceId != Guid.Empty;
    }
    
    /// <summary>
    /// Validates query parameters for medical compliance
    /// </summary>
    public static ValidationResult ValidateForMedicalCompliance(this GetServiceQuery query)
    {
        var validator = new GetServiceQueryValidator();
        return validator.Validate(query);
    }
}
