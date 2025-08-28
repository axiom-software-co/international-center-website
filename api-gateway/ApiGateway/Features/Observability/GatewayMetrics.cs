namespace ApiGateway.Features.Observability;

public class GatewayMetrics
{
    private readonly object _lock = new();
    private readonly List<TimeSpan> _responseTimes = new();

    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }

    public void IncrementTotalRequests()
    {
        lock (_lock)
        {
            TotalRequests++;
        }
    }

    public void IncrementSuccessfulRequests()
    {
        lock (_lock)
        {
            SuccessfulRequests++;
        }
    }

    public void IncrementFailedRequests()
    {
        lock (_lock)
        {
            FailedRequests++;
        }
    }

    public void RecordResponseTime(TimeSpan responseTime)
    {
        lock (_lock)
        {
            _responseTimes.Add(responseTime);
            CalculateAverageResponseTime();
        }
    }

    private void CalculateAverageResponseTime()
    {
        if (_responseTimes.Count == 0)
        {
            AverageResponseTime = TimeSpan.Zero;
            return;
        }

        var totalMilliseconds = _responseTimes.Sum(rt => rt.TotalMilliseconds);
        var averageMilliseconds = totalMilliseconds / _responseTimes.Count;
        AverageResponseTime = TimeSpan.FromMilliseconds(averageMilliseconds);
    }
}
