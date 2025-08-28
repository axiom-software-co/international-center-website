using FluentValidation;

namespace ServicesDomain.Features.GetServiceBySlug;

/// <summary>
/// FluentValidation validator for GetServiceBySlugQuery
/// Ensures slug format meets URL-friendly requirements for public API
/// </summary>
public sealed class GetServiceBySlugValidator : AbstractValidator<GetServiceBySlugQuery>
{
    public GetServiceBySlugValidator()
    {
        RuleFor(x => x.ServiceSlug)
            .NotNull()
            .WithMessage("ServiceSlug is required for service lookup");
            
        When(x => x.ServiceSlug != null, () =>
        {
            RuleFor(x => x.ServiceSlug.Value)
                .NotEmpty()
                .WithMessage("ServiceSlug cannot be empty")
                .Length(3, 255)
                .WithMessage("ServiceSlug must be between 3 and 255 characters")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("ServiceSlug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen");
        });
    }
}
