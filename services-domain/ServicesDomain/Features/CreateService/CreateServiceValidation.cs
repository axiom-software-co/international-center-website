using FluentValidation;

namespace ServicesDomain.Features.CreateService;

/// <summary>
/// FluentValidation validator for CreateServiceCommand
/// Ensures all business rules are met for medical-grade service creation
/// </summary>
public sealed class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .WithMessage("Service title is required");
            
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title.Value)
                .NotEmpty()
                .WithMessage("Service title cannot be empty")
                .Length(3, 255)
                .WithMessage("Service title must be between 3 and 255 characters");
        });

        RuleFor(x => x.Description)
            .NotNull()
            .WithMessage("Service description is required");
            
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description.Value)
                .NotEmpty()
                .WithMessage("Service description cannot be empty")
                .Length(10, 500)
                .WithMessage("Service description must be between 10 and 500 characters");
        });

        RuleFor(x => x.DeliveryMode)
            .NotNull()
            .WithMessage("Delivery mode is required");
            
        When(x => x.Slug != null, () =>
        {
            RuleFor(x => x.Slug!.Value)
                .NotEmpty()
                .WithMessage("Service slug cannot be empty when provided")
                .Length(3, 255)
                .WithMessage("Service slug must be between 3 and 255 characters")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Service slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen");
        });

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required for audit trail")
            .Length(1, 255)
            .WithMessage("User ID must be between 1 and 255 characters");
    }
}
