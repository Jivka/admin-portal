using Microsoft.Extensions.Caching.Memory;

namespace AP.Common.Services.Contracts;

public interface ICacheService
{
    void Set<T>(string key, T value, int cacheExpirationInMinutes = 600);

    T? Get<T>(string key);

    void Remove(string key);

    Task<TItem?> GetOrCreateAsync<TItem>(object key, Func<ICacheEntry, Task<TItem>> factory);

    TItem? GetOrCreate<TItem>(object key, Func<ICacheEntry, TItem> factory);
}