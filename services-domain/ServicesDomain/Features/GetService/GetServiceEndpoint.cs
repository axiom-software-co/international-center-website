using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using ServicesDomain.Features.ServiceManagement.Domain.ValueObjects;
using SharedPlatform.Features.ResultHandling;

namespace ServicesDomain.Features.GetService;

/// <summary>
/// Minimal API endpoint for GetService feature
/// Supports both Public and Admin API access patterns
/// </summary>
public static class GetServiceEndpoint
{
    /// <summary>
    /// Maps GetService endpoints to the application
    /// TDD GREEN phase - minimal implementation
    /// </summary>
    public static void MapGetServiceEndpoints(this IEndpointRouteBuilder app)
    {
        // Public API endpoint - /api/services/{id}
        app.MapGet("/api/services/{id:guid}", GetServiceByIdAsync)
            .WithName("GetServiceById")
            .WithTags("Services")
            .WithOpenApi()
            .Produces<GetServiceResponse>(200)
            .Produces(404)
            .Produces(400);
            
        // Admin API endpoint - /admin/services/{id} 
        app.MapGet("/admin/services/{id:guid}", GetServiceByIdAsync)
            .WithName("AdminGetServiceById")
            .WithTags("Admin-Services")
            .WithOpenApi()
            .Produces<GetServiceResponse>(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization(); // Admin requires auth
    }
    
    /// <summary>
    /// Handles GET request for service by ID
    /// TDD GREEN phase - minimal implementation to make tests pass
    /// </summary>
    private static async Task<Results<Ok<GetServiceResponse>, NotFound, BadRequest<string>>> GetServiceByIdAsync(
        Guid id,
        [FromServices] GetServiceHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate GUID
            if (id == Guid.Empty)
            {
                return TypedResults.BadRequest("Service ID cannot be empty");
            }
            
            // Create query
            var query = GetServiceQuery.FromGuid(id);
            
            // Handle request
            var result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                return TypedResults.Ok(result.Value);
            }
            
            // Map error types to HTTP status codes
            return result.Error.Code switch
            {
                "NOT_FOUND" => TypedResults.NotFound(),
                "VALIDATION_ERROR" => TypedResults.BadRequest(result.Error.Message),
                _ => TypedResults.BadRequest($"An error occurred: {result.Error.Message}")
            };
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (InvalidOperationException)
        {
            // Handler processing error  
            return TypedResults.BadRequest("An internal error occurred");
        }
        catch (TaskCanceledException)
        {
            // Request cancelled
            return TypedResults.BadRequest("Request was cancelled");
        }
    }
}
