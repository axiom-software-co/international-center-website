using ServicesDomain.Features.ServiceManagement.Domain.Entities;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;

namespace ServicesDomain.Features.ServiceManagement.Domain.BusinessRules;

public static class ServiceArchivalRules
{
    public static bool CanBeArchived(Service service)
    {
        return service.Status != ServiceStatusType.Deleted && 
               service.Status != ServiceStatusType.Archived;
    }

    public static bool CanBeRestored(Service service)
    {
        return service.Status == ServiceStatusType.Archived;
    }

    public static DateTime CalculateArchivalDate(Service service, DateTime? requestedDate = null)
    {
        var now = DateTime.UtcNow;
        
        if (requestedDate == null)
            return now;

        // Cannot archive in the future for more than 30 days
        if (requestedDate > now.AddDays(30))
            return now.AddDays(30);

        // Cannot archive in the past
        if (requestedDate < now)
            return now;

        return requestedDate.Value;
    }

    public static string[] GetArchivalBlockers(Service service)
    {
        var blockers = new List<string>();

        if (service.Status == ServiceStatusType.Deleted)
            blockers.Add("Service is already deleted");

        if (service.Status == ServiceStatusType.Archived)
            blockers.Add("Service is already archived");

        return blockers.ToArray();
    }

    public static bool RequiresConfirmation(Service service)
    {
        return service.Status == ServiceStatusType.Published ||
               service.IsFeatured;
    }
}