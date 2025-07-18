namespace Masa.Scheduler.Infrastructure.Cache;

public interface ICacheContext
{
    Task SetAsync<T>(string key, T item, CacheEntryOptions? cacheEntryOptions = default);

    Task<T?> GetAsync<T>(string key);

    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> setter, CacheEntryOptions? cacheEntryOptions = default);

    Task<T> GetOrSetAsync<T>(string key, Func<Task<(T, CacheEntryOptions cacheEntryOptions)>> setter);

    Task Remove<T>(string key);
}