using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServiceValidationRules
{
    public static string[] ValidateService(Service service)
    {
        var errors = new List<string>();

        if (service == null)
        {
            errors.Add("Service cannot be null");
            return errors.ToArray();
        }

        errors.AddRange(ValidateTitle(service.Title?.Value));
        errors.AddRange(ValidateDescription(service.Description?.Value));
        errors.AddRange(ValidateSlug(service.Slug?.Value));
        errors.AddRange(ValidateStatus(service.Status?.Value));
        errors.AddRange(ValidateOrder(service.Order));
        errors.AddRange(ValidateUrls(service.ExternalUrl, service.ImageUrl));

        return errors.ToArray();
    }

    public static string[] ValidateTitle(string? title)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required");
            return errors.ToArray();
        }

        if (title.Length < ServiceTitle.MinLength)
            errors.Add($"Title must be at least {ServiceTitle.MinLength} characters");

        if (title.Length > ServiceTitle.MaxLength)
            errors.Add($"Title cannot exceed {ServiceTitle.MaxLength} characters");

        return errors.ToArray();
    }

    public static string[] ValidateDescription(string? description)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add("Description is required");
            return errors.ToArray();
        }

        if (description.Length < ServiceDescription.MinLength)
            errors.Add($"Description must be at least {ServiceDescription.MinLength} characters");

        if (description.Length > ServiceDescription.MaxLength)
            errors.Add($"Description cannot exceed {ServiceDescription.MaxLength} characters");

        return errors.ToArray();
    }

    public static string[] ValidateSlug(string? slug)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(slug))
        {
            errors.Add("Slug is required");
            return errors.ToArray();
        }

        if (slug.Length < ServiceSlug.MinLength)
            errors.Add($"Slug must be at least {ServiceSlug.MinLength} characters");

        if (slug.Length > ServiceSlug.MaxLength)
            errors.Add($"Slug cannot exceed {ServiceSlug.MaxLength} characters");

        if (!IsValidSlugFormat(slug))
            errors.Add("Slug must contain only lowercase letters, numbers, and hyphens");

        return errors.ToArray();
    }

    public static string[] ValidateStatus(ServiceStatusType? status)
    {
        var errors = new List<string>();

        if (status == null)
        {
            errors.Add("Status is required");
            return errors.ToArray();
        }

        if (!Enum.IsDefined(typeof(ServiceStatusType), status.Value))
            errors.Add("Invalid status value");

        return errors.ToArray();
    }

    public static string[] ValidateOrder(int order)
    {
        var errors = new List<string>();

        if (order < 0)
            errors.Add("Order cannot be negative");

        return errors.ToArray();
    }

    public static string[] ValidateUrls(string? externalUrl, string? imageUrl)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(externalUrl) && !Uri.IsWellFormedUriString(externalUrl, UriKind.Absolute))
            errors.Add("External URL must be a valid absolute URL");

        if (!string.IsNullOrWhiteSpace(imageUrl) && !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            errors.Add("Image URL must be a valid absolute URL");

        return errors.ToArray();
    }

    private static bool IsValidSlugFormat(string slug)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}