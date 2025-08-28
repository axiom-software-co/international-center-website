using ServicesDomain.Features.GetService;

namespace ServicesDomain.Shared.Extensions;

public static class WebApplicationExtensions 
{ 
    public static WebApplication UseServicesDomainPipeline(this WebApplication app)
    {
        // Basic pipeline configuration - minimal implementation for compilation
        app.UseRouting();
        
        // Map GetService endpoints - TDD GREEN phase
        app.MapGetServiceEndpoints();
        
        return app;
    }
}
