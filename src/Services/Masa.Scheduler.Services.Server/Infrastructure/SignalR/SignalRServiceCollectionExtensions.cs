// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.SignalR;

public static class SignalRServiceCollectionExtensions
{
    public static IServiceCollection AddMasaSignalR(this IServiceCollection services, RedisConfigurationOptions options)
    {
        services.AddTransient<IUserIdProvider, MasaUserIdProvider>();
        services.AddTransient<NotificationsHub>();
        services.AddScoped<SignalRUtils>();
        services.AddSignalR().AddStackExchangeRedis(config =>
        {
            config.Configuration = new StackExchange.Redis.ConfigurationOptions()
            {
                AbortOnConnectFail = options.AbortOnConnectFail,
                AllowAdmin = options.AllowAdmin,
                ChannelPrefix = options.ChannelPrefix,
                ClientName = options.ClientName,
                ConnectRetry = options.ConnectRetry,
                ConnectTimeout = options.ConnectTimeout,
                DefaultDatabase = options.DefaultDatabase,
                Password = options.Password,
                Proxy = options.Proxy,
                Ssl = options.Ssl,
                SyncTimeout = options.SyncTimeout
            };

            config.Configuration.ChannelPrefix = "masa-scheduler";

            foreach (var server in options.Servers)
            {
                config.Configuration.EndPoints.Add(server.Host, server.Port);
            }
        });

        return services;
    }
}
