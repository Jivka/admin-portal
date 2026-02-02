using Microsoft.Extensions.Caching.Memory;
using AP.Common.Services.Contracts;

namespace AP.Common.Services;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
    public T? Get<T>(string key)
    {
        return memoryCache.Get<T?>(key);
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
    }

    public void Set<T>(string key, T value, int cacheExpirationInMinutes = 600)
    {
        memoryCache.Set(key, value, TimeSpan.FromMinutes(cacheExpirationInMinutes));
    }

    public async Task<TItem?> GetOrCreateAsync<TItem>(object key, Func<ICacheEntry, Task<TItem>> factory)
    {
        return await memoryCache.GetOrCreateAsync(key, factory);
    }

    public TItem? GetOrCreate<TItem>(object key, Func<ICacheEntry, TItem> factory)
    {
        return memoryCache.GetOrCreate(key, factory);
    }
}