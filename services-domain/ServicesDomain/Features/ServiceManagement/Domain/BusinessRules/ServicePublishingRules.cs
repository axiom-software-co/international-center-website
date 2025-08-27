using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServicePublishingRules
{
    public static bool CanBePublished(Service service)
    {
        return service.Status != ServiceStatusType.Deleted &&
               service.Status != ServiceStatusType.Archived &&
               !string.IsNullOrWhiteSpace(service.Title) &&
               !string.IsNullOrWhiteSpace(service.Description);
    }

    public static bool CanBeUnpublished(Service service)
    {
        return service.Status == ServiceStatusType.Published;
    }

    public static bool RequiresApproval(Service service)
    {
        // Business rule: Services with external URLs require approval
        return !string.IsNullOrWhiteSpace(service.ExternalUrl);
    }

    public static bool IsReadyForPublishing(Service service)
    {
        return CanBePublished(service) && 
               HasRequiredFields(service) && 
               HasValidContent(service);
    }

    public static bool HasRequiredFields(Service service)
    {
        return !string.IsNullOrWhiteSpace(service.Title) &&
               !string.IsNullOrWhiteSpace(service.Description) &&
               !string.IsNullOrWhiteSpace(service.Slug);
    }

    public static bool HasValidContent(Service service)
    {
        return service.Title.Value.Length >= ServiceTitle.MinLength &&
               service.Title.Value.Length <= ServiceTitle.MaxLength &&
               service.Description.Value.Length >= ServiceDescription.MinLength &&
               service.Description.Value.Length <= ServiceDescription.MaxLength;
    }

    public static string[] GetPublishingBlockers(Service service)
    {
        var blockers = new List<string>();

        if (service.Status == ServiceStatusType.Deleted)
            blockers.Add("Service is deleted");

        if (service.Status == ServiceStatusType.Archived)
            blockers.Add("Service is archived");

        if (string.IsNullOrWhiteSpace(service.Title))
            blockers.Add("Title is required");

        if (string.IsNullOrWhiteSpace(service.Description))
            blockers.Add("Description is required");

        if (!HasValidContent(service))
            blockers.Add("Content does not meet length requirements");

        return blockers.ToArray();
    }

    public static DateTime CalculatePublishDate(Service service, DateTime? requestedDate = null)
    {
        var now = DateTime.UtcNow;
        
        // If no specific date requested, publish immediately
        if (requestedDate == null)
            return now;

        // Cannot publish in the past
        if (requestedDate < now)
            return now;

        return requestedDate.Value;
    }
}