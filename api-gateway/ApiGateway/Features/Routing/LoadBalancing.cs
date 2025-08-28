namespace ApiGateway.Features.Routing;

public class LoadBalancing
{
    private int _counter;
    
    public string SelectDestination(string[] destinations)
    {
        if (destinations.Length == 0) return string.Empty;
        var index = Interlocked.Increment(ref _counter) % destinations.Length;
        return destinations[index];
    }
}
