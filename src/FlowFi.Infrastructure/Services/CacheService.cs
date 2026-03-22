using FlowFi.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FlowFi.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var data = await _cache.GetStringAsync(key, ct);
            return data is null ? default : JsonSerializer.Deserialize<T>(data);
        }
        catch { return default; } // Degrade gracefully on Redis failure
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }, ct);
        }
        catch { } // Cache write failure is non-fatal
    }

    public async Task InvalidateAsync(string key, CancellationToken ct = default)
    {
        try { await _cache.RemoveAsync(key, ct); }
        catch { }
    }
}
