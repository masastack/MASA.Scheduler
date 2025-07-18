namespace Masa.Scheduler.Infrastructure.Cache;

internal class CacheContext : ICacheContext
{
    readonly IDistributedCacheClient _distributedCache;

    public CacheContext(
        IDistributedCacheClient distributedCache)
    {
        _distributedCache = distributedCache;
    }


    public async Task SetAsync<T>(string key, T item, CacheEntryOptions? cacheEntryOptions)
    {
        await _distributedCache.SetAsync(key, item, cacheEntryOptions);
    }


    public async Task<T?> GetAsync<T>(string key)
    {
        return await _distributedCache.GetAsync<T>(key);
    }

    public async Task Remove<T>(string key)
    {
       await _distributedCache.RemoveAsync<T>(key);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> setter, CacheEntryOptions? cacheEntryOptions)
    {
        var value = await _distributedCache.GetAsync<T>(key);

        if (value != null)
            return value;

        value = await setter();

        await _distributedCache.SetAsync(key, value, cacheEntryOptions);

        return value;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<(T, CacheEntryOptions cacheEntryOptions)>> setter)
    {
        var value = await _distributedCache.GetAsync<T>(key);

        if (value != null)
            return value;

        var setterResult = await setter();
        value = setterResult.Item1;
        var cacheEntryOptions = setterResult.Item2;

        await _distributedCache.SetAsync(key, value, cacheEntryOptions);

        return value;
    }
}
