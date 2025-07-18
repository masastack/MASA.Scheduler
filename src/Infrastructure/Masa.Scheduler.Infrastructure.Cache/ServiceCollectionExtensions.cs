namespace Masa.Scheduler.Infrastructure.Cache;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCache(this IServiceCollection services, RedisConfigurationOptions redisOptions)
    {
        services.AddMultilevelCache(options => options.UseStackExchangeRedisCache(redisOptions));
        services.TryAddScoped<ICacheContext, CacheContext>();
        return services;
    }
}