namespace ApiGateway.Features.RateLimiting;

public class RedisRateLimitStore
{
    private readonly Dictionary<string, (int Count, DateTime WindowStart)> _store = new();
    private readonly object _lock = new();

    public bool IsConnected => true;

    public Task<int> GetRequestCountAsync(string key, TimeSpan windowSize)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(key, out var data))
            {
                return Task.FromResult(0);
            }

            // Check if window has expired
            if (DateTime.UtcNow - data.WindowStart > windowSize)
            {
                _store.Remove(key);
                return Task.FromResult(0);
            }

            return Task.FromResult(data.Count);
        }
    }

    public Task IncrementRequestCountAsync(string key, TimeSpan windowSize)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            if (_store.TryGetValue(key, out var data))
            {
                // Check if window has expired
                if (now - data.WindowStart > windowSize)
                {
                    _store[key] = (1, now);
                }
                else
                {
                    _store[key] = (data.Count + 1, data.WindowStart);
                }
            }
            else
            {
                _store[key] = (1, now);
            }
        }

        return Task.CompletedTask;
    }
}
