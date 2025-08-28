namespace ApiGateway.Features.Routing;

public class RouteTransformation
{
    public string TransformPath(string originalPath)
    {
        return originalPath.Replace("{id}", "{**catch-all}");
    }
}
