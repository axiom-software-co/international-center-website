using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServiceBusinessRules
{
    public static bool CanPublish(Service service)
    {
        return !service.Status.IsDeleted && 
               !service.Status.IsArchived && 
               !string.IsNullOrWhiteSpace(service.Title) &&
               !string.IsNullOrWhiteSpace(service.Description);
    }

    public static bool CanArchive(Service service)
    {
        return !service.Status.IsDeleted && !service.Status.IsArchived;
    }

    public static bool CanDelete(Service service)
    {
        return !service.Status.IsDeleted;
    }

    public static bool CanUpdate(Service service)
    {
        return !service.Status.IsDeleted && !service.Status.IsArchived;
    }

    public static bool CanSetFeatured(Service service)
    {
        return service.Status.IsActive || service.Status.IsPublished;
    }

    public static bool IsSlugUnique(string slug, IEnumerable<Service> existingServices, ServiceId? excludeId = null)
    {
        return !existingServices.Any(s => 
            s.Slug.Value.Equals(slug, StringComparison.OrdinalIgnoreCase) && 
            (excludeId == null || !s.Id.Equals(excludeId)));
    }

    public static bool HasValidStatus(Service service)
    {
        return Enum.IsDefined(typeof(ServiceStatusType), service.Status.Value);
    }

    public static bool HasValidOrder(Service service)
    {
        return service.Order >= 0;
    }

    public static string[] ValidateForPublication(Service service)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(service.Title))
            errors.Add("Service must have a title");

        if (string.IsNullOrWhiteSpace(service.Description))
            errors.Add("Service must have a description");

        if (service.Status.IsDeleted)
            errors.Add("Cannot publish a deleted service");

        if (service.Status.IsArchived)
            errors.Add("Cannot publish an archived service");

        return errors.ToArray();
    }

    public static string[] ValidateForUpdate(Service service)
    {
        var errors = new List<string>();

        if (service.Status.IsDeleted)
            errors.Add("Cannot update a deleted service");

        if (service.Status.IsArchived)
            errors.Add("Cannot update an archived service");

        if (string.IsNullOrWhiteSpace(service.Title))
            errors.Add("Service title is required");

        if (string.IsNullOrWhiteSpace(service.Description))
            errors.Add("Service description is required");

        if (service.Order < 0)
            errors.Add("Service order cannot be negative");

        return errors.ToArray();
    }
}